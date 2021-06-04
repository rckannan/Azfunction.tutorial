
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Asynreplypatten
{
    public  class AsyncQueueProcesser
    {
        readonly StorageService storageService;

        public AsyncQueueProcesser(StorageService storageService)
        {
            this.storageService = storageService;
        } 

        [Function("AsyncQueueProcesser")]
        public   async Task Run(
            [ServiceBusTrigger("rithvqueue", Connection = "connectionstring")] string myQueueItem,
    int deliveryCount,
    DateTime enqueuedTimeUtc,
    string messageId, 
             FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("AsyncQueueProcesser");
            logger.LogInformation($"C# ServiceBus queue trigger function processed message: {messageId}");
            //var sdsd = Encoding.ASCII.GetString(myQueueItem);
           
            
            await storageService.SendMessageAsync(myQueueItem, messageId);

            await storageService.ReadMessageAsync( messageId);
        }
    }
}
