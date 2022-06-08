using Dapr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using POC.Model;

namespace POC.Controllers
{
    
    [ApiController]
    [Route("[controller]")]
    public class ImageFunctionTriggerController : ControllerBase
    {
        //[Topic("pubsub", "orders")]
        //[HttpPost("", Name = "SubmitOrder")]
        //public async Task<IActionResult> Submit(User order)
        //{
        //    Console.WriteLine("Received a new order from" + " " + order.Id + " " + order.FirstName);
        //    //logger.LogInformation($"Received a new order from {order.CustomerDetails.Name}");
        //    //await emailSender.SendEmailForOrder(order);
        //    return Ok();
        //}
    }
}
