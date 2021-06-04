////using System.Collections.Generic;
////using System.IO;
////using System.Net;
////using Microsoft.Azure.Functions.Worker;
////using Microsoft.Azure.Functions.Worker.Http;
////using Microsoft.Azure.WebJobs;
////using Microsoft.Extensions.Logging;
////using Microsoft.WindowsAzure.Storage.Blob;


using System.IO;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.ServiceBus;
namespace Asynreplypatten
{
    public class AsyncStatusProcesser
    {
        readonly StorageService storageService;

        public AsyncStatusProcesser(StorageService storageService)
        {
            this.storageService = storageService;
        }

        [Function("AsyncStatusProcesser")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
          //[ServiceBus("outqueue", Connection = "ServiceBusConnectionAppSetting", EntityType = Microsoft.Azure.WebJobs.ServiceBus.EntityType.Queue)] out Message myQueueItem,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("Function2");
            logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return response;
        }
    }
}