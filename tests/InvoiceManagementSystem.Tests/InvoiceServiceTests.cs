using InvoiceManagementSystem.BLL.Models;
using InvoiceManagementSystem.BLL.Services;
using InvoiceManagementSystem.DAL.Entities;
using InvoiceManagementSystem.DAL.Enums;
using InvoiceManagementSystem.DAL.Repositories;
using Moq;
using Xunit;

namespace InvoiceManagementSystem.Tests;

public class InvoiceServiceTests
{
    private readonly Mock<IInvoiceRepository> _repoMock;
    private readonly InvoiceService _service;

    public InvoiceServiceTests()
    {
        _repoMock = new Mock<IInvoiceRepository>(MockBehavior.Strict);
        _service = new InvoiceService(_repoMock.Object);
    }

    [Fact]
    public async Task CreateInvoiceAsync_SetsDraftStatusAndCalculatesGrandTotal()
    {
        var invoice = new Invoice
        {
            Tax = 50,
            Discount = 10
        };

        _repoMock
            .Setup(repo => repo.CreateAsync(It.IsAny<Invoice>()))
            .ReturnsAsync((Invoice created) => created);

        var result = await _service.CreateInvoiceAsync(invoice);

        Assert.StartsWith("INV-", result.InvoiceNumber);
        Assert.Equal(InvoiceStatus.Draft, result.Status);
        Assert.Equal(0m, result.SubTotal);
        Assert.Equal(40m, result.GrandTotal);

        _repoMock.Verify(repo => repo.CreateAsync(It.IsAny<Invoice>()), Times.Once);
    }

    [Fact]
    public async Task AddPaymentAsync_WhenInvoiceNotFound_ThrowsException()
    {
        _repoMock
            .Setup(repo => repo.GetByIdAsync(10))
            .ReturnsAsync((Invoice?)null);

        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _service.AddPaymentAsync(10, 100, "UPI"));

