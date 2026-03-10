using DataverseAPI.Configuration;
using DataverseAPI.Models;
using DataverseAPI.Services.Account;
using DataverseAPI.Services.Contacts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.PowerPlatform.Dataverse.Client;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddApplicationInsightsTelemetryWorkerService();
builder.Services.ConfigureFunctionsApplicationInsights();

builder.Services.AddSingleton<IOpenApiConfigurationOptions, OpenApiConfigurationOptions>();

builder.Services.Configure<DataverseSettings>(
    builder.Configuration.GetSection("DataverseSettings"));

builder.Services.AddSingleton(sp =>
{
    var connectionString = builder.Configuration["DataverseSettings:ConnectionString"]
                           ?? builder.Configuration["DataverseConnectionString"];

    return new ServiceClient(connectionString);
});

builder.Services.AddScoped<IContactDataverseService, ContactDataverseService>();
builder.Services.AddScoped<IAccountDataverseService, AccountDataverseService>();

builder.Build().Run();