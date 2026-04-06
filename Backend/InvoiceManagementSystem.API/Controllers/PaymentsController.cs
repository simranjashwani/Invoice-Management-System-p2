using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using InvoiceManagementSystem.BLL.Models;
using InvoiceManagementSystem.DAL.Data;
using InvoiceManagementSystem.DAL.Entities;
using System.Linq;
using InvoiceManagementSystem.DAL.Enums;

namespace InvoiceManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/invoices/{id}/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly InvoiceAnalyticsService _analyticsService;

        public PaymentsController(
            ApplicationDbContext context,
            InvoiceAnalyticsService analyticsService)
        {
            _context = context;
            _analyticsService = analyticsService;
        }

        [Authorize(Roles = "FinanceUser, FinanceManager, Admin")]
        [HttpPost]
        public async Task<IActionResult> AddPayment(int id, [FromBody] CreatePaymentDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Invalid payment data." });

            var invoice = await _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
                return NotFound(new { message = "Invoice not found." });

            if (dto.PaymentAmount <= 0)
                return BadRequest(new { message = "Payment amount must be greater than zero." });

            if (dto.PaymentAmount > invoice.OutstandingBalance)
                return BadRequest(new { message = "Payment amount cannot exceed the outstanding balance." });

            var payment = new Payment
            {
                InvoiceId = id,
                PaymentAmount = dto.PaymentAmount,
                PaymentDate = dto.PaymentDate,
                PaymentMethod = dto.PaymentMethod,
                ReferenceNumber = dto.ReferenceNumber,
                ReceivedDate = dto.ReceivedDate
            };

            _context.Payments.Add(payment);

            var totalPaid = invoice.Payments.Sum(p => p.PaymentAmount) + dto.PaymentAmount;

            invoice.OutstandingBalance = invoice.GrandTotal - totalPaid;

            if (invoice.OutstandingBalance <= 0)
            {
                invoice.OutstandingBalance = 0;
                invoice.Status = InvoiceStatus.Paid;
            }
            else
            {
                invoice.Status = InvoiceStatus.PartiallyPaid;
            }

            await _context.SaveChangesAsync();
            await _analyticsService.ClearCache();

            return Ok(new
            {
                message = "Payment added successfully",
                invoiceId = invoice.InvoiceId,
                outstandingBalance = invoice.OutstandingBalance,
                status = invoice.Status.ToString()
            });
        }

        [Authorize(Roles = "FinanceUser,FinanceManager,Admin")]
        [HttpGet]
        public async Task<IActionResult> GetPayments(int id)
        {
            var payments = await _context.Payments
                .Where(p => p.InvoiceId == id)
                .ToListAsync();

            return Ok(payments);
        }
    }
}
