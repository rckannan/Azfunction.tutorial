using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;

namespace app1
{
    public class Service1
    {
        private readonly HttpClient _client;
        private readonly IdataRepo _dataRepo;

        public Service1(HttpClient client, IdataRepo dataRepo)
        {
            _client = client;
            _dataRepo = dataRepo;
        }


        [FunctionName("Function1")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

          
            return new OkObjectResult(_dataRepo.GetMessage(name));
        }


    }

    public class Service2
    {
        private readonly HttpClient _client;
        private readonly IdataRepo _dataRepo;

        public Service2(HttpClient client, IdataRepo dataRepo)
        {
            _client = client;
            _dataRepo = dataRepo;
        }


        [FunctionName("Function2")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;


            return new OkObjectResult(_dataRepo.GetMessage(name));
        }


    }

    public interface IdataRepo
    {
        public string GetMessage(string inParam);
    }

    public class DataRepo : IdataRepo
    {
        private readonly ILogger _logger;

        public DataRepo( )
        {
           
        }

        public string GetMessage(string inParam)
        {
            //_logger.LogInformation("Processing request...");
            string responseMessage = string.IsNullOrEmpty(inParam)
              ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
              : $"Hello, {inParam}. This HTTP triggered function executed successfully.";
            //_logger.LogInformation("Processed request...");
            return responseMessage;

        }
    }
}
