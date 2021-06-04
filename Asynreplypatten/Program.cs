using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Threading.Tasks;

namespace Asynreplypatten
{
    public class Program
    {
        readonly static string _sbConnectionString =   Environment.GetEnvironmentVariable("connectionstring") ?? string.Empty;
        readonly static string _storageConnectionString = Environment.GetEnvironmentVariable("storageconnectionstring") ?? string.Empty;

        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults() 
                .ConfigureServices(services => {
                    services.AddSingleton<ServiceBussService>();
                    services.AddSingleton(prov => new ServiceBusClient(_sbConnectionString));

                    services.AddSingleton<StorageService>();
                    services.AddSingleton(prov => new BlobServiceClient(_storageConnectionString));
                })
                .Build();

            host.Run();
        }
    }
}