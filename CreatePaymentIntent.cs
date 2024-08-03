using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stripe;

namespace StripePaymentFunction
{
    public class CreatePaymentIntent
    {
        private readonly ILogger _logger;

        public CreatePaymentIntent(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<CreatePaymentIntent>();
        }

        [Function("CreatePaymentIntent")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function,  "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<CreatePaymentIntentRequest>(requestBody);

            StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("StripeSecretKey");

            var options = new PaymentIntentCreateOptions
            {
                Amount = data.Amount,
                Currency = "usd",
                PaymentMethodTypes = new List<string> { "card" },
            };
            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            var paymentIntentStatus = paymentIntent.Status;

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            var responseBody = JsonConvert.SerializeObject(new
            {
                clientSecret = paymentIntent.ClientSecret,
                status = paymentIntentStatus
            });
            await response.WriteStringAsync(responseBody);

            return response;
        }

        public class CreatePaymentIntentRequest
        {
            public long Amount { get; set; }
        }
    }
}
