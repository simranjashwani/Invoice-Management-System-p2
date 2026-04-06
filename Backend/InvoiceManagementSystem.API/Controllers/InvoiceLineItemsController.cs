using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InvoiceManagementSystem.BLL.Interfaces;
using InvoiceManagementSystem.BLL.Models;

namespace InvoiceManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/invoices/{invoiceId}/items")]
    public class InvoiceLineItemsController : ControllerBase
    {
        private readonly IInvoiceLineItemService _lineItemService;

        public InvoiceLineItemsController(IInvoiceLineItemService lineItemService)
        {
            _lineItemService = lineItemService;
        }

        [Authorize(Roles = "FinanceUser,FinanceManager,Admin")]
        [HttpGet]
        public async Task<IActionResult> GetByInvoiceId(int invoiceId)
        {
            if (invoiceId <= 0)
                return BadRequest("Invalid invoice ID");

            var items = await _lineItemService.GetByInvoiceIdAsync(invoiceId);
            return Ok(items);
        }

        [Authorize(Roles = "FinanceUser,Admin")]
        [HttpPost]
        public async Task<IActionResult> AddLineItem(int invoiceId, [FromBody] AddInvoiceLineItemDto dto)
        {
            if (invoiceId <= 0)
                return BadRequest("Invalid invoice ID");

            var createdItem = await _lineItemService.AddLineItemAsync(invoiceId, dto);
            return Ok(createdItem);
        }

        [Authorize(Roles = "FinanceUser,FinanceManager,Admin")]
        [HttpPut("{itemId}")]
        public async Task<IActionResult> UpdateLineItem(int invoiceId, int itemId, [FromBody] UpdateInvoiceLineItemDto dto)
        {
            if (invoiceId <= 0 || itemId <= 0)
                return BadRequest("Invalid invoice ID or item ID");

            var updatedItem = await _lineItemService.UpdateLineItemAsync(invoiceId, itemId, dto);

            if (updatedItem == null)
                return NotFound("Line item not found");

            return Ok(updatedItem);
        }

        [Authorize(Roles = "Admin,FinanceUser")]
        [HttpDelete("{itemId}")]
        public async Task<IActionResult> DeleteLineItem(int invoiceId, int itemId)
        {
            if (invoiceId <= 0 || itemId <= 0)
                return BadRequest("Invalid invoice ID or item ID");

            var deleted = await _lineItemService.DeleteLineItemAsync(invoiceId, itemId);

            if (!deleted)
                return NotFound("Line item not found");

            return Ok("Line item deleted successfully");
        }
    }
}