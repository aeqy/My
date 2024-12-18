using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace MyCc.WebAPI.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        // 设置默认状态码为 InternalServerError
        var statusCode = HttpStatusCode.InternalServerError;

        // 记录详细的错误信息
        _logger.LogError(exception, $"发生未处理的异常：{context.Request.Path} {context.Request.Method}");

        switch (exception)
        {
            case ApplicationException:
                statusCode = HttpStatusCode.BadRequest;
                break;
            case KeyNotFoundException e:
                statusCode = HttpStatusCode.NotFound;
                break;
            default:
                break;
        }

        response.StatusCode = (int)statusCode;

        // 使用 ProblemDetails 来标准化错误响应
        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = "An error occurred while processing your request.",
            Detail = exception.Message
        };

        var result = JsonSerializer.Serialize(problemDetails);
        await response.WriteAsync(result);
    }
}