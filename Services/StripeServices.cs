using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using StripeIntegrationSample.Models;
using StripeIntegrationSample.Models.DTO;
using StripeIntegrationSample.Services.Interfaces;

namespace StripeIntegrationSample.Services
{
    public class StripeServices : IStripeServices
    {
        private readonly StripePaymentConfig _stripeConfig;
        private readonly ILogger<StripeServices> _logger;
        private readonly IntegrationDbContext _integrationDbContext;
        private readonly IEmailService _emailService;
        public StripeServices(IOptionsMonitor<StripePaymentConfig> optionsMonitor, ILogger<StripeServices> logger,
            IntegrationDbContext integrationDbContext, IEmailService emailService)
        {
            _logger = logger;
            _stripeConfig = optionsMonitor.CurrentValue;
            _integrationDbContext = integrationDbContext;
            _emailService = emailService;
        }

        public string GetCheckoutUrl(StripeDataDTO stripeDataDTO)
        {
            StripeConfiguration.ApiKey = _stripeConfig.SecretKey;

            var amountInCents = stripeDataDTO.Amount * 100;

            string checkoutUrl = string.Empty;

            try
            {
                var options = new SessionCreateOptions
                {
                    LineItems = new List<SessionLineItemOptions>
                {
                  new SessionLineItemOptions
                  {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "USD",
                        UnitAmountDecimal = amountInCents, //in cents 
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = stripeDataDTO.ProductName,
                            Description = stripeDataDTO.ProductDescription
                        }
                    },
                    Quantity = 1,
                  },
                },
                    Mode = "payment",
                    SuccessUrl = stripeDataDTO.SuccessUrl,
                    CancelUrl = stripeDataDTO.CancelUrl,
                    ClientReferenceId = stripeDataDTO.PaymentReference,
                    CustomerEmail = stripeDataDTO.CustomerEmail
                };
                var service = new SessionService();
                Session session = service.Create(options);
                checkoutUrl = session.Url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return checkoutUrl;
        }

        public async Task<int> ProcessPaymentAsync(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Session;

            string paymentReference = session!.ClientReferenceId;

            if (string.IsNullOrEmpty(paymentReference))
            {
                return 0;
            }

            StripePayment stripePayment = new StripePayment
            {
                PaymentStatus = session.PaymentStatus,
                Currency = session.Currency,
                CustomerEmail = session.CustomerEmail,
                AmountPaid = Convert.ToInt32(session.AmountTotal),
                PaymentReference = paymentReference,
                DateCreated = DateTime.UtcNow,
                LastModified = DateTime.UtcNow
            };

            //save to DB

            _integrationDbContext.StripePayments.Add(stripePayment);

            int result = await _integrationDbContext.SaveChangesAsync();

            //send email on success
            if (result > 0)
            {
                EmailDTO emailDataDTO = new EmailDTO
                {
                    EmailSubject = "Integration payment",
                    EmailContent = "Your payment was successful",
                    EmailTemplateId = string.Empty,
                    EmailTo = session.CustomerEmail,
                    EmailVariables = string.Empty
                };
                await _emailService.SendEmailUsingSESAsync(emailDataDTO);
            }

            return result;
        }
    }
}
