using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Asynreplypatten
{
    public static class AsyncOperationStatusChecker
    {
        [FunctionName("AsyncOperationStatusChecker")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "RequestStatus/{thisGUID}")] HttpRequest req,
            [Blob("data/{thisGuid}.blobdata", FileAccess.Read, Connection = "storageconnectionstring")] CloudBlockBlob inputBlob, string thisGUID,
            ILogger log)
        {

            OnCompleteEnum OnComplete = Enum.Parse<OnCompleteEnum>(req.Query["OnComplete"].FirstOrDefault() ?? "Redirect");
            OnPendingEnum OnPending = Enum.Parse<OnPendingEnum>(req.Query["OnPending"].FirstOrDefault() ?? "Accepted");

            log.LogInformation($"C# HTTP trigger function processed a request for status on {thisGUID} - OnComplete {OnComplete} - OnPending {OnPending}");

            // Check to see if the blob is present.
            if (await inputBlob.ExistsAsync())
            {
                // If it's present, depending on the value of the optional "OnComplete" parameter choose what to do.
                return await OnCompleted(OnComplete, inputBlob, thisGUID);
            }
            else
            {
                // If it's NOT present, check the optional "OnPending" parameter.
                string rqs = $"http://{Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME")}/api/RequestStatus/{thisGUID}";

                switch (OnPending)
                {
                    case OnPendingEnum.Accepted:
                        {
                            // Return an HTTP 202 status code.
                            return (ActionResult)new AcceptedResult() { Location = rqs };
                        }

                    case OnPendingEnum.Synchronous:
                        {
                            // Back off and retry. Time out if the backoff period hits one minute
                            int backoff = 250;

                            while (!await inputBlob.ExistsAsync() && backoff < 64000)
                            {
                                log.LogInformation($"Synchronous mode {thisGUID}.blob - retrying in {backoff} ms");
                                backoff = backoff * 2;
                                await Task.Delay(backoff);
                            }

                            if (await inputBlob.ExistsAsync())
                            {
                                log.LogInformation($"Synchronous Redirect mode {thisGUID}.blob - completed after {backoff} ms");
                                return await OnCompleted(OnComplete, inputBlob, thisGUID);
                            }
                            else
                            {
                                log.LogInformation($"Synchronous mode {thisGUID}.blob - NOT FOUND after timeout {backoff} ms");
                                return (ActionResult)new NotFoundResult();
                            }
                        }

                    default:
                        {
                            throw new InvalidOperationException($"Unexpected value: {OnPending}");
                        }
                }
            }
        }

        private static async Task<IActionResult> OnCompleted(OnCompleteEnum OnComplete, CloudBlockBlob inputBlob, string thisGUID)
        {
            switch (OnComplete)
            {
                case OnCompleteEnum.Redirect:
                    {
                        // Redirect to the SAS URI to blob storage
                        return (ActionResult)new RedirectResult(inputBlob.GenerateSASURI());
                    }

                case OnCompleteEnum.Stream:
                    {
                        // Download the file and return it directly to the caller.
                        // For larger files, use a stream to minimize RAM usage.
                        return (ActionResult)new OkObjectResult(await inputBlob.DownloadTextAsync());
                    }

                default:
                    {
                        throw new InvalidOperationException($"Unexpected value: {OnComplete}");
                    }
            }
        }
    }

    public enum OnCompleteEnum
    {

        Redirect,
        Stream
    }

    public enum OnPendingEnum
    {

        Accepted,
        Synchronous
    }

    public class CustomerPOCO
    {
        public string id { get; set; }
        public string customername { get; set; }
    }

    public static class CloudBlockBlobExtensions
    {
        public static string GenerateSASURI(this CloudBlockBlob blob)
        {
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(-5);
            sasConstraints.SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddMinutes(10);
            sasConstraints.Permissions = SharedAccessBlobPermissions.Read;

            //Generate the shared access signature on the blob, setting the constraints directly on the signature.
            string sasBlobToken = blob.GetSharedAccessSignature(sasConstraints);

            //Return the URI string for the container, including the SAS token.
            return blob.Uri + sasBlobToken;
        }
    }
}
