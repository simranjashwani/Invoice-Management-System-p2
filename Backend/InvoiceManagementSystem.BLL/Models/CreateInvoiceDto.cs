using System;
using System.Collections.Generic;

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

        public List<CreateInvoiceLineItemDto> LineItems { get; set; } = new List<CreateInvoiceLineItemDto>();
    }

    public class CreateInvoiceLineItemDto
    {
        public string Description { get; set; } = "";
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
    }
}