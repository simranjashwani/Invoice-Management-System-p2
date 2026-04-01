using Microsoft.AspNetCore.Mvc;
using InvoiceManagementSystem.BLL.Interfaces;
 using InvoiceManagementSystem.BLL.Models;
using InvoiceManagementSystem.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
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

       public InvoicesController(
    IInvoiceService invoiceService,
    GetInvoiceByIdHandler getHandler,
    CreateInvoiceHandler createHandler)
{
    _invoiceService = invoiceService;
    _getHandler = getHandler;
    _createHandler = createHandler;
}


        //  GET: api/invoices
        [Authorize(Roles = "FinanceUser, FinanceManager, Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllInvoices()
        {
            var invoices = await _invoiceService.GetAllInvoicesAsync();
            return Ok(invoices);
        }

        //  GET: api/invoices/{id}
        [Authorize(Roles = "FinanceUser, FinanceManager, Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoiceById(int id)
        {
            // Validation
            if (id <= 0)
                return BadRequest("Invalid invoice ID");

           // var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
           var query = new GetInvoiceByIdQuery(id);
var invoice = await _getHandler.Handle(query);

            if (invoice == null)
                return NotFound(new { message = $"Invoice with ID {id} not found" });

            return Ok(invoice);
        }

        [Authorize(Roles = "FinanceUser , Admin")]
[HttpPost]
public async Task<IActionResult> CreateInvoice(CreateInvoiceDto dto )
{
    //  Validation
    // if (dto.DueDate <= dto.InvoiceDate)
    //     return BadRequest("Due date must be greater than invoice date");

    //  Map DTO → Entity
    var invoice = new Invoice
    {
        CustomerId = dto.CustomerId,
        QuoteId = dto.QuoteId,
        InvoiceDate = dto.InvoiceDate,
        DueDate = dto.DueDate,
        Discount = dto.Discount,
        Tax = dto.Tax,
        CreatedDate = DateTime.Now
    };

    // Call service
    //var createdInvoice = await _invoiceService.CreateInvoiceAsync(invoice);
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
  //throw new Exception("Test exception");
    if (updatedInvoice == null)
        return NotFound($"Invoice with ID {id} not found");

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
        return Ok("Invoice deleted successfully");
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}
        }
    }
