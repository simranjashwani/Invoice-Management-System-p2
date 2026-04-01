using InvoiceManagementSystem.DAL.Data;
using InvoiceManagementSystem.DAL.Entities;
using InvoiceManagementSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using InvoiceManagementSystem.BLL.Exceptions;
using InvoiceManagementSystem.BLL.CQRS.Commands;

public class CreatePaymentHandler
{
    private readonly ApplicationDbContext _context;
    private readonly InvoiceAnalyticsService _analyticsService;

   public CreatePaymentHandler(ApplicationDbContext context, InvoiceAnalyticsService analyticsService)
{
    _context = context;
    _analyticsService = analyticsService;
}
    public async Task<Payment> Handle(CreatePaymentCommand command)
    {
        //  Start transaction (atomic)
        using var transaction = await _context.Database.BeginTransactionAsync();

        var invoice = await _context.Invoices
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.InvoiceId == command.InvoiceId);

        if (invoice == null)
            throw new BusinessException("Invoice not found");

        var totalPayments = invoice.Payments.Sum(p => p.PaymentAmount);
        var outstanding = invoice.GrandTotal - totalPayments;

        if (command.PaymentAmount <= 0)
            throw new BusinessException("Payment must be greater than zero");

        if (command.PaymentAmount > outstanding)
            throw new BusinessException("Payment exceeds outstanding balance");

        //  Create payment
        var payment = new Payment
        {
            InvoiceId = command.InvoiceId,
            PaymentAmount = command.PaymentAmount,
            PaymentDate = DateTime.Now,
            PaymentMethod = command.PaymentMethod,
            ReferenceNumber = command.ReferenceNumber,
            ReceivedDate = DateTime.Now
        };

        _context.Payments.Add(payment);

        //  Update OutstandingBalance
        invoice.OutstandingBalance = outstanding - command.PaymentAmount;

        //  Update Status
        if (invoice.OutstandingBalance == 0)
            invoice.Status = InvoiceStatus.Paid;
        else
            invoice.Status = InvoiceStatus.PartiallyPaid;

        await _context.SaveChangesAsync();


//  CLEAR CACHE
await _analyticsService.ClearCache();
        await transaction.CommitAsync();

        return payment;
    }
}