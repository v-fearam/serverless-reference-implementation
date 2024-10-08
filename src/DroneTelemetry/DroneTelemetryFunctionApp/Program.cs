using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serverless.Serialization.Models;
using Serverless.Serialization;
using DroneTelemetryFunctionApp;
using Microsoft.Azure.Cosmos;
using Azure.Identity;
using Microsoft.Extensions.Azure;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddTransient<ITelemetrySerializer<DroneState>, TelemetrySerializer<DroneState>>();
        services.AddTransient<ITelemetryProcessor, TelemetryProcessor>();
        services.AddSingleton(serviceProvider =>
        {
            return new CosmosClient(
               accountEndpoint: Environment.GetEnvironmentVariable("COSMOSDB_CONNECTION_STRING__accountEndpoint"),
               new DefaultAzureCredential()
           );
        });
        var deadLetterStorageName = Environment.GetEnvironmentVariable("DeadLetterStorageName");
        services.AddAzureClients(clientBuilder =>
        {
            clientBuilder.AddQueueServiceClient(
                new Uri($"https://{deadLetterStorageName}.queue.core.windows.net"));
            clientBuilder.UseCredential(new DefaultAzureCredential());
        });
    })
    .Build();

await host.RunAsync();
