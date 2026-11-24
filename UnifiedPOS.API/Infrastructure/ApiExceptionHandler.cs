using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnifiedPOS.Application.Common;

namespace UnifiedPOS.API.Infrastructure;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger, IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, title, detail) = exception switch
        {
            PosException pos => (pos.StatusCode, "Request failed", pos.Message),
            DbUpdateConcurrencyException => (409, "Concurrency conflict", "The record changed while the request was being processed. Retry the operation."),
            DbUpdateException => (409, "Data conflict", "The operation conflicts with existing data."),
            OperationCanceledException when context.RequestAborted.IsCancellationRequested => (499, "Request cancelled", "The request was cancelled."),
            _ => (500, "Unexpected error", "An unexpected error occurred.")
        };
        if (status >= 500) logger.LogError(exception, "Request failed. TraceId: {TraceId}", context.TraceIdentifier);
        else logger.LogWarning("Request rejected with status {Status}. TraceId: {TraceId}; Reason: {Reason}", status, context.TraceIdentifier, detail);
        context.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context, Exception = exception,
            ProblemDetails = new ProblemDetails { Status = status, Title = title, Detail = detail, Extensions = { ["traceId"] = context.TraceIdentifier } }
        });
    }
}
