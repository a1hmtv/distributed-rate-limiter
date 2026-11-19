using FluentValidation;
using RateLimiter.Writer.Application.Services.Models;

namespace RateLimiter.Writer.Application.Services.Validators;

public sealed class CreateLimitValidator : AbstractValidator<CreateLimit>
{
    public CreateLimitValidator()
    {
        RuleFor(x => x.Route).NotEmpty().WithMessage("Route must not be empty.");

        RuleFor(x => x.RequestsPerMinute).GreaterThan(0).WithMessage("Requests per minute must be greater than 0.");
    }
}