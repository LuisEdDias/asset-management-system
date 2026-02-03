using AssetManagement.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AssetManagement.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(
        HttpContext context,
        IStringLocalizer<SharedMessages> messageLocalizer)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex, messageLocalizer, context.RequestAborted);
        }
    }

    private static async Task HandleAsync(
        HttpContext context,
        Exception ex,
        IStringLocalizer<SharedMessages> messageLocalizer,
        CancellationToken ct)
    {
        if (ex is ValidationException validationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var localizedErrors = validationException.Errors.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Select(k => messageLocalizer[k].Value).ToArray()
            );

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = messageLocalizer["Common.ValidationError"].Value,
                Detail = messageLocalizer["Common.ValidationErrorDetail"].Value,
                Instance = context.Request.Path
            };

            problemDetails.Extensions["errors"] = localizedErrors;

            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problemDetails, ct);
            return;
        }

        if (ex is AppException appEx)
        {
            var status = appEx switch
            {
                EntityNotFoundException => StatusCodes.Status404NotFound,
                AssetTypeDuplicateNameException => StatusCodes.Status409Conflict,
                AllocationConflictException => StatusCodes.Status409Conflict,
                AssetNotAvailableException => StatusCodes.Status409Conflict,
                AssetReturnInvalidException => StatusCodes.Status409Conflict,
                AssetTypeInUseException => StatusCodes.Status409Conflict,
                UserDuplicateEmailException => StatusCodes.Status409Conflict,

                _ => StatusCodes.Status400BadRequest
            };

            context.Response.StatusCode = status;

            var titleKey = status switch
            {
                StatusCodes.Status404NotFound => "Common.NotFound",
                StatusCodes.Status409Conflict => "Common.Conflict",
                StatusCodes.Status400BadRequest => "Common.ValidationError",
                _ => "Common.UnexpectedError"
            };

            var problem = new ProblemDetails
            {
                Status = status,
                Title = messageLocalizer[titleKey].Value,
                Detail = messageLocalizer[appEx.MessageKey].Value,
                Instance = context.Request.Path
            };

            await context.Response.WriteAsJsonAsync(problem, ct);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var unexpected = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = messageLocalizer["Common.UnexpectedError"].Value,
            Detail = messageLocalizer["Common.UnexpectedError"].Value,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(unexpected, ct);
    }
}