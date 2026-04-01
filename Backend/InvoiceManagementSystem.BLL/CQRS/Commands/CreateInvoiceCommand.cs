using InvoiceManagementSystem.DAL.Entities;

namespace InvoiceManagementSystem.BLL.CQRS.Commands
{
    public class CreateInvoiceCommand
    {
        public Invoice Invoice { get; set; }

        public CreateInvoiceCommand(Invoice invoice)
        {
            Invoice = invoice;
        }
    }
}