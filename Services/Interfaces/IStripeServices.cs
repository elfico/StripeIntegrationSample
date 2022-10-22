using Stripe;
using StripeIntegrationSample.Models.DTO;

namespace StripeIntegrationSample.Services.Interfaces
{
    public interface IStripeServices
    {
        string GetCheckoutUrl(StripeDataDTO stripeDataDTO);
        Task<int> ProcessPaymentAsync(Event stripeEvent);
    }
}