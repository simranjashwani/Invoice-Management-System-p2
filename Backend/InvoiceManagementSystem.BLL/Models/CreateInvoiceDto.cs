namespace InvoiceManagementSystem.BLL.Models
{
    public class CreateInvoiceDto
    {
        public int CustomerId { get; set; }
        public int? QuoteId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
    }
}