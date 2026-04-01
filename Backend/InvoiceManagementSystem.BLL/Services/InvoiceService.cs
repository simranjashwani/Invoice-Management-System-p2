using InvoiceManagementSystem.DAL.Entities;
using InvoiceManagementSystem.DAL.Repositories;
using InvoiceManagementSystem.BLL.Interfaces;
using InvoiceManagementSystem.DAL.Enums;
using InvoiceManagementSystem.BLL.Models;

namespace InvoiceManagementSystem.BLL.Services
{
    public class InvoiceService : IInvoiceService
    {

private void ValidateStatusTransition(InvoiceStatus currentStatus, InvoiceStatus newStatus)
{
    var validTransitions = new Dictionary<InvoiceStatus, List<InvoiceStatus>>
    {
        { InvoiceStatus.Draft, new List<InvoiceStatus> { InvoiceStatus.Sent, InvoiceStatus.Cancelled } },
        { InvoiceStatus.Sent, new List<InvoiceStatus> { InvoiceStatus.PartiallyPaid, InvoiceStatus.Paid, InvoiceStatus.Cancelled } },
        { InvoiceStatus.PartiallyPaid, new List<InvoiceStatus> { InvoiceStatus.Paid } },
        { InvoiceStatus.Overdue, new List<InvoiceStatus> { InvoiceStatus.Paid } },
        { InvoiceStatus.Paid, new List<InvoiceStatus>() },
        { InvoiceStatus.Cancelled, new List<InvoiceStatus>() }
    };

    if (!validTransitions.ContainsKey(currentStatus) ||
        !validTransitions[currentStatus].Contains(newStatus))
    {
        throw new Exception($"Invalid status transition from {currentStatus} to {newStatus}");
    }
}


public async Task ChangeInvoiceStatusAsync(int invoiceId, InvoiceStatus newStatus)
{
    var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);

    if (invoice == null)
        throw new Exception("Invoice not found.");

    ValidateStatusTransition(invoice.Status, newStatus);

    invoice.Status = newStatus;

