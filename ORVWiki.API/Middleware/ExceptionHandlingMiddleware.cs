using Microsoft.AspNetCore.Mvc;
using ORVWiki.Application.Common.Exceptions;

namespace ORVWiki.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await WriteProblem(context, StatusCodes.Status400BadRequest, "Validation failed.", ex.Errors);
        }
        catch (AuthException ex)
        {
            await WriteProblem(context, StatusCodes.Status401Unauthorized, ex.Message);
        }
        catch (ForbiddenException ex)
        {
            await WriteProblem(context, StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (NotFoundException ex)
        {
            await WriteProblem(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteProblem(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteProblem(context, StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }

    private static async Task WriteProblem(
        HttpContext context,
        int status,
        string detail,
        IDictionary<string, string[]>? errors = null)
    {
        if (context.Response.HasStarted) return;

        context.Response.Clear();
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        if (errors is not null)
        {
            var vp = new ValidationProblemDetails(errors)
            {
                Status = status,
                Title = detail
            };
            await context.Response.WriteAsJsonAsync(vp);
            return;
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = ReasonPhrases.Get(status),
            Detail = detail
        };
        await context.Response.WriteAsJsonAsync(problem);
    }

    private static class ReasonPhrases
    {
        public static string Get(int status) => status switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            409 => "Conflict",
            _ => "Error"
        };
    }
}
