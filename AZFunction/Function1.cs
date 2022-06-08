using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.IO.Compression;

namespace AZFunction
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            Console.WriteLine("Data");
            name = name ?? data?.Name;
            Console.WriteLine("Get Name" + name.Substring(0, 70));

            
            string response = "";
            byte[] image = Convert.FromBase64String(name);
            using (var ms1 = new MemoryStream(image))
            {
                using (var targetMs = new MemoryStream())
                {
                    using (Image images = Image.FromStream(ms1))
                    {
                        images.Save(targetMs, ImageFormat.Png);
                        var imageBytes1 = targetMs.ToArray();
                        response = Convert.ToBase64String(imageBytes1);
                    }
                }
            }
           
            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : response;

            return new OkObjectResult(responseMessage);
        }
    }
}
