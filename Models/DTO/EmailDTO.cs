namespace StripeIntegrationSample.Models.DTO
{
    public class EmailDTO
    {
        public string EmailTo { get; set; }
        public string EmailSubject { get; set; }
        public string EmailContent { get; set; }
        public string EmailTemplateId { get; set; }
        public string EmailVariables { get; set; }
    }
}
