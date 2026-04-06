using InvoiceManagementSystem.BLL.Models;

namespace InvoiceManagementSystem.BLL.Interfaces
{
    public interface IInvoiceLineItemService
    {
        Task<IEnumerable<InvoiceLineItemDto>> GetByInvoiceIdAsync(int invoiceId);
        Task<InvoiceLineItemDto> AddLineItemAsync(int invoiceId, AddInvoiceLineItemDto dto);
        Task<InvoiceLineItemDto?> UpdateLineItemAsync(int invoiceId, int itemId, UpdateInvoiceLineItemDto dto);
        Task<bool> DeleteLineItemAsync(int invoiceId, int itemId);
    }
}