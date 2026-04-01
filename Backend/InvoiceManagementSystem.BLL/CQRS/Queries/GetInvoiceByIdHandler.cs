using InvoiceManagementSystem.DAL.Repositories;
using InvoiceManagementSystem.DAL.Entities;
using System.Linq;

namespace InvoiceManagementSystem.BLL.CQRS.Queries
{
    public class GetInvoiceByIdHandler
    {
        private readonly IInvoiceRepository _repository;

        public GetInvoiceByIdHandler(IInvoiceRepository repository)
        {
            _repository = repository;
        }

        public async Task<Invoice?> Handle(GetInvoiceByIdQuery query)
{
    var invoice = await _repository.GetByIdAsync(query.Id);

    if (invoice != null)
    {
        // 🔥 Calculate total payments
        var totalPayments = invoice.Payments?.Sum(p => p.PaymentAmount) ?? 0;

        // 🔥 Calculate OutstandingBalance
        invoice.OutstandingBalance = invoice.GrandTotal - totalPayments;

        // 🔥 Prevent negative value
        if (invoice.OutstandingBalance < 0)
            invoice.OutstandingBalance = 0;
    }

    return invoice;
}
    }
}