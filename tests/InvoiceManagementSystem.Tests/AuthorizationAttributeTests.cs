using InvoiceManagementSystem.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace InvoiceManagementSystem.Tests;

public class AuthorizationAttributeTests
{
    [Fact]
    public void CreateInvoice_Has_FinanceUser_And_Admin_Authorization()
    {
        var method = typeof(InvoicesController).GetMethod("CreateInvoice");
        var attribute = Assert.Single(method!.GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>());

        Assert.Contains("FinanceUser", attribute.Roles ?? string.Empty);
        Assert.Contains("Admin", attribute.Roles ?? string.Empty);
    }

    [Fact]
    public void UpdateInvoice_Has_FinanceManager_And_Admin_Authorization()
    {
        var method = typeof(InvoicesController).GetMethod("UpdateInvoice");
        var attribute = Assert.Single(method!.GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>());

        Assert.Contains("FinanceManager", attribute.Roles ?? string.Empty);
        Assert.Contains("Admin", attribute.Roles ?? string.Empty);
    }

    [Fact]
    public void DeleteInvoice_Has_Admin_Only_Authorization()
    {
        var method = typeof(InvoicesController).GetMethod("DeleteInvoice");
        var attribute = Assert.Single(method!.GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>());

        Assert.Equal("Admin", attribute.Roles);
    }
}
