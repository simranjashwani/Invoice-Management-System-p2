using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InvoiceManagementSystem.BLL.CQRS.Commands;
using InvoiceManagementSystem.BLL.Models;
using Microsoft.EntityFrameworkCore;
using InvoiceManagementSystem.DAL.Data;

[Authorize]
[ApiController]
[Route("api/invoices/{id}/payments")]
public class PaymentsController : ControllerBase
{
    private readonly CreatePaymentHandler _handler;
    private readonly ApplicationDbContext _context;

    public PaymentsController(CreatePaymentHandler handler, ApplicationDbContext context)
    {
        _handler = handler;
        _context = context;
    }

[Authorize(Roles = "FinanceUser, Admin")]
   [HttpPost]
public async Task<IActionResult> AddPayment(int id, CreatePaymentDto dto)
{
    var command = new CreatePaymentCommand
    {
        InvoiceId = id,
        PaymentAmount = dto.PaymentAmount,
        PaymentMethod = dto.PaymentMethod,
        ReferenceNumber = dto.ReferenceNumber
    };

    var result = await _handler.Handle(command);

    return Ok(result);
}

[Authorize(Roles = "FinanceUser, FinanceManager, Admin")]
[HttpGet]
public async Task<IActionResult> GetPayments(int id)
{
    var payments = await _context.Payments
        .Where(p => p.InvoiceId == id)
        .ToListAsync();

    return Ok(payments);
}
}