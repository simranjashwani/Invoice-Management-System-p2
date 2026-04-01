



// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.DependencyInjection;
// using InvoiceManagementSystem.DAL.Data;
// using InvoiceManagementSystem.DAL.Repositories;
// using InvoiceManagementSystem.BLL.Interfaces;
// using InvoiceManagementSystem.BLL.Services;
// using InvoiceManagementSystem.DAL.Entities;

// using InvoiceManagementSystem.DAL.Enums;



// // Setup DI
// var services = new ServiceCollection();

// services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseSqlServer("Server=LAPTOP-A9K6HH4U\\SQLEXPRESS;Database=InvoiceManagementDB;Trusted_Connection=True;TrustServerCertificate=True;")
// );

// services.AddScoped<IInvoiceRepository, InvoiceRepository>();
// services.AddScoped<IInvoiceService, InvoiceService>();

// var serviceProvider = services.BuildServiceProvider();

// var invoiceService = serviceProvider.GetRequiredService<IInvoiceService>();

// // Create sample invoice
// var invoice = new Invoice
// {
//     //InvoiceNumber = "INV-002",
//     CustomerId = 1,
//     DueDate = DateTime.Now.AddDays(30),
//     LineItems =
//     [
//         new InvoiceLineItem
//         {
//             Description = "Product A",
//             Quantity = 2,
//             UnitPrice = 500,
//             Tax = 50,
//             Discount = 20
//         },
//         new InvoiceLineItem
//         {
//             Description = "Product B",
//             Quantity = 1,
//             UnitPrice = 1000,
//             Tax = 100,
//             Discount = 50
//         }
//     ]
// };

// await invoiceService.CreateInvoiceAsync(invoice);



// try
// {
//     Console.WriteLine("Changing status from Draft to Sent...");
//     await invoiceService.ChangeInvoiceStatusAsync(2, InvoiceStatus.Sent);
//     Console.WriteLine("Status changed to Sent successfully.");

//     Console.WriteLine("Trying invalid transition: Sent → Draft...");
//     await invoiceService.ChangeInvoiceStatusAsync(1, InvoiceStatus.Draft);
// }
// catch (Exception ex)
// {
//     Console.WriteLine($"Error: {ex.Message}");
// }

// Console.WriteLine("Invoice created successfully!");

// // Fetch and display invoices
// var invoices = await invoiceService.GetAllInvoicesAsync();

// foreach (var inv in invoices)
// {
//     Console.WriteLine($"Invoice: {inv.InvoiceNumber}");
//     Console.WriteLine($"Total: {inv.GrandTotal}");
//     Console.WriteLine($"Status: {inv.Status}");
//     Console.WriteLine("--------------------------");
// }

// // Add Payment Test
// Console.WriteLine("Adding payment of 1000...");

// await invoiceService.AddPaymentAsync(1, 1000, "UPI");

// Console.WriteLine("Payment added.");

// // Fetch updated invoice
// var updatedInvoice = await invoiceService.GetInvoiceByIdAsync(1);

// Console.WriteLine($"Updated Status: {updatedInvoice?.Status}");
// Console.WriteLine($"Total Paid: {updatedInvoice?.Payments.Sum(p => p.PaymentAmount)}");






using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using InvoiceManagementSystem.DAL.Data;
using InvoiceManagementSystem.DAL.Repositories;
using InvoiceManagementSystem.BLL.Interfaces;
using InvoiceManagementSystem.BLL.Services;
using InvoiceManagementSystem.DAL.Entities;
using InvoiceManagementSystem.DAL.Enums;

var services = new ServiceCollection();

services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer("Server=LAPTOP-A9K6HH4U\\SQLEXPRESS;Database=InvoiceManagementDB;Trusted_Connection=True;TrustServerCertificate=True;")
);

services.AddScoped<IInvoiceRepository, InvoiceRepository>();
services.AddScoped<IInvoiceService, InvoiceService>();
services.AddScoped<ICustomerRepository, CustomerRepository>();

