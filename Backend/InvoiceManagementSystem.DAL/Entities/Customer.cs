using System.Collections.Generic;

namespace InvoiceManagementSystem.DAL.Entities
{
    public class Customer
    {
        public int CustomerId { get; set; }

        public string Name { get; set; } = null!;   
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Address { get; set; } = null!;

        // Navigation
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}