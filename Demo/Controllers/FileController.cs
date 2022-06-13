using Dapr.Client;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Demo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly DaprClient _daprClient;
        const string storeName = "statestore";

        public FileController(DaprClient daprClient)
        {
            _daprClient = daprClient;
        }

        
        // GET: api/<BlobController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            //_daprClient.GetBulkStateAsync(storeName,);
            return new string[] { "value1", "value2" };
        }

        // GET api/<BlobController>/5
        [HttpGet("{filename}")]
        public async Task<IActionResult> Get(string filename)
        {
            var data = await _daprClient.GetStateAsync<String>(storeName, filename);
            if (data != null)
            {
                return Ok("Data Successfully get by Filename"+" "+filename);
            }
            else {
                return Ok("Something Wrong...");
            }            
        }

        // POST api/<BlobController>
        [HttpPost]
        public async Task<IActionResult> Post([FromForm] IFormFile file)
        {
            MemoryStream ms = new MemoryStream(100);
            file.CopyTo(ms);
            var imageBytes = ms.ToArray();
            var imageBytesURL = Convert.ToBase64String(imageBytes);
            Dictionary<string, string> dc = new Dictionary<string, string>();
            dc.Add("ContentType", "image/jpeg");

            await _daprClient.SaveStateAsync(storeName, file.FileName, imageBytes,metadata:dc);
            return Ok("Data Successfully Store By Name"+" "+ file.FileName);
        }

        // PUT api/<BlobController>/5
        [HttpPut("{filename}")]
        public async Task<IActionResult> Put(string filename, [FromForm] IFormFile file)
        {
            var data = await _daprClient.GetStateEntryAsync<byte[]>(storeName, filename);

            MemoryStream ms = new MemoryStream(100);
            file.CopyTo(ms);
            var imageBytes = ms.ToArray();
            var imageBytesURL = Convert.ToBase64String(imageBytes);
            Dictionary<string, string> dc = new Dictionary<string, string>();
            dc.Add("ContentType", "image/jpeg");

            data.Value = imageBytes;
            data.SaveAsync(metadata:dc);

            return Ok("Data Successfully Updated By Name" + " " + filename);
        }

        // DELETE api/<BlobController>/5
        [HttpDelete("{filename}")]
        public async Task<IActionResult> Delete(string filename)
        {
             await _daprClient.DeleteStateAsync(storeName, filename);

            return Ok("Data Successfully Deleted By Name" + " " + filename);
        }
    }
}
