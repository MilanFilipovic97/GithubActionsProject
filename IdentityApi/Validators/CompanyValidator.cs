using FluentValidation;
using IdentityApi.DTOs;

namespace IdentityApi.Validators
{
    public class CompanyValidator : AbstractValidator<CompanyPostModel>
    {
        public CompanyValidator()
        {
            RuleFor(company => company.Name)
                .NotEmpty()
                .WithMessage("Company name is required");

            RuleFor(company => company.Pib)
                .NotEmpty()
                .Length(9)
                .WithMessage("Pib must be 9 characters long.");

            RuleFor(company => company.MaticniBroj)
                .NotEmpty()
                .Length(8)
                .WithMessage("Maticni broj must be 8 characters long.");

        }
    }
}
