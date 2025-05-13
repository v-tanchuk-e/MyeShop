using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.ServiceBus;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Blobs;
using System.Text;

namespace OrderItemsReserver7a
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function(nameof(Function1))]
        public async Task Run(
            [ServiceBusTrigger("eshop", Connection = "ShopOrdersBus")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            try
            {
                _logger.LogInformation("Message ID: {id}", message.MessageId);
                _logger.LogInformation("Message Body: {body}", message.Body);
                _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

                string conn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");


                // read the contents of the posted data into a string
                string requestBody = message.Body.ToString();


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
               await  SendEmail(orderstr);

                //return new OkObjectResult(responseMessage);
            }
            catch (Exception e)
            {
                //return new BadRequestObjectResult("FAILURE! \\r\\n" + e.ToString());
            }

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
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

        static async Task SendEmail(string order)
        {
            var client = new HttpClient();
            // requires using System.Text.Json;
            string jsonData = JsonConvert.SerializeObject(new
            {
                Message = order
            });

            HttpResponseMessage result = await client.PostAsync(
                "https://prod-16.canadacentral.logic.azure.com:443/workflows/63e731bdbf8e4d69aed18c908721c9e6/triggers/When_a_HTTP_request_is_received/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2FWhen_a_HTTP_request_is_received%2Frun&sv=1.0&sig=m6ReRtzLyNno8zHNiCriqVw7jmmnDx7DGzia1jWc5w8",
                new StringContent(jsonData, Encoding.UTF8, "application/json"));

            var statusCode = result.StatusCode.ToString();
        }


    }
}
