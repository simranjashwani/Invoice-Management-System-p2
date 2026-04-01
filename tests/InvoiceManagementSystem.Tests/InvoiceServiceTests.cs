using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;

using InvoiceManagementSystem.BLL.Services;
using InvoiceManagementSystem.DAL.Entities;
using InvoiceManagementSystem.DAL.Enums;
using InvoiceManagementSystem.DAL.Repositories;

namespace InvoiceManagementSystem.Tests
{
    public class InvoiceServiceTests : IDisposable
    {
        private readonly Mock<IInvoiceRepository> _repoMock;
        private readonly InvoiceService _service;

        public InvoiceServiceTests()
        {
            _repoMock = new Mock<IInvoiceRepository>(MockBehavior.Strict);
            _service = new InvoiceService(_repoMock.Object);
        }

        public void Dispose()
        {
            // teardown if needed
        }

        // ---------------------------
        // CreateInvoiceAsync tests
        // ---------------------------

        [Fact]
        public async Task CreateInvoiceAsync_WhenNoPreviousInvoice_GeneratesINV0001_AndCalculatesTotals()
        {
            // Arrange
            _repoMock.Setup(r => r.GetLastInvoiceAsync()).ReturnsAsync((Invoice?)null);

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Invoice>()))
                .ReturnsAsync((Invoice inv) => inv);

            var invoice = new Invoice
            {
                LineItems = new List<InvoiceLineItem>
                {
                    new InvoiceLineItem { Quantity = 2, UnitPrice = 100, Tax = 10, Discount = 5 },
                    new InvoiceLineItem { Quantity = 1, UnitPrice = 50,  Tax = 0,  Discount = 0 }
                },
                Payments = new List<Payment>()
            };

            // Act
            var result = await _service.CreateInvoiceAsync(invoice);

            // Assert
            Assert.Equal("INV-0001", result.InvoiceNumber);

            // subtotal = 2*100 + 1*50 = 250
            Assert.Equal(250m, result.SubTotal);

            // tax = 10 + 0 = 10
            Assert.Equal(10m, result.Tax);

            // discount = 5 + 0 = 5
            Assert.Equal(5m, result.Discount);

            // grand = 250 + 10 - 5 = 255
            Assert.Equal(255m, result.GrandTotal);

            Assert.Equal(InvoiceStatus.Draft, result.Status);
            Assert.True((DateTime.Now - result.CreatedDate).TotalSeconds < 5);