        Assert.Contains("Invoice not found", exception.Message);
    }

    [Fact]
    public async Task AddPaymentAsync_WhenAmountIsNotPositive_ThrowsException()
    {
        var invoice = new Invoice
        {
            InvoiceId = 1,
            GrandTotal = 500,
            Payments = new List<Payment>()
        };

        _repoMock
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(invoice);

        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _service.AddPaymentAsync(1, 0, "Cash"));

        Assert.Contains("greater than zero", exception.Message);
        _repoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Invoice>()), Times.Never);
    }

    [Fact]
    public async Task AddPaymentAsync_WhenPaymentExceedsGrandTotal_ThrowsException()
    {
        var invoice = new Invoice
        {
            InvoiceId = 1,
            GrandTotal = 300,
            Payments = new List<Payment>
            {
                new() { PaymentAmount = 250 }
            }
        };

        _repoMock
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(invoice);

        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _service.AddPaymentAsync(1, 100, "Card"));

        Assert.Contains("exceeds remaining balance", exception.Message);
        _repoMock.Verify(repo => repo.UpdateAsync(It.IsAny<Invoice>()), Times.Never);
    }

    [Fact]
    public async Task AddPaymentAsync_WhenPaymentIsPartial_SetsPartiallyPaidStatus()
    {
        var invoice = new Invoice
        {
            InvoiceId = 1,
            GrandTotal = 400,
            Status = InvoiceStatus.Sent,
            Payments = new List<Payment>()
        };

        _repoMock
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(invoice);
        _repoMock
            .Setup(repo => repo.UpdateAsync(invoice))
            .ReturnsAsync(invoice);

        await _service.AddPaymentAsync(1, 150, "Cash");

        Assert.Single(invoice.Payments);
        Assert.Equal(150m, invoice.Payments.First().PaymentAmount);
        Assert.Equal(InvoiceStatus.PartiallyPaid, invoice.Status);
        _repoMock.Verify(repo => repo.UpdateAsync(invoice), Times.Once);
    }

    [Fact]
    public async Task AddPaymentAsync_WhenPaymentSettlesInvoice_SetsPaidStatus()
    {
        var invoice = new Invoice
        {
            InvoiceId = 1,
            GrandTotal = 250,
            Status = InvoiceStatus.Sent,
            Payments = new List<Payment>()
        };

        _repoMock
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(invoice);
        _repoMock
            .Setup(repo => repo.UpdateAsync(invoice))
            .ReturnsAsync(invoice);

        await _service.AddPaymentAsync(1, 250, "UPI");

        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        _repoMock.Verify(repo => repo.UpdateAsync(invoice), Times.Once);
    }

    [Fact]
    public async Task UpdateInvoiceAsync_WhenInvoiceIsPaid_ThrowsException()
    {
        var invoice = new Invoice
        {
            InvoiceId = 3,
            Status = InvoiceStatus.Paid
        };

        _repoMock
            .Setup(repo => repo.GetByIdAsync(3))
            .ReturnsAsync(invoice);

        var dto = new CreateInvoiceDto
        {
            CustomerId = 2,
            InvoiceDate = new DateTime(2026, 4, 6),
            DueDate = new DateTime(2026, 5, 6),
            Discount = 5,
            Tax = 10
        };

        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _service.UpdateInvoiceAsync(3, dto));

        Assert.Contains("Cannot update a paid invoice", exception.Message);
    }

    [Fact]
    public async Task UpdateInvoiceAsync_WhenInvoiceExists_UpdatesHeaderFieldsAndRecalculatesGrandTotal()
    {
        var invoice = new Invoice
        {
            InvoiceId = 7,
            CustomerId = 1,
            SubTotal = 500,
            Tax = 20,
            Discount = 10,
            GrandTotal = 510,
            Status = InvoiceStatus.Draft
        };

        _repoMock
            .Setup(repo => repo.GetByIdAsync(7))
            .ReturnsAsync(invoice);
        _repoMock
            .Setup(repo => repo.UpdateAsync(invoice))
            .ReturnsAsync(invoice);

        var dto = new CreateInvoiceDto
        {
            CustomerId = 9,
            InvoiceDate = new DateTime(2026, 4, 6),
            DueDate = new DateTime(2026, 4, 20),
            Discount = 30,
            Tax = 40
        };

        var result = await _service.UpdateInvoiceAsync(7, dto);

        Assert.NotNull(result);
        Assert.Equal(9, invoice.CustomerId);
        Assert.Equal(510m, invoice.GrandTotal);
        Assert.Equal(dto.InvoiceDate, invoice.InvoiceDate);
        Assert.Equal(dto.DueDate, invoice.DueDate);
        _repoMock.Verify(repo => repo.UpdateAsync(invoice), Times.Once);
    }

    [Fact]
    public async Task DeleteInvoiceAsync_WhenPaymentsExist_ThrowsException()
    {
        var invoice = new Invoice
        {
            InvoiceId = 11,
            Payments = new List<Payment>
            {
                new() { PaymentAmount = 50 }
            }
        };

        _repoMock
            .Setup(repo => repo.GetByIdAsync(11))
            .ReturnsAsync(invoice);

        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _service.DeleteInvoiceAsync(11));

        Assert.Contains("Cannot delete invoice with payments", exception.Message);
        _repoMock.Verify(repo => repo.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteInvoiceAsync_WhenNoPayments_DeletesInvoice()
    {
        var invoice = new Invoice
        {
            InvoiceId = 12,
            Payments = new List<Payment>()
        };

        _repoMock
            .Setup(repo => repo.GetByIdAsync(12))
            .ReturnsAsync(invoice);
        _repoMock
            .Setup(repo => repo.DeleteAsync(12))
            .Returns(Task.CompletedTask);

        await _service.DeleteInvoiceAsync(12);

        _repoMock.Verify(repo => repo.DeleteAsync(12), Times.Once);
    }
}
