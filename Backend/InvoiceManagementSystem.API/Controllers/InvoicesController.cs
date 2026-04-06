using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using InvoiceManagementSystem.BLL.Interfaces;
using InvoiceManagementSystem.BLL.Models;
using InvoiceManagementSystem.DAL.Entities;
using InvoiceManagementSystem.BLL.CQRS.Commands;
using InvoiceManagementSystem.BLL.CQRS.Queries;

namespace InvoiceManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;
        private readonly GetInvoiceByIdHandler _getHandler;
        private readonly CreateInvoiceHandler _createHandler;
        private readonly InvoiceAnalyticsService _analyticsService;

        public InvoicesController(
            IInvoiceService invoiceService,
            GetInvoiceByIdHandler getHandler,
            CreateInvoiceHandler createHandler,
            InvoiceAnalyticsService analyticsService)
        {
            _invoiceService = invoiceService;
            _getHandler = getHandler;
            _createHandler = createHandler;
            _analyticsService = analyticsService;
        }

        [Authorize(Roles = "FinanceUser, FinanceManager, Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllInvoices()
        {
            var invoices = await _invoiceService.GetAllInvoicesAsync();
            return Ok(invoices);
        }

        [Authorize(Roles = "FinanceUser, FinanceManager, Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoiceById(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid invoice ID");

            var query = new GetInvoiceByIdQuery(id);
            var invoice = await _getHandler.Handle(query);

            if (invoice == null)
                return NotFound(new { message = $"Invoice with ID {id} not found" });

            return Ok(invoice);
        }

        [Authorize(Roles = "FinanceUser, Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateInvoice(CreateInvoiceDto dto)
        {
            if (dto.LineItems == null || dto.LineItems.Count == 0)
                return BadRequest(new { message = "At least one line item is required." });

            var invoice = new Invoice
            {
                CustomerId = dto.CustomerId,
                QuoteId = dto.QuoteId,
                InvoiceDate = dto.InvoiceDate,
                DueDate = dto.DueDate,
                Discount = dto.Discount,
                Tax = dto.Tax,
                CreatedDate = DateTime.Now,
                LineItems = dto.LineItems.Select(item => new InvoiceLineItem
                {
                    Description = item.Description,
                    Quantity = (int)item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Discount = item.Discount,
                    Tax = item.Tax
                }).ToList()
            };

            var command = new CreateInvoiceCommand(invoice);
            var createdInvoice = await _createHandler.Handle(command);

            return Ok(createdInvoice);
        }

        [Authorize(Roles = "FinanceManager, Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInvoice(int id, CreateInvoiceDto dto)
        {
            if (id <= 0)
                return BadRequest("Invalid ID");

            var updatedInvoice = await _invoiceService.UpdateInvoiceAsync(id, dto);

            if (updatedInvoice == null)
                return NotFound($"Invoice with ID {id} not found");

            await _analyticsService.ClearCache();
            return Ok(updatedInvoice);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid ID");

            try
            {
                await _invoiceService.DeleteInvoiceAsync(id);
                await _analyticsService.ClearCache();
                return Ok("Invoice deleted successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
