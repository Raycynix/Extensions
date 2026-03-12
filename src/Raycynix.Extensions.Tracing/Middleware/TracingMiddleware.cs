using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Raycynix.Extensions.Tracing.Middleware;

public class TracingMiddleware(RequestDelegate next)
{
    private const string CorrelationHeader = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Пытаемся получить CorrelationId из заголовков или создаем новый
        if (!context.Request.Headers.TryGetValue(CorrelationHeader, out var correlationId))
        {
            correlationId = Activity.Current?.RootId ?? Guid.NewGuid().ToString();
        }

        // 2. Прокидываем его в ответ, чтобы клиент видел ID операции
        context.Response.Headers[CorrelationHeader] = correlationId;

        // 3. Обогащаем логи Serilog и привязываем к контексту
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}