    await _invoiceRepository.UpdateAsync(invoice);
}


public async Task GenerateAgingReportAsync()
{
    var invoices = await _invoiceRepository.GetAllAsync();

    var today = DateTime.Now;

    var agingData = invoices
        .Where(i => i.Status != InvoiceStatus.Paid &&
                    i.Status != InvoiceStatus.Cancelled &&
                    i.DueDate < today)
        .Select(i => new AgingReportItem
        {
            Invoice = i,
            DaysOverdue = (today - i.DueDate).Days,
            Outstanding = i.GrandTotal - i.Payments.Sum(p => p.PaymentAmount)
        })
        .Where(x => x.Outstanding > 0)
        .ToList();

    var bucket1 = agingData.Where(x => x.DaysOverdue <= 30).ToList();
    var bucket2 = agingData.Where(x => x.DaysOverdue > 30 && x.DaysOverdue <= 60).ToList();
    var bucket3 = agingData.Where(x => x.DaysOverdue > 60).ToList();

    Console.WriteLine("\n===== AGING REPORT =====");

    PrintBucket("0-30 Days", bucket1);
    PrintBucket("31-60 Days", bucket2);
    PrintBucket("61+ Days", bucket3);
}



public async Task UpdateOverdueInvoicesAsync()
{
    var invoices = await _invoiceRepository.GetAllAsync();
    var today = DateTime.Now;

    foreach (var invoice in invoices)
    {
        if (invoice.DueDate < today &&
            invoice.Status != InvoiceStatus.Paid &&
            invoice.Status != InvoiceStatus.Cancelled &&
            invoice.Status != InvoiceStatus.Overdue)
        {
            invoice.Status = InvoiceStatus.Overdue;
            await _invoiceRepository.UpdateAsync(invoice);
        }
    }

    Console.WriteLine("Overdue invoices updated.");
}

public async Task CalculateDsoAsync()
{
    var invoices = await _invoiceRepository.GetAllAsync();

    var validInvoices = invoices
        .Where(i => i.Status != InvoiceStatus.Cancelled)
        .ToList();

    decimal totalSales = validInvoices.Sum(i => i.GrandTotal);

    decimal totalOutstanding = validInvoices.Sum(i =>
        i.GrandTotal - i.Payments.Sum(p => p.PaymentAmount));

    if (totalSales == 0)
    {
        Console.WriteLine("No sales data available.");
        return;
    }

    decimal dso = (totalOutstanding / totalSales) * 30;

    Console.WriteLine("\n===== DSO REPORT =====");
    Console.WriteLine($"Total Sales: {totalSales}");
    Console.WriteLine($"Total Outstanding: {totalOutstanding}");
    Console.WriteLine($"Estimated DSO (30-day basis): {Math.Round(dso, 2)} days");
}

private void PrintBucket(string title, List<AgingReportItem> invoices)
{
    Console.WriteLine($"\n--- {title} ---");

    decimal total = 0;

    foreach (var item in invoices)
    {
        Console.WriteLine($"Invoice: {item.Invoice.InvoiceNumber}");
        Console.WriteLine($"Days Overdue: {item.DaysOverdue}");
        Console.WriteLine($"Outstanding: {item.Outstanding}");
        Console.WriteLine("------------------------");

        total += item.Outstanding;
    }

    Console.WriteLine($"Total Outstanding in {title}: {total}");
}



        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceService(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        // public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
        // {
        //     // Calculate SubTotal
        //     invoice.SubTotal = invoice.LineItems.Sum(li => li.Quantity * li.UnitPrice);

        //     // Calculate Line Tax
        //     var totalTax = invoice.LineItems.Sum(li => li.Tax);
        //     invoice.Tax = totalTax;

        //     // Calculate Discount
        //     var totalDiscount = invoice.LineItems.Sum(li => li.Discount);
        //     invoice.Discount = totalDiscount;

        //     // Calculate Grand Total
        //     invoice.GrandTotal = invoice.SubTotal + invoice.Tax - invoice.Discount;

        //     invoice.Status = "Draft";
        //     invoice.InvoiceDate = DateTime.Now;
        //     invoice.CreatedDate = DateTime.Now;

        //     return await _invoiceRepository.CreateAsync(invoice);
        // }


public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
{
    // 🔥 Auto Invoice Number
    invoice.InvoiceNumber = "INV-" + DateTime.Now.Ticks;

    // 🔥 SubTotal (for now simple)
    invoice.SubTotal = 0; // we will update later with line items

    // 🔥 Grand Total Calculation
    invoice.GrandTotal = invoice.SubTotal + invoice.Tax - invoice.Discount;

    // 🔥 Default Status
    invoice.Status = InvoiceStatus.Draft;

    return await _invoiceRepository.CreateAsync(invoice);
}

        public async Task<List<Invoice>> GetAllInvoicesAsync()
        {
            return await _invoiceRepository.GetAllAsync();
        }

        public async Task<Invoice?> GetInvoiceByIdAsync(int id)
        {
            return await _invoiceRepository.GetByIdAsync(id);
        }

        public async Task AddPaymentAsync(int invoiceId, decimal amount, string paymentMethod)
{
    var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);

    if (invoice == null)
        throw new Exception("Invoice not found.");

    if (amount <= 0)
        throw new Exception("Payment amount must be greater than zero.");

    var totalPaidSoFar = invoice.Payments.Sum(p => p.PaymentAmount);

    if (totalPaidSoFar + amount > invoice.GrandTotal)
        throw new Exception("Payment exceeds remaining balance.");

    // Create Payment record
    var payment = new Payment
    {
        InvoiceId = invoiceId,
        PaymentAmount = amount,
        PaymentDate = DateTime.Now,
        PaymentMethod = paymentMethod,
        ReceivedDate = DateTime.Now
    };

    invoice.Payments.Add(payment);

    var newTotalPaid = totalPaidSoFar + amount;

    if (newTotalPaid == invoice.GrandTotal)
       invoice.Status = InvoiceStatus.Paid;
    else
       invoice.Status = InvoiceStatus.PartiallyPaid;

    await _invoiceRepository.UpdateAsync(invoice);
}


public async Task GenerateRevenueSummaryAsync()
{
    var invoices = await _invoiceRepository.GetAllAsync();

    decimal totalRevenue = invoices.Sum(i => i.GrandTotal);
    decimal totalCollected = invoices.Sum(i => i.Payments.Sum(p => p.PaymentAmount));
    decimal totalOutstanding = totalRevenue - totalCollected;

    Console.WriteLine("\n===== REVENUE SUMMARY =====");
    Console.WriteLine($"Total Revenue: {totalRevenue}");
    Console.WriteLine($"Total Collected: {totalCollected}");
    Console.WriteLine($"Total Outstanding: {totalOutstanding}");
}


public async Task GenerateCustomerOutstandingReportAsync()
{
    var invoices = await _invoiceRepository.GetAllAsync();

    var report = invoices
        .GroupBy(i => i.Customer.Name)
        .Select(g => new
        {
            CustomerName = g.Key,
            Outstanding = g.Sum(i => i.GrandTotal - i.Payments.Sum(p => p.PaymentAmount))
        });

    Console.WriteLine("\n===== OUTSTANDING PER CUSTOMER =====");

    foreach (var item in report)
    {
        Console.WriteLine($"{item.CustomerName} → {item.Outstanding}");
    }
}

public async Task GenerateReconciliationReportAsync()
{
    var invoices = await _invoiceRepository.GetAllAsync();

    Console.WriteLine("\n===== RECONCILIATION REPORT =====");

    foreach (var invoice in invoices)
    {
        decimal paid = invoice.Payments.Sum(p => p.PaymentAmount);
        decimal outstanding = invoice.GrandTotal - paid;

        string status = outstanding == 0 ? "Reconciled" :
                        outstanding > 0 ? "Pending" :
                        "Error";

        Console.WriteLine($"Invoice: {invoice.InvoiceNumber}");
        Console.WriteLine($"Total: {invoice.GrandTotal}");
        Console.WriteLine($"Paid: {paid}");
        Console.WriteLine($"Outstanding: {outstanding}");
        Console.WriteLine($"Status: {status}");
        Console.WriteLine("---------------------------");
    }
}


public async Task GenerateTopUnpaidInvoicesAsync()
{
    var invoices = await _invoiceRepository.GetAllAsync();

    var topUnpaid = invoices
        .Select(i => new
        {
            i.InvoiceNumber,
            Outstanding = i.GrandTotal - i.Payments.Sum(p => p.PaymentAmount)
        })
        .Where(x => x.Outstanding > 0)
        .OrderByDescending(x => x.Outstanding)
        .Take(5);

    Console.WriteLine("\n===== TOP 5 UNPAID INVOICES =====");

    foreach (var item in topUnpaid)
    {
        Console.WriteLine($"{item.InvoiceNumber} → {item.Outstanding}");
    }
}

public async Task<Invoice?> UpdateInvoiceAsync(int id, CreateInvoiceDto dto)
{
    var invoice = await _invoiceRepository.GetByIdAsync(id);

    if (invoice == null)
        return null;

    // ❌ Business Rule: Cannot update if Paid
    if (invoice.Status == InvoiceStatus.Paid)
        throw new Exception("Cannot update a paid invoice");

    // Update fields
    invoice.CustomerId = dto.CustomerId;
    invoice.InvoiceDate = dto.InvoiceDate;
    invoice.DueDate = dto.DueDate;
    invoice.Discount = dto.Discount;
    invoice.Tax = dto.Tax;

    // Recalculate total
    invoice.GrandTotal = invoice.SubTotal + invoice.Tax - invoice.Discount;

    return await _invoiceRepository.UpdateAsync(invoice);
}

public async Task DeleteInvoiceAsync(int id)
{
    var invoice = await _invoiceRepository.GetByIdAsync(id);

    if (invoice == null)
        throw new Exception("Invoice not found");

    // ❌ Business Rule: Cannot delete if payments exist
    if (invoice.Payments != null && invoice.Payments.Any())
        throw new Exception("Cannot delete invoice with payments");

    await _invoiceRepository.DeleteAsync(id);
}

    }
}