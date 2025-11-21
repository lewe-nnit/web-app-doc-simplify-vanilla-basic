using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NNIT.Veeva.Documents;

var builder = FunctionsApplication.CreateBuilder(args);

// Register configuration and dependencies
builder.Services.Configure<DocumentSearchSettings>(
    builder.Configuration.GetSection("DocumentSearchSettings"));

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
