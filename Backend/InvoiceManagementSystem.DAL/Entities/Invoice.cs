using System;
using System.Collections.Generic;
using InvoiceManagementSystem.DAL.Enums;

namespace InvoiceManagementSystem.DAL.Entities
{
    public class Invoice
    {
        public int InvoiceId { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;

        //public int CustomerId { get; set; }   // FK (from Customer module)
        
        // Foreign Key
        public int CustomerId { get; set; }

// Navigation Property
        public Customer Customer { get; set; } = null!;

        public int? QuoteId { get; set; }     // Optional FK (from Quotation module)

        public DateTime InvoiceDate { get; set; }

        public DateTime DueDate { get; set; }

       public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        public decimal SubTotal { get; set; }

        public decimal Tax { get; set; }

        public decimal Discount { get; set; }

        public decimal GrandTotal { get; set; }

        public decimal OutstandingBalance { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation Properties
        public ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();

        public ICollection<InvoiceAttachment> Attachments { get; set; } = new List<InvoiceAttachment>();
    }
}
