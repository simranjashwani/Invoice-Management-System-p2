using InvoiceManagementSystem.DAL.Repositories;
using InvoiceManagementSystem.DAL.Entities;
using InvoiceManagementSystem.DAL.Enums;

namespace InvoiceManagementSystem.BLL.CQRS.Commands
{
    public class CreateInvoiceHandler
    {
        private readonly IInvoiceRepository _repository;
        private readonly InvoiceAnalyticsService _analyticsService;

        public CreateInvoiceHandler(IInvoiceRepository repository, InvoiceAnalyticsService analyticsService)
        {
            _repository = repository;
            _analyticsService = analyticsService;
        }

        public async Task<Invoice> Handle(CreateInvoiceCommand command)
        {
            var invoice = command.Invoice;

            // 1. Generate Invoice Number (FORMAT: INV-YYYY-0001)
            var lastInvoice = await _repository.GetLastInvoiceAsync();
            int nextNumber = 1;

            if (lastInvoice != null && !string.IsNullOrEmpty(lastInvoice.InvoiceNumber))
            {
                var parts = lastInvoice.InvoiceNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            invoice.InvoiceNumber = $"INV-{DateTime.Now.Year}-{nextNumber:D4}";

            // 2. Default fields
            invoice.Status = InvoiceStatus.Draft;
            invoice.CreatedDate = DateTime.Now;

            // 3. Make sure LineItems list exists
            invoice.LineItems ??= new List<InvoiceLineItem>();

            // 4. Calculate each line item total
            foreach (var item in invoice.LineItems)
            {
                item.LineTotal = (item.Quantity * item.UnitPrice) - item.Discount + item.Tax;
            }

            // 5. Calculate invoice totals from line items
            invoice.SubTotal = invoice.LineItems.Sum(x => x.Quantity * x.UnitPrice);
            invoice.Tax = invoice.LineItems.Sum(x => x.Tax);
            invoice.Discount = invoice.LineItems.Sum(x => x.Discount);

            var total = invoice.LineItems.Sum(x => x.LineTotal);
            invoice.GrandTotal = total < 0 ? 0 : total;

            // 6. Outstanding balance initially equals grand total
            invoice.OutstandingBalance = invoice.GrandTotal;

            // 7. Save invoice
            var createdInvoice = await _repository.CreateAsync(invoice);

            // 8. Clear analytics cache
            await _analyticsService.ClearCache();

            return createdInvoice;
        }
    }
}