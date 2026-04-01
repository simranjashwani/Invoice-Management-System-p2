using InvoiceManagementSystem.DAL.Entities;

namespace InvoiceManagementSystem.DAL.Repositories
{
    public interface ICustomerRepository
    {
        Task<List<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task AddAsync(Customer customer);
    }
}