using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using InvoiceManagementSystem.DAL.Data;

namespace InvoiceManagementSystem.DAL.Factory
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=LAPTOP-A9K6HH4U\\SQLEXPRESS;Database=InvoiceManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
            );

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}







