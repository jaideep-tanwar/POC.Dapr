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
        const string statestorenosql = "statestorenosql";
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


        [HttpGet("secret")]
        public async Task<IActionResult> GetSecrets()
        {
            var secret = await _daprClient.GetBulkSecretAsync(DAPR_SECRET_STORE);
            Console.WriteLine("Get Secrets");
            return Ok(secret);
        }

        [HttpGet("blobstorage/{filename}")]
        public async Task<IActionResult> GetBlobStorageById(string filename)
        {
            var data = await _daprClient.GetStateAsync<byte[]>(storeName, filename);
            Console.WriteLine("Get Blob Storage");
            return Ok(data);
        }

        [HttpGet("cosmosdb/{id:int}")]
        public async Task<IActionResult> GetCosmosDbById(int id)
        {
            var data = await _daprClient.GetStateAsync<NoSqlDb>(statestorenosql, id.ToString());
            Console.WriteLine("Get Cosmos Db");
            return Ok(data);
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
            //var secret = await _daprClient.GetBulkSecretAsync(DAPR_SECRET_STORE);
            //var secretPicValue = secret.Values.ToList();
            //var sep = secretPicValue[0];
            //Console.WriteLine($"Fetched Secret: {sep}");
            //Secret Store End

            MemoryStream ms = new();
            user.MyFile.CopyTo(ms);
            //ms.Position = 0;
            user.ImageBytes = ms.ToArray();
            var imageBytesURL = Convert.ToBase64String(user.ImageBytes);
            Image image = Image.FromStream(ms, true, true);

            // Start Save image in local folder

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

            // End Save image in local folder

            byte[] b;
            using (BinaryReader br = new BinaryReader(user.MyFile.OpenReadStream()))
            {
                b = br.ReadBytes((int)user.MyFile.OpenReadStream().Length);
                // Convert the image in to bytes
            }

            Console.WriteLine(user.Id.ToString() + "||" + user.Id.ToString());
            Dictionary<string, string> dc = new Dictionary<string, string>();
            dc.Add("ContentType", user.MyFile.ContentType);


            await _daprClient.SaveStateAsync(storeName, user.MyFile.FileName, b, metadata: dc);


            ms.Close();
            ms.Dispose();



            Console.WriteLine("Blob Store");
            NoSqlDb noSqlDb = new NoSqlDb()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Profilepic = user.MyFile.FileName,
                ProfilePicUrl = ""
            };


            await _daprClient.SaveStateAsync<NoSqlDb>(statestorenosql, (noSqlDb.Id).ToString(), noSqlDb);

            PublishModel publishModel = new PublishModel()
            {
                Id = noSqlDb.Id,
                FirstName = noSqlDb.FirstName,
                LastName = noSqlDb.LastName,
                Profilepic = noSqlDb.Profilepic,
                ProfilePicUrl = noSqlDb.ProfilePicUrl,
                //ImageBytesURL = imageBytesURL
            };

            Console.WriteLine("CosmoDb Store" + (publishModel.Id).ToString());


            await _daprClient.PublishEventAsync("pubsub", "callazurefunction", publishModel);

            Console.WriteLine("Publish");

            return Ok();
        }

    }
}