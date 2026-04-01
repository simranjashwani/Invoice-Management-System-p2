namespace InvoiceManagementSystem.BLL.CQRS.Commands
{
    public class CreatePaymentCommand
    {
        public int InvoiceId { get; set; }
        public decimal PaymentAmount { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    }
}