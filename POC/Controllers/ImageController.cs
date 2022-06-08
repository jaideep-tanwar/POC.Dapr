using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using POC.Model;
using System.Drawing;
using System.Drawing.Imaging;

namespace POC.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly DaprClient _daprClient;
        const string storeName = "statestore";
        const string cosmosDbStore = "statestorecosmodb";
        const string DAPR_SECRET_STORE = "localsecretstore";
        const string SECRET_NAME = "secretPicUrl";

        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<ImageController> _logger;

        public ImageController(ILogger<ImageController> logger,DaprClient daprClient)
        {
            _logger = logger;
            _daprClient = daprClient;
        }


        //[HttpGet("hello")]
        //public ActionResult Get()
        //{
        //    return Ok("Hello");
        //}

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost("create")]
        public async Task<ActionResult<User>> Post([FromForm] User user)
        {
            Random random = new Random();
            user.Id = random.Next(1, 1000);
            //Secret Store start
            var secret = await _daprClient.GetSecretAsync(DAPR_SECRET_STORE, SECRET_NAME);
            var secretPicValue = string.Join(", ", secret);
            Console.WriteLine($"Fetched Secret: {secretPicValue}");
            //Secret Store End

            MemoryStream ms = new MemoryStream(100);
                user.MyFile.CopyTo(ms);
                user.ImageBytes = ms.ToArray();
                user.ProfilePicUrl = Convert.ToBase64String(user.ImageBytes);

                ms.Close();
                ms.Dispose();


            Console.WriteLine(user.Id.ToString() +"||" + user.Id.ToString());
            var userId = "1";
            //await _daprClient.SaveStateAsync(storeName, user.Id.ToString(), user.ImageBytes);

            //var data = await _daprClient.GetStateAsync<byte[]>(storeName, user.Id.ToString());
            //FirstName,lastName,URL = userID    CosmosDB
            Console.WriteLine("Blob Store");
            PublishModel publishModel = new PublishModel()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Profilepic = "dummy",
                ProfilePicUrl = secretPicValue+"/"+user.Id,
                ImageBytes = ""

            };


            

            //await _daprClient.SaveStateAsync<PublishModel>(cosmosDbStore, (publishModel.Id+1).ToString(), publishModel);

            //PublishModel publishModel1 = new PublishModel()
            //{
            //    Id = user.Id,
            //    FirstName = "Update",
            //    LastName = user.LastName,
            //    //Profilepic = user.Profilepic,
            //    ProfilePicUrl = secretPicValue + "/" + user.Id,
            //    //ImageBytes = user.ImageBytes

            //};

            //var state = await _daprClient.GetStateEntryAsync<PublishModel>(cosmosDbStore, (user.Id).ToString());
            //state.Value = publishModel1;
            //await state.SaveAsync();
            Console.WriteLine("CosmoDb Store"+(user.Id).ToString());
            //var dap = new DaprClientBuilder().Build();
            //var result1 = dap.CreateInvokeMethodRequest(HttpMethod.Get, "azure", "updateasync");
            //Console.WriteLine("Result: " + result1);
            //await _daprClient.InvokeMethodAsync(result1);
            ////Console.WriteLine("Order requested: " + orderId);
            //Console.WriteLine("call: " + result1);

            await _daprClient.PublishEventAsync("pubsub", "callazurefunction", publishModel);
            //await _daprClient.PublishEventAsync("pubsub", "azurefunction", user);
            Console.WriteLine("Publish");

            //var result = _daprClient.CreateInvokeMethodRequest(HttpMethod.Post, "azure", "azure/SubmitOrder", publishModel);
            //Console.WriteLine("Result" + result);
            //var forecasts = await _daprClient.InvokeMethodAsync<PublishModel>(result);

            return Ok();
        }

        //[Topic("hello","")]
        //[HttpGet("hello")]
        //public ActionResult<string> Get()
        //{
        //    Console.WriteLine("Hello, World.");
        //    return "World";
        //}

    }
}