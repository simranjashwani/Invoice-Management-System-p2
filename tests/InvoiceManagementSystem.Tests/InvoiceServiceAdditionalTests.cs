using InvoiceManagementSystem.BLL.Services;
using InvoiceManagementSystem.DAL.Entities;
using InvoiceManagementSystem.DAL.Enums;
using InvoiceManagementSystem.DAL.Repositories;
using Moq;
using Xunit;

namespace InvoiceManagementSystem.Tests;

public class InvoiceServiceAdditionalTests
{
    private readonly Mock<IInvoiceRepository> _repoMock;
    private readonly InvoiceService _service;

    public InvoiceServiceAdditionalTests()
    {
        _repoMock = new Mock<IInvoiceRepository>(MockBehavior.Strict);
        _service = new InvoiceService(_repoMock.Object);
    }

    [Fact]
    public async Task ChangeInvoiceStatusAsync_WhenTransitionIsValid_UpdatesInvoice()
    {
        var invoice = new Invoice { InvoiceId = 1, Status = InvoiceStatus.Draft };

        _repoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(invoice);
        _repoMock.Setup(repo => repo.UpdateAsync(invoice)).ReturnsAsync(invoice);

        await _service.ChangeInvoiceStatusAsync(1, InvoiceStatus.Sent);

        Assert.Equal(InvoiceStatus.Sent, invoice.Status);
        _repoMock.Verify(repo => repo.UpdateAsync(invoice), Times.Once);
    }

    [Fact]
    public async Task ChangeInvoiceStatusAsync_WhenTransitionIsInvalid_ThrowsException()
    {
        var invoice = new Invoice { InvoiceId = 1, Status = InvoiceStatus.Draft };

        _repoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(invoice);

        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _service.ChangeInvoiceStatusAsync(1, InvoiceStatus.Paid));

        Assert.Contains("Invalid status transition", exception.Message);
    }

    [Fact]
    public async Task UpdateOverdueInvoicesAsync_MarksOnlyEligibleInvoicesAsOverdue()
    {
        var today = DateTime.Now;
        var overdue = new Invoice { InvoiceId = 1, DueDate = today.AddDays(-5), Status = InvoiceStatus.Sent };
        var paid = new Invoice { InvoiceId = 2, DueDate = today.AddDays(-5), Status = InvoiceStatus.Paid };
        var future = new Invoice { InvoiceId = 3, DueDate = today.AddDays(5), Status = InvoiceStatus.Sent };

        _repoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Invoice> { overdue, paid, future });
        _repoMock.Setup(repo => repo.UpdateAsync(overdue)).ReturnsAsync(overdue);

        await _service.UpdateOverdueInvoicesAsync();

        Assert.Equal(InvoiceStatus.Overdue, overdue.Status);
        Assert.Equal(InvoiceStatus.Paid, paid.Status);
        Assert.Equal(InvoiceStatus.Sent, future.Status);
        _repoMock.Verify(repo => repo.UpdateAsync(overdue), Times.Once);
    }

    [Fact]
    public async Task CalculateDsoAsync_WhenSalesExist_PrintsDsoReport()
    {
        var invoices = new List<Invoice>
        {
            new()
            {
                Status = InvoiceStatus.Sent,
                GrandTotal = 1000,
                Payments = new List<Payment> { new() { PaymentAmount = 250 } }
            },
            new()
            {
                Status = InvoiceStatus.Cancelled,
                GrandTotal = 500,
                Payments = new List<Payment>()
            }
        };

        _repoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(invoices);

        using var writer = new StringWriter();
        Console.SetOut(writer);

        await _service.CalculateDsoAsync();

        var output = writer.ToString();
        Assert.Contains("DSO REPORT", output);
        Assert.Contains("Total Sales", output);
        Assert.Contains("Estimated DSO", output);
    }

    [Fact]
    public async Task GenerateAgingReportAsync_PrintsOutstandingBuckets()
    {
        var today = DateTime.Now;
        var invoices = new List<Invoice>
        {
            new()
            {
                InvoiceNumber = "INV-001",
                Status = InvoiceStatus.Sent,
                DueDate = today.AddDays(-10),
                GrandTotal = 500,
                Payments = new List<Payment> { new() { PaymentAmount = 100 } }
            }
        };

        _repoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(invoices);

        using var writer = new StringWriter();
        Console.SetOut(writer);

        await _service.GenerateAgingReportAsync();

        var output = writer.ToString();
        Assert.Contains("AGING REPORT", output);
        Assert.Contains("0-30 Days", output);
        Assert.Contains("INV-001", output);
    }

    [Fact]
    public async Task GenerateRevenueSummaryAsync_PrintsRevenueAndOutstandingTotals()
    {
        var invoices = new List<Invoice>
        {
            new()
            {
                GrandTotal = 1000,
                Payments = new List<Payment> { new() { PaymentAmount = 400 } }
            },
            new()
            {
                GrandTotal = 500,
                Payments = new List<Payment>()
            }
        };

        _repoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(invoices);

        using var writer = new StringWriter();
        Console.SetOut(writer);

        await _service.GenerateRevenueSummaryAsync();

        var output = writer.ToString();
        Assert.Contains("REVENUE SUMMARY", output);
        Assert.Contains("Total Revenue", output);
        Assert.Contains("Total Outstanding", output);
    }
}
