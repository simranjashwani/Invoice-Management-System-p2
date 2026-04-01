using FluentValidation;
using InvoiceManagementSystem.BLL.Models;

namespace InvoiceManagementSystem.API.Validators
{
    public class CreateInvoiceValidator : AbstractValidator<CreateInvoiceDto>
    {
        public CreateInvoiceValidator()
        {
            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("CustomerId must be greater than 0");

            RuleFor(x => x.InvoiceDate)
                .NotEmpty().WithMessage("Invoice date is required");

            RuleFor(x => x.DueDate)
                .GreaterThan(x => x.InvoiceDate)
                .WithMessage("Due date must be greater than invoice date");

            RuleFor(x => x.Discount)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.Tax)
                .GreaterThanOrEqualTo(0);
        }
    }
}