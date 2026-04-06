using InvoiceManagementSystem.API.Validators;
using InvoiceManagementSystem.BLL.Models;
using Xunit;

namespace InvoiceManagementSystem.Tests;

public class CreateInvoiceValidatorTests
{
    private readonly CreateInvoiceValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_CustomerId_Is_Zero()
    {
        var model = new CreateInvoiceDto
        {
            CustomerId = 0,
            InvoiceDate = new DateTime(2026, 4, 6),
            DueDate = new DateTime(2026, 4, 20)
        };

        var result = _validator.Validate(model);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateInvoiceDto.CustomerId));
    }

    [Fact]
    public void Should_Have_Error_When_DueDate_Is_Not_After_InvoiceDate()
    {
        var model = new CreateInvoiceDto
        {
            CustomerId = 1,
            InvoiceDate = new DateTime(2026, 4, 6),
            DueDate = new DateTime(2026, 4, 6)
        };

        var result = _validator.Validate(model);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateInvoiceDto.DueDate));
    }

    [Fact]
    public void Should_Have_Error_When_Discount_Is_Negative()
    {
        var model = new CreateInvoiceDto
        {
            CustomerId = 1,
            InvoiceDate = new DateTime(2026, 4, 6),
            DueDate = new DateTime(2026, 4, 20),
            Discount = -1
        };

        var result = _validator.Validate(model);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateInvoiceDto.Discount));
    }

    [Fact]
    public void Should_Not_Have_Error_For_Valid_Invoice()
    {
        var model = new CreateInvoiceDto
        {
            CustomerId = 1002,
            InvoiceDate = new DateTime(2026, 4, 6),
            DueDate = new DateTime(2026, 4, 20),
            Discount = 0,
            Tax = 18
        };

        var result = _validator.Validate(model);

        Assert.True(result.IsValid);
    }
}
