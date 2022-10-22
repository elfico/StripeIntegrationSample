using Amazon;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Microsoft.Extensions.Options;
using StripeIntegrationSample.Models;
using StripeIntegrationSample.Models.DTO;
using StripeIntegrationSample.Services.Interfaces;

namespace StripeIntegrationSample.Services
{
    public class EmailService : IEmailService
    {

        private readonly ILogger<EmailService> _logger;
        private readonly EmailConfig _emailConfig;
        public EmailService(ILogger<EmailService> logger, IOptionsMonitor<EmailConfig> optionsMonitor)
        {
            _emailConfig = optionsMonitor.CurrentValue;
            _logger = logger;
        }

        public async Task<string> SendEmailUsingSESAsync(EmailDTO emailDTO)
        {
            //if template Id empty, send as normal email
            string accessKey = _emailConfig.AWSClientId;
            string accessSecret = _emailConfig.AWSClientSecret;

            AmazonSimpleEmailServiceV2Client client = new AmazonSimpleEmailServiceV2Client(accessKey, accessSecret, RegionEndpoint.USEast1);

            string emailFrom = _emailConfig.EmailFrom;

            SendEmailRequest emailRequest = new SendEmailRequest
            {
                Destination = new Destination
                {
                    ToAddresses = new List<string> { emailDTO.EmailTo }
                },
                FromEmailAddress = emailFrom,
            };

            //if template set
            if (!string.IsNullOrEmpty(emailDTO.EmailTemplateId))
            {
                emailRequest.Content = new EmailContent
                {
                    Template = new Template
                    {
                        TemplateArn = string.Empty,
                        TemplateData = !string.IsNullOrEmpty(emailDTO.EmailVariables) ? emailDTO.EmailVariables : "{ \"name\":\"friend\" }",
                        TemplateName = emailDTO.EmailTemplateId
                    }
                };
            }
            else
            {
                emailRequest.Content = new EmailContent
                {
                    Simple = new Message
                    {
                        Body = new Body
                        {
                            Html = new Content
                            {
                                Data = emailDTO.EmailContent,
                            },
                        },
                        Subject = new Content
                        {
                            Data = emailDTO.EmailSubject
                        }
                    }
                };
            }

            string response = string.Empty;

            try
            {
                SendEmailResponse emailResponse = await client.SendEmailAsync(emailRequest);
                response = $"{emailResponse.HttpStatusCode}:{emailResponse.MessageId}";
            }
            catch (Exception ex)
            {
                response = $"Failed: {ex.Message}";
                _logger.LogError(ex.ToString());
            }

            return response;
        }
    }
}
