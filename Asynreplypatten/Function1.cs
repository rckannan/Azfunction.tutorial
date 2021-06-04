using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Asynreplypatten
{
    public class AsyncReqProcesser
    {
        readonly ServiceBussService serviceBussService;

        public AsyncReqProcesser(ServiceBussService serviceBussService)
        {
            this.serviceBussService = serviceBussService;
        }

        [Function("AsyncReqProcesser")]
        
        //[return: ServiceBus("rithvSBqueue", Connection = "ServiceBusConnection")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestData req,
            FunctionContext executionContext)
        { 

            RequestPOCO pp = new RequestPOCO();
            pp.Request_ID = System.Guid.NewGuid();
            pp.Request = "rithv";

            var messagePayload = JsonConvert.SerializeObject(pp);
            Message pps = new Message(Encoding.UTF8.GetBytes(messagePayload));
            pps.MessageId  = pp.Request_ID.ToString();
            pps.Label = pp.Request_ID.ToString(); 
            await serviceBussService.SendMessageAsync(pps);

            var logger = executionContext.GetLogger("AsyncReqProcesser");
            logger.LogInformation("Processed the request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Request has been processed............");

            return response;
        }
    }
}
