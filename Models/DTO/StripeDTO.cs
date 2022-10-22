using System.ComponentModel.DataAnnotations;

namespace StripeIntegrationSample.Models.DTO
{
    public class StripeDataDTO
    {
        [Required]
        public string SuccessUrl { get; set; }

        [Required]
        public string CancelUrl { get; set; }

        [Required]
        public string ProductName { get; set; }

        [Required]
        public string ProductDescription { get; set; }

        [Required]
        [Range(0.5, double.MaxValue)] //minimum of 50 cents
        public decimal Amount { get; set; }

        [Required]
        public string CustomerEmail { get; set; }

        [Required]
        public string PaymentReference { get; set; }
    }

    public class PaymentDTO
    {
        public string CustomerEmail { get; set; }
        public int Amount { get; set; }
    }
}
