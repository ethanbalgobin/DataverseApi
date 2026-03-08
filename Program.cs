using DataverseAPI.Models;
using DataverseAPI.Services.Contacts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.PowerPlatform.Dataverse.Client;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddApplicationInsightsTelemetryWorkerService();
builder.Services.ConfigureFunctionsApplicationInsights();

builder.Services.Configure<DataverseSettings>(
    builder.Configuration.GetSection("DataverseSettings"));

builder.Services.AddSingleton(sp =>
{
    var connectionString = builder.Configuration["DataverseSettings:ConnectionString"]
                           ?? builder.Configuration["DataverseConnectionString"];

    return new ServiceClient(connectionString);
});

builder.Services.AddScoped<IContactDataverseService, ContactDataverseService>();

builder.Build().Run();