using Dapr.Client;
using Demo.Model;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Demo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class NosqlController : ControllerBase
    {
        private readonly DaprClient _daprClient;
        const string cosmosDbStore = "statestorecosmodb";

        public NosqlController(DaprClient daprClient)
        {
            _daprClient = daprClient;
        }


        // GET: api/<CosmosController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            string json = @"{""filter"": { ""EQ"": { ""firstName"": ""Testing"" } } }";
            var data =  await _daprClient.QueryStateAsync<Nosql>(cosmosDbStore,json);
            return Ok(data);
        }

        // GET api/<CosmosController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            Nosql data = null;
            try
            {
                 data = await _daprClient.GetStateAsync<Nosql>(cosmosDbStore, id.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception", ex);
            }
            if (data != null)
            {
                return Ok(data);
            }
            else
            {
                return Ok("Something Wrong...");
            }
        }

        // POST api/<CosmosController>
        [HttpPost]
        public async Task<IActionResult> Post([FromForm] Nosql nosql)
        {
            if (nosql != null)
            {
                try
                {
                    await _daprClient.SaveStateAsync<Nosql>(cosmosDbStore, nosql.Id.ToString(), nosql);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception", ex);
                    return Ok("Something Wrong...");
                }
                return Ok("Data Successfully Store by Id" + " " + nosql.Id);
            }
            else
            {
                return Ok("Something Wrong...");
            }
        }

        // PUT api/<CosmosController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromForm] Nosql nosql)
        {
            var data = await _daprClient.GetStateEntryAsync<Nosql>(cosmosDbStore, id.ToString());
            var cosmosdata = new Nosql() { Id = id, FirstName = nosql.FirstName, LastName = nosql.LastName };
            if (data.Value != null)
            {
                data.Value = cosmosdata;
                data.SaveAsync();

                return Ok("Data Successfully Updated By Id" + " " + id);
            }
            else {
                return Ok("Something Wrong...");
            }
        }

        // DELETE api/<CosmosController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _daprClient.DeleteStateAsync(cosmosDbStore, id.ToString());

            return Ok("Data Successfully Deleted By Id" + " " + id);
        }
    }
}
