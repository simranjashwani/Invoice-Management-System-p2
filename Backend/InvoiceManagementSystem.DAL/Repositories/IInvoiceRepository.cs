using InvoiceManagementSystem.DAL.Entities;

namespace InvoiceManagementSystem.DAL.Repositories
{
    public interface IInvoiceRepository
    {
        Task<Invoice> CreateAsync(Invoice invoice);

        Task<Invoice?> GetByIdAsync(int id);

        Task<List<Invoice>> GetAllAsync();

        Task<Invoice> UpdateAsync(Invoice invoice);

        Task DeleteAsync(int id);
        

        Task<Invoice?> GetLastInvoiceAsync();

       
    }
}