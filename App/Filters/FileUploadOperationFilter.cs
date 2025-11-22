using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TransferaShipments.App.Filters
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Find parameters on the method that are either IFormFile/IFormFileCollection or have properties of those types
            var methodParams = context.MethodInfo.GetParameters();

            var fileParams = methodParams.Where(p =>
                p.ParameterType == typeof(IFormFile) ||
                p.ParameterType == typeof(IFormFileCollection) ||
                p.ParameterType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Any(pr => pr.PropertyType == typeof(IFormFile) || pr.PropertyType == typeof(IFormFileCollection))
            ).ToList();

            if (!fileParams.Any()) return;

            var multipartSchema = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>()
            };

            foreach (var p in fileParams)
            {
                if (p.ParameterType == typeof(IFormFile))
                {
                    if (!string.IsNullOrEmpty(p.Name))
                        multipartSchema.Properties[p.Name] = new OpenApiSchema { Type = "string", Format = "binary" };
                    continue;
                }

                if (p.ParameterType == typeof(IFormFileCollection))
                {
                    if (!string.IsNullOrEmpty(p.Name))
                    {
                        multipartSchema.Properties[p.Name] = new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema { Type = "string", Format = "binary" }
                        };
                    }
                    continue;
                }

                // Complex type: iterate its public instance properties
                var props = p.ParameterType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var prop in props)
                {
                    if (prop.PropertyType == typeof(IFormFile))
                    {
                        multipartSchema.Properties[prop.Name] = new OpenApiSchema { Type = "string", Format = "binary" };
                    }
                    else if (prop.PropertyType == typeof(IFormFileCollection))
                    {
                        multipartSchema.Properties[prop.Name] = new OpenApiSchema
                        {
                            Type = "array",
                            Items = new OpenApiSchema { Type = "string", Format = "binary" }
                        };
                    }
                    else
                    {
                        // Non-file scalar properties - represent as string (keeps swagger UI usable)
                        if (!multipartSchema.Properties.ContainsKey(prop.Name))
                            multipartSchema.Properties[prop.Name] = new OpenApiSchema { Type = "string" };
                    }
                }
            }

            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = multipartSchema
                    }
                }
            };

            // Remove parameters that were moved into the request body to avoid duplicates
            if (operation.Parameters != null && operation.Parameters.Count > 0)
            {
                var toRemove = operation.Parameters.Where(op => 
                    !string.IsNullOrEmpty(op.Name) && 
                    fileParams.Any(fp => !string.IsNullOrEmpty(fp.Name) && fp.Name == op.Name)).ToList();
                foreach (var r in toRemove) operation.Parameters.Remove(r);
            }
        }
    }
}
