using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Blobs;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using System.ComponentModel;
using System.Dynamic;
using Container = Microsoft.Azure.Cosmos.Container;
using Newtonsoft.Json.Converters;
//using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

namespace OrderItemsReserver3
{
    public class DeliveryOrder
    {
        private readonly ILogger<DeliveryOrder> _logger;

       
        public DeliveryOrder(ILogger<DeliveryOrder> logger)
        {
            _logger = logger;
        }

        /*
        [Function("Function2")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            try
            {
                string conn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                _logger.LogInformation("C# HTTP trigger function processed a request." + requestBody);
                return new OkObjectResult("Welcome to Azure Functions!" + requestBody);
            }
            catch(Exception e)
            {
                return new OkObjectResult("FAILURE! \\r\\n" + e.ToString());
            }
        }
        */

        /*

                [Function("Function2")]
                public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
                {
                    try
                    {
                        _logger.LogInformation("HTTP request.");

                        string conn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");


                        // read the contents of the posted data into a string
                        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();


                        string orderstr = string.Empty;
                        string responseMessage = string.Empty;

                        // use Json.NET to deserialize the posted JSON into a C# dynamic object
                        if (!string.IsNullOrWhiteSpace(requestBody))
                        {
                            dynamic data = JsonConvert.DeserializeObject(requestBody);
                            orderstr = JsonConvert.SerializeObject(data, Formatting.Indented);
                        }
                        else
                        {
                            orderstr = "{ \"datetime\" : \"" + DateTime.Now.ToString() + "\", \"content\": \"" + (requestBody ?? "") + "\"}";
                        }
                        orderstr += "\r\n";

                        byte[] tbytes = Encoding.UTF8.GetBytes(orderstr);
                        using (MemoryStream tstream = new MemoryStream(tbytes))
                        {
                            await AppendToBlob(tstream);
                        }
                        responseMessage += " You order was Saved in the Blob.";

                        return new OkObjectResult(responseMessage);
                    }
                    catch (Exception e)
                    {
                        return new BadRequestObjectResult("FAILURE! \\r\\n" + e.ToString());
                    }
                }



        static async Task AppendToBlob(MemoryStream logEntryStream)
        {
            string conn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            BlobContainerClient containerClient = new BlobContainerClient(conn, "eshoporders");
            AppendBlobClient appendBlobClient = containerClient.GetAppendBlobClient("Orders.txt");

            appendBlobClient.CreateIfNotExists();

            int maxBlockSize = appendBlobClient.AppendBlobMaxAppendBlockBytes;
            long bytesLeft = logEntryStream.Length;
            byte[] buffer = new byte[maxBlockSize];
            while (bytesLeft > 0)
            {
                int blockSize = (int)Math.Min(bytesLeft, maxBlockSize);
                int bytesRead = await logEntryStream.ReadAsync(buffer, 0, blockSize);
                using (MemoryStream memoryStream = new MemoryStream(buffer, 0, bytesRead))
                {
                    await appendBlobClient.AppendBlockAsync(memoryStream);
                }
                bytesLeft -= bytesRead;
            }
        }

                */

        [Function("SaveDeliveryOrder")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            string requestBody = string.Empty;
            try
            {
                _logger.LogInformation("HTTP request.");

                // read the contents of the posted data into a string
                requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                string responseMessage = string.Empty;
                if (!string.IsNullOrWhiteSpace(requestBody))
                {
                    CosmosClient _cosmosClient;
                    Database _database;
                    Container _container;

                    _cosmosClient = new CosmosClient("https://taskfincosmos.documents.azure.com:443/",
                        "DHP9F8RW2hwngkfApQr1V0jdBaJctSZYiogUFhwDkpkwYwoYWvOjcjt3IMybuqY3LSKBVEhAkpqtACDbOD8zvA==");
                    _database = _cosmosClient.GetDatabase("eShopDelivery");
                    _container = _database.GetContainer("Orders");

                    //dynamic data = JsonConvert.DeserializeObject(requestBody);
                    dynamic data = JsonConvert.DeserializeObject<ExpandoObject>(requestBody, new ExpandoObjectConverter());
                    data.id = data.Id.ToString();
                    double total = 0;
                    foreach(var item in data.OrderItems)
                    {
                        total += item.UnitPrice * item.Units;
                    }
                    data.FinalPrice = total;
                    await _container.CreateItemAsync(data);
                    responseMessage = "You order was Saved in the Delivery Orders. "; // + requestBody;
                    //responseMessage = "Body: " + requestBody;
                    return new OkObjectResult(responseMessage);
                }
                responseMessage = "FAILURE! Empty request";

                return new BadRequestObjectResult(responseMessage);
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult($"FAILURE! \r\n{requestBody}\\r\\n" + e.ToString());
            }
        }

    }
}
