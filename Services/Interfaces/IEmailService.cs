using StripeIntegrationSample.Models.DTO;

namespace StripeIntegrationSample.Services.Interfaces
{
    public interface IEmailService
    {
        Task<string> SendEmailUsingSESAsync(EmailDTO emailDTO);
    }
}