var serviceProvider = services.BuildServiceProvider();
var invoiceService = serviceProvider.GetRequiredService<IInvoiceService>();
var customerRepository = serviceProvider.GetRequiredService<ICustomerRepository>();

while (true)
{
    Console.WriteLine("\n===== INVOICE MANAGEMENT SYSTEM =====");
   Console.WriteLine("1. Create Customer");
Console.WriteLine("2. Create Invoice");
Console.WriteLine("3. View All Invoices");
Console.WriteLine("4. Add Payment");
Console.WriteLine("5. Change Invoice Status");
Console.WriteLine("6. Aging Report");
Console.WriteLine("7. Update Overdue Invoices");
Console.WriteLine("8. Calculate DSO");
Console.WriteLine("9. Revenue Summary Report");
Console.WriteLine("10. Outstanding Per Customer");
Console.WriteLine("11. Reconciliation Report");
Console.WriteLine("12. Top 5 Unpaid Invoices");
Console.WriteLine("13. Exit");
    Console.Write("Select Option: ");


    var choice = Console.ReadLine();

    switch (choice)
    {

        case "1":
    await CreateCustomer(customerRepository);
    break;
        case "2":
            await CreateInvoice(invoiceService, customerRepository);
            break;

        case "3":
            await ViewInvoices(invoiceService);
            break;

        case "4":
            await AddPayment(invoiceService);
            break;

        case "5":
            await ChangeStatus(invoiceService);
            break;

        case "6":
    await invoiceService.GenerateAgingReportAsync();
    break;

case "7":
    await invoiceService.UpdateOverdueInvoicesAsync();
    break;

case "8":
    await invoiceService.CalculateDsoAsync();
    break;

case "9":
    await invoiceService.GenerateRevenueSummaryAsync();
    break;

case "10":
    await invoiceService.GenerateCustomerOutstandingReportAsync();
    break;

case "11":
    await invoiceService.GenerateReconciliationReportAsync();
    break;

case "12":
    await invoiceService.GenerateTopUnpaidInvoicesAsync();
    break;

case "13":
    return;

        default:
            Console.WriteLine("Invalid choice.");
            break;
    }
}

async Task CreateInvoice(
    IInvoiceService invoiceService,
    ICustomerRepository customerRepository)
{
    var customers = await customerRepository.GetAllAsync();

if (!customers.Any())
{
    Console.WriteLine("No customers found. Please create a customer first.");
    return;
}

Console.WriteLine("Select Customer:");

foreach (var customer in customers)
{
    Console.WriteLine($"{customer.CustomerId}. {customer.Name}");
}

Console.Write("Enter Customer ID: ");
int customerId = int.Parse(Console.ReadLine()!);

if (!customers.Any(c => c.CustomerId == customerId))
{
    Console.WriteLine("Invalid Customer ID.");
    return;
}

    Console.Write("Enter Due Days (e.g. 30): ");
    int dueDays = int.Parse(Console.ReadLine()!);

    var lineItems = new List<InvoiceLineItem>();

    while (true)
    {
        Console.WriteLine("\n--- Add Line Item ---");

        Console.Write("Description: ");
        string description = Console.ReadLine()!;

        Console.Write("Quantity: ");
        int quantity = int.Parse(Console.ReadLine()!);

        Console.Write("Unit Price: ");
        decimal unitPrice = decimal.Parse(Console.ReadLine()!);

        Console.Write("Tax: ");
        decimal tax = decimal.Parse(Console.ReadLine()!);

        Console.Write("Discount: ");
        decimal discount = decimal.Parse(Console.ReadLine()!);

        lineItems.Add(new InvoiceLineItem
        {
            Description = description,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Tax = tax,
            Discount = discount
        });

        Console.Write("Add another line item? (y/n): ");
        var more = Console.ReadLine();

        if (more?.ToLower() != "y")
            break;
    }

    var invoice = new Invoice
    {
        CustomerId = customerId,
        DueDate = DateTime.Now.AddDays(dueDays),
        LineItems = lineItems
    };

    await invoiceService.CreateInvoiceAsync(invoice);

    Console.WriteLine("Invoice created successfully!");
}

