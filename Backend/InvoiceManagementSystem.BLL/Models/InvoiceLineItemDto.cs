namespace InvoiceManagementSystem.BLL.Models
{
    public class InvoiceLineItemDto
    {
        public int LineItemId { get; set; }

        public int InvoiceId { get; set; }

        public string Description { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Discount { get; set; }

        public decimal Tax { get; set; }

        public decimal LineTotal { get; set; }
    }
}