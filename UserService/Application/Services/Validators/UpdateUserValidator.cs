using FluentValidation;
using UserService.Application.Services.Models;

namespace UserService.Application.Services.Validators;

public class UpdateUserValidator : AbstractValidator<UpdateUser>
{
    public UpdateUserValidator()
    {
        RuleFor(user => user.Password).NotEmpty().When(user => user.Password is not null);

        RuleFor(user => user.Name).NotEmpty().When(user => user.Name is not null);

        RuleFor(user => user.Surname).NotEmpty().When(user => user.Surname is not null);

        RuleFor(user => user.Age).GreaterThan(0).When(user => user.Age.HasValue);

        RuleFor(user => user).Must(user => user.Password is not null || user.Name is not null || user.Surname is not null || user.Age.HasValue)
            .WithMessage("At least one field must be specified for updating");
    }
}