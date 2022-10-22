using System.ComponentModel.DataAnnotations;

namespace StripeIntegrationSample.Models
{
    public class StripePayment
    {
        [Key]
        public int PaymentId { get; set; }
        public string PaymentReference { get; set; }
        public string CustomerEmail { get; set; }
        public string PaymentStatus { get; set; }
        public string Currency { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastModified { get; set; }
    }
}
