using Dapr;
using Dapr.Client;
using Demo.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace Demo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DemoController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
        private readonly DaprClient _daprClient;
        private readonly ILogger<DemoController> _logger;
        const string storeName = "statestore";
        const string cosmosDbStore = "statestorecosmodb";
        const string DAPR_SECRET_STORE = "localsecretstore";
        const string SECRET_NAME = "secretPicUrl";
        //public dynamic ViewBag { get; set; }

        public DemoController(ILogger<DemoController> logger,DaprClient daprClient)
        {
            _logger = logger;
            _daprClient = daprClient;
        }

        [HttpGet("weathernew")]
        public IEnumerable<WeatherForecast> Get()
        {
            Console.WriteLine("Called Get");
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
        [HttpPost("update")]
        public async Task<IActionResult> UpdateData(PublishModelCreation user)
        {
            Console.WriteLine("Post Called"+user.Id+user.FirstName+user.LastName);
            return Ok();
        }

            [HttpPost("create")]
        public async Task<ActionResult<User>> Post([FromForm] User user)
        {
            Random random = new Random();
            user.Id = random.Next(1, 1000);
            //Secret Store start
            var secret = await _daprClient.GetSecretAsync(DAPR_SECRET_STORE, SECRET_NAME);
            var secretPicValue = secret.Values.ToList();
            var sep = secretPicValue[0];
            Console.WriteLine($"Fetched Secret: {sep}");
            //Secret Store End

            MemoryStream ms = new MemoryStream(100);
            user.MyFile.CopyTo(ms);
            user.ImageBytes = ms.ToArray();
            var imageBytesURL = Convert.ToBase64String(user.ImageBytes);

            string currentDirectory = Directory.GetCurrentDirectory();
            string previousDirectory = Path.GetFullPath(Path.Combine(currentDirectory, @"..\"));
            string path = Path.Combine(previousDirectory, "Azure", "UploadedFiles");

            FileInfo fileInfo = new FileInfo(user.MyFile.FileName);
            string fileName = user.Id + fileInfo.Extension;

            string fileNameWithPath = Path.Combine(path, fileName);

            using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
            {
                user.MyFile.CopyTo(stream);
            }
           


            
            ms.Close();
            ms.Dispose();


            Console.WriteLine(user.Id.ToString() + "||" + user.Id.ToString());

            await _daprClient.SaveStateAsync(storeName, user.Id.ToString(), user.ImageBytes);


            Console.WriteLine("Blob Store");
            PublishCosmosDb publishCosmosDb = new PublishCosmosDb()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Profilepic = "",
                ProfilePicUrl = sep + "/" + user.Id
            };

            await _daprClient.SaveStateAsync<PublishCosmosDb>(cosmosDbStore, (publishCosmosDb.Id).ToString(), publishCosmosDb);

            PublishModel publishModel = new PublishModel()
            {
                Id = publishCosmosDb.Id,
                FirstName = publishCosmosDb.FirstName,
                LastName = publishCosmosDb.LastName,
                Profilepic = publishCosmosDb.Profilepic,
                ProfilePicUrl = publishCosmosDb.ProfilePicUrl,
                //ImageBytesURL = imageBytesURL
            };

            Console.WriteLine("CosmoDb Store" + (publishModel.Id).ToString());


            await _daprClient.PublishEventAsync("pubsub", "callazurefunction", publishModel);

            Console.WriteLine("Publish");

            return Ok();
        }

    }
}