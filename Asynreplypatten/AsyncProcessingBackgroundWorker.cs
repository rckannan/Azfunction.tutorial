using System.IO;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Asynreplypatten
{
    public static class AsyncProcessingBackgroundWorker
    {
        [FunctionName("AsyncProcessingBackgroundWorker")]
        public static void Run(
            [ServiceBusTrigger("outqueue", Connection = "storageconnectionstring")] Message myQueueItem,
            [Blob("data", FileAccess.ReadWrite, Connection = "AzureWebJobsStorage")] CloudBlobContainer inputBlob,
            ILogger log)
        {
            // Perform an actual action against the blob data source for the async readers to be able to check against.
            // This is where your actual service worker processing will be performed.

            var id = myQueueItem.UserProperties["RequestGUID"] as string;

            CloudBlockBlob cbb = inputBlob.GetBlockBlobReference($"{id}.blobdata");

            // Now write the results to blob storage.
            cbb.UploadFromByteArrayAsync(myQueueItem.Body, 0, myQueueItem.Body.Length);
        }
    }
}
