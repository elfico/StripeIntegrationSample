namespace StripeIntegrationSample.Models
{
    public class StripePaymentConfig
    {
        public string SecretKey { get; set; }
        public string PublicKey { get; set; }
        public string SigningSecret { get; set; }
    }

    public class EmailConfig
    {
        public string EmailFrom { get; set; }
        public string SenderId { get; set; }
        public string AWSClientId { get; set; }
        public string AWSClientSecret { get; set; }
    }
}
