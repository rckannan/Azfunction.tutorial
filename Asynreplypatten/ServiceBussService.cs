using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Azure.Storage.Blobs;
using Microsoft.Azure.ServiceBus;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Asynreplypatten
{
    public class ServiceBussService
    {
        readonly ServiceBusClient serviceBusClient;
        const string _Queuename = "rithvqueue";

        public ServiceBussService(ServiceBusClient serviceBusClient)
        {
            this.serviceBusClient = serviceBusClient;
        }

        public async Task SendMessageAsync(Message requestPOCO)
        {
            // create a Service Bus client 
            
                // create a sender for the queue 
                ServiceBusSender sender = serviceBusClient.CreateSender(_Queuename);

            var messagePayload = Encoding.UTF8.GetString(requestPOCO.Body);

            //create a message that we can send
           ServiceBusMessage message = new ServiceBusMessage(messagePayload);

            // send the message
            await sender.SendMessageAsync(message);
        }
    }

    public class StorageService
    {
        readonly BlobServiceClient blobServiceClient;
        const string _continer = "rithvcont";

        public StorageService(BlobServiceClient blobServiceClient)
        {
            this.blobServiceClient = blobServiceClient;
        }

        public async Task SendMessageAsync(String myQueueItem, String messageId)
        {
            // create a Service Bus client 

            // create a sender for the queue 
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient (_continer);

            // create a message that we can send
            BlobClient blobClient = containerClient.GetBlobClient(messageId);

            MemoryStream stringInMemoryStream = new MemoryStream(ASCIIEncoding.Default.GetBytes(myQueueItem));

            await blobClient.UploadAsync(stringInMemoryStream);
        }

        public async Task ReadMessageAsync(String messageId)
        {
            // create a Service Bus client 

            // create a sender for the queue 
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_continer);

            // create a message that we can send
            BlobClient blobClient = containerClient.GetBlobClient(messageId);
             
            var obj =  await blobClient.DownloadAsync();
        }
    }
}