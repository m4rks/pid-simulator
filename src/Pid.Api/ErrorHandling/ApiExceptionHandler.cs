using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Pid.Application.Common;
using Pid.Domain.Common;

namespace Pid.Api.ErrorHandling;

/// <summary>
/// Translates exceptions from the inner layers into HTTP responses (RFC 7807 "problem details"),
/// so endpoints do not need try/catch. Anything not listed here stays a 500.
/// </summary>
internal sealed class ApiExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails? problem = exception switch
        {
            ValidationException validation => new HttpValidationProblemDetails(
                validation.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray()))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed.",
            },
            NotFoundException => new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = exception.Message },
            DomainException => new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = exception.Message },
            // Malformed JSON or a wrong value type in the request body.
            BadHttpRequestException badRequest => new ProblemDetails { Status = badRequest.StatusCode, Title = "Bad request." },
            _ => null,
        };

        if (problem is null)
            return false;

        httpContext.Response.StatusCode = problem.Status!.Value;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });
    }
}
