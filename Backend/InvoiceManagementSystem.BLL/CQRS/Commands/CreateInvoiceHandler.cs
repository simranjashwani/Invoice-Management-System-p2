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

            //  1. Generate Invoice Number (FORMAT: INV-YYYY-0001)
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

            //  2. Set Default Fields
            invoice.Status = InvoiceStatus.Draft;
            invoice.CreatedDate = DateTime.Now;

            //  3. Calculate GrandTotal
            var total = invoice.SubTotal + invoice.Tax - invoice.Discount;

invoice.GrandTotal = total < 0 ? 0 : total;

            //  4. OutstandingBalance (initially full amount)
           var totalPayments = invoice.Payments?.Sum(p => p.PaymentAmount) ?? 0;

invoice.OutstandingBalance = invoice.GrandTotal - totalPayments;
          //  return await _repository.CreateAsync(invoice);
          var createdInvoice = await _repository.CreateAsync(invoice);

//  CLEAR CACHE HERE
await _analyticsService.ClearCache();

return createdInvoice;
        }
    }
}