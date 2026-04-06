namespace InvoiceManagementSystem.BLL.Models
{
    public class CreatePaymentDto
    {
        public decimal PaymentAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public DateTime ReceivedDate { get; set; }
    }
}
