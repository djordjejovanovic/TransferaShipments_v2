using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TransferaShipments.App.Swagger;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParameters = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile))
            .ToList();

        if (!fileParameters.Any())
            return;

        // Remove IFormFile parameters from operation parameters (they'll be in the request body)
        var parametersToRemove = operation.Parameters
            .Where(p => fileParameters.Any(fp => fp.Name == p.Name))
            .ToList();
        
        foreach (var param in parametersToRemove)
        {
            operation.Parameters.Remove(param);
        }

        // Create proper request body for file upload
        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = fileParameters.ToDictionary(
                            p => p.Name ?? "file",
                            p => new OpenApiSchema
                            {
                                Type = "string",
                                Format = "binary"
                            }
                        ),
                        Required = new HashSet<string>(fileParameters.Select(p => p.Name ?? "file"))
                    }
                }
            }
        };
    }
}
