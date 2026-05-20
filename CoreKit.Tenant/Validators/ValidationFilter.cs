using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CoreKit.IAM.Validators;

public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilter(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        foreach (var (_, value) in context.ActionArguments)
        {
            if (value is null) continue;

            var validatorType = typeof(IValidator<>)
                .MakeGenericType(value.GetType());

            if (_serviceProvider.GetService(validatorType)
                is not IValidator validator)
                continue;

            var validationContext = new ValidationContext<object>(value);
            var result = await validator.ValidateAsync(
                validationContext, context.HttpContext.RequestAborted);

            if (result.IsValid) continue;

            var errors = result.Errors
                .Select(e => new
                {
                    field = e.PropertyName,
                    message = e.ErrorMessage
                })
                .ToList();

            context.Result = new ObjectResult(new
            {
                success = false,
                errorCode = "VALIDATION_ERROR",
                message = "One or more validation errors occurred.",
                errors
            })
            {
                StatusCode = 422
            };

            return;
        }

        await next();
    }
}