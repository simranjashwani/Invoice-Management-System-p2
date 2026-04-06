namespace InvoiceManagementSystem.BLL.Models
{
    public class AddInvoiceLineItemDto
    {
        public string Description { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Discount { get; set; }

        public decimal Tax { get; set; }
    }
}