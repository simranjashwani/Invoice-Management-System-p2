using InvoiceManagementSystem.DAL.Entities;

namespace InvoiceManagementSystem.BLL.Models
{
    public class AgingReportItem
    {
       public Invoice Invoice { get; set; } = null!;
        public int DaysOverdue { get; set; }
        public decimal Outstanding { get; set; }
    }
}