using ContactFunction.Models;
using ContactFunction.Services;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.Configure<DataverseSettings>(
    builder.Configuration.GetSection("DataverseSettings"));

builder.Services.AddScoped<IDataverseService, DataverseService>();

builder.Build().Run();