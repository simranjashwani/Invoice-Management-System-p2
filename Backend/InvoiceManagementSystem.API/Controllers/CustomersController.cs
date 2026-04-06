using InvoiceManagementSystem.DAL.Repositories;
using InvoiceManagementSystem.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceManagementSystem.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomersController(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        [Authorize(Roles = "FinanceUser, FinanceManager, Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await _customerRepository.GetAllAsync();
            return Ok(customers.OrderBy(customer => customer.Name));
        }

        [Authorize(Roles = "FinanceUser, FinanceManager, Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.Name))
            {
                return BadRequest(new { message = "Customer name is required." });
            }

            if (string.IsNullOrWhiteSpace(customer.Email))
            {
                return BadRequest(new { message = "Customer email is required." });
            }

            if (string.IsNullOrWhiteSpace(customer.Phone))
            {
                return BadRequest(new { message = "Customer phone is required." });
            }

            if (string.IsNullOrWhiteSpace(customer.Address))
            {
                return BadRequest(new { message = "Customer address is required." });
            }

            var newCustomer = new Customer
            {
                Name = customer.Name.Trim(),
                Email = customer.Email.Trim(),
                Phone = customer.Phone.Trim(),
                Address = customer.Address.Trim()
            };

            await _customerRepository.AddAsync(newCustomer);
            return Ok(newCustomer);
        }
    }
}
