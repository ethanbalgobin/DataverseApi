using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace DataverseAPI.Configuration;

public class OpenApiConfigurationOptions : DefaultOpenApiConfigurationOptions
{
    public override OpenApiInfo Info { get; set; } = new OpenApiInfo
    {
        Title = "Dataverse API",
        Version = "1.0.0",
        Description = "RestAPI for managing Dataverse entities.",
        Contact = new OpenApiContact
        {
            Name = "Ethan Balgobin",
            Email = "ethanbalgo@hotmail.com",
            Url = new Uri("https://www.ethanbalgobin.com")
        }
    };

    public override OpenApiVersionType OpenApiVersion { get; set; } = OpenApiVersionType.V3;
}