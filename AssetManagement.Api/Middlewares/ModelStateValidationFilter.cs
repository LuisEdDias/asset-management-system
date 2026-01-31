using AssetManagement.Application.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AssetManagement.Api.Middlewares;

public sealed class ModelStateValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        {
            if (context.ModelState.IsValid)
                return;

            var erros = context.ModelState
                .Where(kv => kv.Value?.Errors.Count > 0)
                .ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value!.Errors
                        .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                            ? "Common.ValidationError"
                            : e.ErrorMessage)
                        .Distinct()
                        .ToArray()
                );

            throw new ValidationException(erros);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}