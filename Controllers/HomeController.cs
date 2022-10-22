using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using StripeIntegrationSample.Models;
using StripeIntegrationSample.Models.DTO;
using StripeIntegrationSample.Services.Interfaces;
using System.Diagnostics;

namespace StripeIntegrationSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IStripeServices _stripeServices;
        private readonly StripePaymentConfig _stripeConfig;
        private readonly IntegrationDbContext _integrationDbContext;

        public HomeController(ILogger<HomeController> logger, IStripeServices stripeServices, 
            IOptionsMonitor<StripePaymentConfig> optionsMonitor, IntegrationDbContext integrationDbContext)
        {
            _logger = logger;
            _stripeServices = stripeServices;
            _stripeConfig = optionsMonitor.CurrentValue;
            _integrationDbContext = integrationDbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index([FromForm] PaymentDTO paymentDTO)
        {
            if(paymentDTO == null || !ModelState.IsValid)
            {
                return View(ModelState);
            }

            StripeDataDTO stripeDataDTO = new StripeDataDTO
            {
                SuccessUrl = string.Empty,
                ProductDescription = "Payment for integration",
                Amount = paymentDTO.Amount,
                CustomerEmail = paymentDTO.CustomerEmail,
                CancelUrl = string.Empty,
                PaymentReference = DateTime.UtcNow.ToString("yyyyMMddmmsstt"),
                ProductName = "Integration"
            };

            //get checkout url
            var paymentUrl = _stripeServices.GetCheckoutUrl(stripeDataDTO);

            if (string.IsNullOrEmpty(paymentUrl))
            {
                return View();
            }

            return Redirect(paymentUrl);
        }

        // stripe webhook
        [HttpPost("stripe/webhook")]
        public async Task<IActionResult> NotificationWebhookAsync()
        {
            //verify the event is from stripe

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            string stripeSecretKey = _stripeConfig.SigningSecret;

            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], stripeSecretKey);
            }
            catch (StripeException)
            {
                return BadRequest();
            }

            if (stripeEvent.Type == Events.CheckoutSessionCompleted)
            {
                //get payment reference
                var session = stripeEvent.Data.Object as Stripe.Checkout.Session;

                //check if payment already saved, else save payment

                bool isPaymentSaved = _integrationDbContext.StripePayments
                    .Any(x => x.PaymentReference == session.ClientReferenceId);

                if (isPaymentSaved)
                {
                    return Ok();
                }

                //process payment
                int response = await _stripeServices.ProcessPaymentAsync(stripeEvent);
            }

            return Ok();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}