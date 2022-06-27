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
            //await _daprClient.InvokeBindingAsync("pubsub", "create", "Heelo From Shankar");
            Console.WriteLine("publishing topics...");
            await _daprClient.PublishEventAsync<Sample>("pubsub", "sampletopic", sample);
            Console.WriteLine("Published sampletopic");
            //await _daprClient.PublishEventAsync<Sample>("pubsub", "sampletopic2", sample);
            //Console.WriteLine("Published sampletopic 2");
            //await _daprClient.PublishEventAsync<Sample>("pubsub", "sampletopic3", sample);
            //Console.WriteLine("Published sampletopic 3");


            return Ok();
        }
        [Topic("pubsub", "sampletopic")]
        [HttpPost("Subscribe")]
        public async Task<IActionResult> Subscribe(Sample sample)
        {
            Console.WriteLine("Subscribe sampletopic");
            return Ok(sample);
        }
    }
}
