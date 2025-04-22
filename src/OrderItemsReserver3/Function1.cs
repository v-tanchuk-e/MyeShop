using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs.Specialized;
using System.Text;
using Microsoft.Azure.Functions.Worker;


public static class Function1
{
    [FunctionName("Function1")]
    public static async Task<IActionResult> Run(
        [Microsoft.Azure.Functions.Worker.HttpTrigger(Microsoft.Azure.Functions.Worker.AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
        [Blob("eshoporders/Orders.txt", FileAccess.Write, Connection = "AzureWebJobsStorage")] AppendBlobClient cloudBlockBlob,
       // [Blob("sample-container/{rand-guid}.txt", FileAccess.Write, Connection = "AzureWebJobsStorage")] Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob cloudBlockBlob,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);

        if (data == null)
        {
            return new BadRequestObjectResult("Please pass a valid JSON in the request body");
        }
        string orderstr = JsonConvert.SerializeObject(data, Formatting.Indented);

        byte[] tbytes = Encoding.UTF8.GetBytes(orderstr);
        using (MemoryStream tstream = new MemoryStream(tbytes))
        {
            await cloudBlockBlob.AppendBlockAsync(tstream);
        }
        //await cloudBlockBlob.UploadTextAsync(requestBody);

        return new OkObjectResult("Data written to blob successfully");
    }
}
