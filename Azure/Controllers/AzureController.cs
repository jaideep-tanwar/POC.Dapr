using Azure.Model;
using Azure.Service;
using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;


namespace Azure.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AzureController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
        private readonly DaprClient _daprClient;
        private readonly ILogger<AzureController> _logger;
        const string storeName = "statestore";
        const string statestorenosql = "statestorenosql";
        private readonly IAzureService _azureService;
        //public dynamic ViewBag { get; set; }

        public AzureController(ILogger<AzureController> logger,DaprClient daprClient,IAzureService azureService)
        {
            _logger = logger;
            _daprClient = daprClient;
            _azureService = azureService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IActionResult> Get()
        {
            return Ok("Wow");
        }

        [Topic("pubsub", "callazurefunction")]
        [HttpPost("", Name = "SubmitOrder")]
        public async Task<IActionResult> Submit(PublishModel user)
        {
            Console.WriteLine("Received a new user from" + " " + user.Id + " " + user.FirstName);

            Console.WriteLine("binding start");

            string currentDirectory = Directory.GetCurrentDirectory();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");

            var files = Directory.GetFiles(path, user.Id + ".jpg");

            var filename = Path.GetFileName(files[0]);
            byte[] photo = System.IO.File.ReadAllBytes(files[0]);


            string azureBaseUrl = "http://localhost:7071/api/Function1";
            //string urlQueryStringParams = $"?name={user.ProfilePicUrl}";
            var data = new { Name = Convert.ToBase64String(photo) };
            var myContent = JsonConvert.SerializeObject(data);

            var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var result = "";
            using (HttpClient client = new HttpClient())
            {
                //for post
                var response = await client.PostAsync(azureBaseUrl, byteContent);
                result = response.Content.ReadAsStringAsync().Result;

                Console.WriteLine("response");
            }

            System.Drawing.Image imageIn = ConvertByteArrayToImage(Convert.FromBase64String(result));
            string previousDirectory = Path.GetFullPath(Path.Combine(currentDirectory, @"..\"));
            string pngpath = Path.Combine(previousDirectory, "Demo", "UploadedFiles");
            imageIn.Save(pngpath +"\\" + user.Id+".png");



            PublishModel publishModel = new PublishModel()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Profilepic = user.Profilepic,
                ProfilePicUrl = user.ProfilePicUrl,
                ImageBytesURL = ""
            };

            var httpClient1 = DaprClient.CreateInvokeHttpClient();
            await httpClient1.PostAsJsonAsync("http://demo/demo/update", publishModel);


            Console.WriteLine("binding success");

            return Ok();
        }


        public Image ConvertByteArrayToImage(byte[] byteArrayIn)
        {
            using (MemoryStream ms = new MemoryStream(byteArrayIn))
            {
                return Image.FromStream(ms);
            }
        }

        //public async Task updateBlob(int id,string image)
        //{

        //    //Delete
        //     //await _daprClient.DeleteStateAsync(storeName, id.ToString());
        //    //Insert
        //    //await _daprClient.SaveStateAsync(storeName, id.ToString(), Convert.FromBase64String(image));

        //    //state.Value = Convert.FromBase64String(image);
        //    //await state.SaveAsync();
        //    Console.WriteLine("update blob"+ id);
        //}

        //public async Task updateCosmosDb(int id,PublishModel user)
        //{
        //    //var blobUrl = Convert.FromBase64String(image);
        //    Console.WriteLine("update cosmosDb start" + (id + 1));
        //    //using var client = new DaprClientBuilder().Build();
        //    PublishModel publishModel = new PublishModel()
        //    {
        //        Id = user.Id,
        //        FirstName = "updatedcosmos",
        //        LastName = user.LastName,
        //        //Profilepic = user.Profilepic,
        //        ProfilePicUrl = user.ProfilePicUrl,
        //        //ImageBytes = user.ImageBytes

        //    };
        //    try
        //    {
        //        //var state1 = await _daprClient.GetStateEntryAsync<string>(storeName, id.ToString());

        //        var state = await _daprClient.GetStateEntryAsync<PublishModel>(statestorenosql, (id+1).ToString());
        //        state.Value = publishModel;
        //        await state.SaveAsync();
        //        Console.WriteLine("update cosmosDb running");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("exception" + ex.ToString());
        //    }
        //    Console.WriteLine("update cosmosDb" + (id+1));
        //}
    }
}