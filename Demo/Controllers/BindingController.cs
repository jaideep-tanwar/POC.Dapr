using Dapr.Client;
using Demo.Model;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Demo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BindingController : ControllerBase
    {


        // POST api/<BindingController>
        [HttpPost("urlbinding")]
        public async Task<IActionResult> Post([FromBody] object value)
        {
            Console.WriteLine("Binding called");
            return Ok();
        }

    }
}
