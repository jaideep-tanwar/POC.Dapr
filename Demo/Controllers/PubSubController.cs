using Dapr;
using Dapr.Client;
using Demo.Model;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Demo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PubSubController : ControllerBase
    {
        private readonly DaprClient _daprClient;

        public PubSubController(DaprClient daprClient)
        {
            _daprClient = daprClient;
        }        

        // POST api/<PubSubController>
        [HttpPost]
        public async Task<IActionResult> Post([FromForm] Sample sample)
        {
            await _daprClient.PublishEventAsync<Sample>("pubsub", "sample", sample);
           return Ok();
        }
        [Topic("pubsub", "sample")]
        [HttpPost("Subscribe")]
        public async Task<IActionResult> Subscribe(Sample sample)
        {
            Console.WriteLine(sample.Name);
            return Ok(sample);
        }
      
    }
}
