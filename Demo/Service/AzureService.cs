using Dapr.Client;

namespace Demo.Service
{
    public class AzureService :IAzureService
    {
        private readonly HttpClient _httpClient;

        public AzureService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task UpdateAsync()
        {
            Console.WriteLine("Hellpo");
            var dap = new DaprClientBuilder().Build();


            //var baseURL = (Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost") + ":" + (Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3500");

            //var client = new HttpClient();
            //client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            //// Adding app id as part of the header
            //client.DefaultRequestHeaders.Add("poc", "image");

            //var response = await client.GetAsync($"{baseURL}/hello");



            //var request =  new HttpRequestMessage(HttpMethod.Get, "poc","/image/hello");
            //var result1 = dap.CreateInvokeMethodRequest(HttpMethod.Get, "poc", "/image/hello");
            //Console.WriteLine("Result: " + result1);
            //await dap.InvokeMethodAsync(result1);
            ////Console.WriteLine("Order requested: " + orderId);
            //Console.WriteLine("call: " + result1);
            //var response = await _httpClient.SendAsync(request);
        }
    }
}
