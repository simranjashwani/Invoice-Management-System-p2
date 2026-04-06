using System.Collections.Generic;
using System.Threading.Tasks;
using InvoiceManagementSystem.DAL.Entities;

namespace InvoiceManagementSystem.DAL.Repositories
{
    public interface IInvoiceLineItemRepository
    {
        Task<IEnumerable<InvoiceLineItem>> GetByInvoiceIdAsync(int invoiceId);
        Task<InvoiceLineItem?> GetByIdAsync(int lineItemId);
        Task<InvoiceLineItem> AddAsync(InvoiceLineItem item);
        Task<InvoiceLineItem?> UpdateAsync(InvoiceLineItem item);
        Task<bool> DeleteAsync(int lineItemId);
    }
}