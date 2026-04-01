using System;

namespace InvoiceManagementSystem.DAL.Entities
{
    public class Payment
    {
        public int PaymentId { get; set; }

        public int InvoiceId { get; set; }

        public decimal PaymentAmount { get; set; }

        public DateTime PaymentDate { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;

        public string? ReferenceNumber { get; set; }

        public DateTime ReceivedDate { get; set; } = DateTime.Now;

        // Navigation
        public Invoice? Invoice { get; set; }
    }
}