            _repoMock.Verify(r => r.GetLastInvoiceAsync(), Times.Once);
            _repoMock.Verify(r => r.CreateAsync(It.IsAny<Invoice>()), Times.Once);
        }

        [Fact]
        public async Task CreateInvoiceAsync_WhenPreviousInvoiceExists_IncrementsInvoiceNumber()
        {
            // Arrange
            _repoMock.Setup(r => r.GetLastInvoiceAsync())
                .ReturnsAsync(new Invoice { InvoiceNumber = "INV-0009" });

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Invoice>()))
                .ReturnsAsync((Invoice inv) => inv);

            var invoice = new Invoice
            {
                LineItems = new List<InvoiceLineItem>
                {
                    new InvoiceLineItem { Quantity = 1, UnitPrice = 100, Tax = 0, Discount = 0 }
                }
            };

            // Act
            var result = await _service.CreateInvoiceAsync(invoice);

            // Assert
            Assert.Equal("INV-0010", result.InvoiceNumber);
            _repoMock.Verify(r => r.CreateAsync(It.IsAny<Invoice>()), Times.Once);
        }

        // ---------------------------
        // ChangeInvoiceStatusAsync tests
        // ---------------------------

        [Fact]
        public async Task ChangeInvoiceStatusAsync_ValidTransition_DraftToSent_UpdatesInvoice()
        {
            // Arrange
            var inv = new Invoice { InvoiceId = 1, Status = InvoiceStatus.Draft };

            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(inv);
            _repoMock.Setup(r => r.UpdateAsync(inv)).Returns(Task.CompletedTask);

            // Act
            await _service.ChangeInvoiceStatusAsync(1, InvoiceStatus.Sent);

            // Assert
            Assert.Equal(InvoiceStatus.Sent, inv.Status);
            _repoMock.Verify(r => r.UpdateAsync(inv), Times.Once);
        }

        [Fact]
        public async Task ChangeInvoiceStatusAsync_InvalidTransition_DraftToPaid_Throws()
        {
            // Arrange
            var inv = new Invoice { InvoiceId = 1, Status = InvoiceStatus.Draft };

            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(inv);

            // Act + Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.ChangeInvoiceStatusAsync(1, InvoiceStatus.Paid));

            Assert.Contains("Invalid status transition", ex.Message);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Invoice>()), Times.Never);
        }

        [Fact]
        public async Task ChangeInvoiceStatusAsync_WhenInvoiceNotFound_Throws()
        {
            _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Invoice?)null);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.ChangeInvoiceStatusAsync(99, InvoiceStatus.Sent));

            Assert.Contains("Invoice not found", ex.Message);
        }

        // ---------------------------
        // AddPaymentAsync tests
        // ---------------------------

        [Fact]
        public async Task AddPaymentAsync_WhenInvoiceNotFound_Throws()
        {
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Invoice?)null);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.AddPaymentAsync(1, 100, "UPI"));

            Assert.Contains("Invoice not found", ex.Message);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Invoice>()), Times.Never);
        }

        [Fact]
        public async Task AddPaymentAsync_WhenAmountIsZeroOrNegative_Throws()
        {
            var inv = new Invoice
            {
                InvoiceId = 1,
                GrandTotal = 500,
                Status = InvoiceStatus.Sent,
                Payments = new List<Payment>()
            };

            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(inv);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.AddPaymentAsync(1, 0, "Cash"));

            Assert.Contains("greater than zero", ex.Message);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Invoice>()), Times.Never);
        }

        [Fact]
        public async Task AddPaymentAsync_WhenPaymentExceedsRemaining_Throws()
        {
            var inv = new Invoice
            {
                InvoiceId = 1,
                GrandTotal = 200,
                Status = InvoiceStatus.PartiallyPaid,
                Payments = new List<Payment>
                {
                    new Payment { PaymentAmount = 150 }
                }
            };

            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(inv);

            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.AddPaymentAsync(1, 100, "Card"));

            Assert.Contains("exceeds remaining balance", ex.Message);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Invoice>()), Times.Never);
        }

        [Fact]
        public async Task AddPaymentAsync_WhenPartialPayment_SetsStatusPartiallyPaid_AndUpdates()
        {
            var inv = new Invoice
            {
                InvoiceId = 1,
                GrandTotal = 500,
                Status = InvoiceStatus.Sent,
                Payments = new List<Payment>()
            };

            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(inv);
            _repoMock.Setup(r => r.UpdateAsync(inv)).Returns(Task.CompletedTask);

            await _service.AddPaymentAsync(1, 200, "Cash");

            Assert.Single(inv.Payments);
            Assert.Equal(200m, inv.Payments.First().PaymentAmount);
            Assert.Equal(InvoiceStatus.PartiallyPaid, inv.Status);

            _repoMock.Verify(r => r.UpdateAsync(inv), Times.Once);
        }

        [Fact]
        public async Task AddPaymentAsync_WhenFullPayment_SetsStatusPaid_AndUpdates()
        {
            var inv = new Invoice
            {
                InvoiceId = 1,
                GrandTotal = 300,
                Status = InvoiceStatus.Sent,
                Payments = new List<Payment>()
            };

            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(inv);
            _repoMock.Setup(r => r.UpdateAsync(inv)).Returns(Task.CompletedTask);

            await _service.AddPaymentAsync(1, 300, "UPI");

            Assert.Single(inv.Payments);
            Assert.Equal(InvoiceStatus.Paid, inv.Status);

            _repoMock.Verify(r => r.UpdateAsync(inv), Times.Once);
        }

        // ---------------------------
        // UpdateOverdueInvoicesAsync tests
        // ---------------------------

        [Fact]
        public async Task UpdateOverdueInvoicesAsync_MarksEligibleInvoicesAsOverdue_AndUpdates()
        {
            var now = DateTime.Now;

            var inv1 = new Invoice { InvoiceId = 1, DueDate = now.AddDays(-1), Status = InvoiceStatus.Sent };
            var inv2 = new Invoice { InvoiceId = 2, DueDate = now.AddDays(-10), Status = InvoiceStatus.PartiallyPaid };
            var inv3 = new Invoice { InvoiceId = 3, DueDate = now.AddDays(-5), Status = InvoiceStatus.Paid };      // ignore
            var inv4 = new Invoice { InvoiceId = 4, DueDate = now.AddDays(5), Status = InvoiceStatus.Sent };       // not overdue
            var inv5 = new Invoice { InvoiceId = 5, DueDate = now.AddDays(-2), Status = InvoiceStatus.Overdue };   // already overdue

            var invoices = new List<Invoice> { inv1, inv2, inv3, inv4, inv5 };

            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(invoices);

            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Invoice>()))
                .Returns(Task.CompletedTask);

            using var sw = new StringWriter();
            Console.SetOut(sw);

            await _service.UpdateOverdueInvoicesAsync();

            Assert.Equal(InvoiceStatus.Overdue, inv1.Status);
            Assert.Equal(InvoiceStatus.Overdue, inv2.Status);
            Assert.Equal(InvoiceStatus.Paid, inv3.Status);
            Assert.Equal(InvoiceStatus.Sent, inv4.Status);
            Assert.Equal(InvoiceStatus.Overdue, inv5.Status);

            _repoMock.Verify(r => r.UpdateAsync(It.Is<Invoice>(x => x.InvoiceId == 1)), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.Is<Invoice>(x => x.InvoiceId == 2)), Times.Once);

            // should NOT update these
            _repoMock.Verify(r => r.UpdateAsync(It.Is<Invoice>(x => x.InvoiceId == 3)), Times.Never);
            _repoMock.Verify(r => r.UpdateAsync(It.Is<Invoice>(x => x.InvoiceId == 4)), Times.Never);
            _repoMock.Verify(r => r.UpdateAsync(It.Is<Invoice>(x => x.InvoiceId == 5)), Times.Never);

            Assert.Contains("Overdue invoices updated", sw.ToString());
        }

        // ---------------------------
        // Console-output methods
        // ---------------------------

        [Fact]
        public async Task CalculateDsoAsync_PrintsDsoReport()
        {
            var invoices = new List<Invoice>
            {
                new Invoice
                {
                    Status = InvoiceStatus.Sent,
                    GrandTotal = 1000,
                    Payments = new List<Payment> { new Payment { PaymentAmount = 200 } }
                },
                new Invoice
                {
                    Status = InvoiceStatus.Cancelled, // ignored
                    GrandTotal = 500,
                    Payments = new List<Payment>()
                }
            };

            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(invoices);

            using var sw = new StringWriter();
            Console.SetOut(sw);

            await _service.CalculateDsoAsync();

            var output = sw.ToString();
            Assert.Contains("DSO REPORT", output);
            Assert.Contains("Total Sales", output);
            Assert.Contains("Total Outstanding", output);
        }

        [Fact]
        public async Task GenerateAgingReportAsync_PrintsAgingBucketsAndOutstanding()
        {
            var today = DateTime.Now;

            var invoices = new List<Invoice>
            {
                new Invoice
                {
                    InvoiceNumber = "INV-0001",
                    Status = InvoiceStatus.Sent,
                    DueDate = today.AddDays(-10),
                    GrandTotal = 500,
                    Payments = new List<Payment> { new Payment { PaymentAmount = 100 } }
                },
                new Invoice
                {
                    InvoiceNumber = "INV-0002",
                    Status = InvoiceStatus.Paid, // ignored
                    DueDate = today.AddDays(-40),
                    GrandTotal = 300,
                    Payments = new List<Payment>()
                }
            };

            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(invoices);

            using var sw = new StringWriter();
            Console.SetOut(sw);

            await _service.GenerateAgingReportAsync();

            var output = sw.ToString();
            Assert.Contains("AGING REPORT", output);
            Assert.Contains("0-30 Days", output);
            Assert.Contains("INV-0001", output);
            Assert.Contains("Outstanding: 400", output); // 500-100
        }
    }
}