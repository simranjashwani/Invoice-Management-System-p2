using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using InvoiceManagementSystem.DAL.Data;
using InvoiceManagementSystem.DAL.Entities;

namespace InvoiceManagementSystem.DAL.Repositories
{
    public class InvoiceLineItemRepository : IInvoiceLineItemRepository
    {
        private readonly ApplicationDbContext _context;

        public InvoiceLineItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InvoiceLineItem>> GetByInvoiceIdAsync(int invoiceId)
        {
            return await _context.InvoiceLineItems
                .Where(li => li.InvoiceId == invoiceId)
                .OrderBy(li => li.LineItemId)
                .ToListAsync();
        }

        public async Task<InvoiceLineItem?> GetByIdAsync(int lineItemId)
        {
            return await _context.InvoiceLineItems
                .FirstOrDefaultAsync(li => li.LineItemId == lineItemId);
        }

        public async Task<InvoiceLineItem> AddAsync(InvoiceLineItem item)
        {
            await _context.InvoiceLineItems.AddAsync(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<InvoiceLineItem?> UpdateAsync(InvoiceLineItem item)
        {
            var existingItem = await _context.InvoiceLineItems
                .FirstOrDefaultAsync(li => li.LineItemId == item.LineItemId);

            if (existingItem == null)
                return null;

            existingItem.Description = item.Description;
            existingItem.Quantity = item.Quantity;
            existingItem.UnitPrice = item.UnitPrice;
            existingItem.Discount = item.Discount;
            existingItem.Tax = item.Tax;
            existingItem.LineTotal = item.LineTotal;

            await _context.SaveChangesAsync();
            return existingItem;
        }

        public async Task<bool> DeleteAsync(int lineItemId)
        {
            var item = await _context.InvoiceLineItems
                .FirstOrDefaultAsync(li => li.LineItemId == lineItemId);

            if (item == null)
                return false;

            _context.InvoiceLineItems.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}