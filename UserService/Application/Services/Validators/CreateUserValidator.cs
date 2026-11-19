using FluentValidation;
using UserService.Application.Services.Models;

namespace UserService.Application.Services.Validators;

public class CreateUserValidator : AbstractValidator<CreateUser>
{
    public CreateUserValidator()
    {
        RuleFor(user => user.Login).NotEmpty();
        RuleFor(user => user.Password).NotEmpty();
        RuleFor(user => user.Name).NotEmpty();
        RuleFor(user => user.Surname).NotEmpty();

        RuleFor(user => user.Age).GreaterThan(0);
    }
}