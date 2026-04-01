using Microsoft.EntityFrameworkCore;
using InvoiceManagementSystem.DAL.Data;
using InvoiceManagementSystem.DAL.Entities;

namespace InvoiceManagementSystem.DAL.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly ApplicationDbContext _context;

        public InvoiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Invoice> CreateAsync(Invoice invoice)
        {
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task<Invoice?> GetByIdAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.LineItems)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);
        }

        public async Task<List<Invoice>> GetAllAsync()
        {
            return await _context.Invoices
                .Include(i => i.LineItems)
                .Include(i => i.Payments)
                .ToListAsync();
        }

        public async Task<Invoice> UpdateAsync(Invoice invoice)
{
    _context.Invoices.Update(invoice);
    await _context.SaveChangesAsync();
    return invoice;
}

       public async Task DeleteAsync(int id)
{
    var invoice = await _context.Invoices.FindAsync(id);

    if (invoice != null)
    {
        _context.Invoices.Remove(invoice);
        await _context.SaveChangesAsync();
    }
}

        public async Task<Invoice?> GetLastInvoiceAsync()
{
    return await _context.Invoices
        .OrderByDescending(i => i.InvoiceId)
        .FirstOrDefaultAsync();
}


    }
}