async Task ViewInvoices(IInvoiceService invoiceService)
{
    var invoices = await invoiceService.GetAllInvoicesAsync();

    foreach (var inv in invoices)
    {
        Console.WriteLine("\n====================================");
        Console.WriteLine($"Invoice ID: {inv.InvoiceId}");
        Console.WriteLine($"Number: {inv.InvoiceNumber}");
        Console.WriteLine($"Customer ID: {inv.CustomerId}");
        Console.WriteLine($"Status: {inv.Status}");
        Console.WriteLine($"Invoice Date: {inv.InvoiceDate}");
        Console.WriteLine($"Due Date: {inv.DueDate}");
        Console.WriteLine("------------------------------------");

        Console.WriteLine("Line Items:");
        foreach (var item in inv.LineItems)
        {
            Console.WriteLine($"  - {item.Description}");
            Console.WriteLine($"      Qty: {item.Quantity}");
            Console.WriteLine($"      Unit Price: {item.UnitPrice}");
            Console.WriteLine($"      Tax: {item.Tax}");
            Console.WriteLine($"      Discount: {item.Discount}");
            Console.WriteLine($"      Line Total: {item.Quantity * item.UnitPrice + item.Tax - item.Discount}");
            Console.WriteLine();
        }

        Console.WriteLine("------------------------------------");
        Console.WriteLine($"SubTotal: {inv.SubTotal}");
        Console.WriteLine($"Tax: {inv.Tax}");
        Console.WriteLine($"Discount: {inv.Discount}");
        Console.WriteLine($"Grand Total: {inv.GrandTotal}");

        Console.WriteLine("------------------------------------");
        Console.WriteLine("Payments:");
        foreach (var payment in inv.Payments)
        {
            Console.WriteLine($"  - {payment.PaymentAmount} via {payment.PaymentMethod} on {payment.PaymentDate}");
        }

        Console.WriteLine($"Total Paid: {inv.Payments.Sum(p => p.PaymentAmount)}");
        Console.WriteLine("====================================\n");
    }
}

async Task AddPayment(IInvoiceService invoiceService)
{
    Console.Write("Enter Invoice ID: ");
    int id = int.Parse(Console.ReadLine()!);

    Console.Write("Enter Payment Amount: ");
    decimal amount = decimal.Parse(Console.ReadLine()!);

    Console.Write("Enter Payment Method: ");
    string method = Console.ReadLine()!;

    try
    {
        await invoiceService.AddPaymentAsync(id, amount, method);
        Console.WriteLine("Payment added successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

async Task ChangeStatus(IInvoiceService invoiceService)
{
    Console.Write("Enter Invoice ID: ");
    int id = int.Parse(Console.ReadLine()!);

    Console.WriteLine("Select New Status:");
    foreach (var status in Enum.GetValues<InvoiceStatus>())
    {
        Console.WriteLine($"{(int)status} - {status}");
    }

    int statusChoice = int.Parse(Console.ReadLine()!);
    var newStatus = (InvoiceStatus)statusChoice;

    try
    {
        await invoiceService.ChangeInvoiceStatusAsync(id, newStatus);
        Console.WriteLine("Status updated successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

async Task CreateCustomer(ICustomerRepository customerRepo)
{
    Console.Write("Enter Customer Name: ");
    string name = Console.ReadLine()!;

    Console.Write("Enter Email: ");
    string email = Console.ReadLine()!;

    Console.Write("Enter Phone: ");
    string phone = Console.ReadLine()!;

    Console.Write("Enter Address: ");
    string address = Console.ReadLine()!;

    var customer = new Customer
    {
        Name = name,
        Email = email,
        Phone = phone,
        Address = address
    };

    await customerRepo.AddAsync(customer);

    Console.WriteLine("Customer created successfully!");
}