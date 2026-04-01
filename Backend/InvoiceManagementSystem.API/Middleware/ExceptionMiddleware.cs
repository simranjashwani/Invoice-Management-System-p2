using System.Net;
using System.Text.Json;
using InvoiceManagementSystem.BLL.Exceptions;

namespace InvoiceManagementSystem.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
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

    

private static Task HandleExceptionAsync(HttpContext context, Exception ex)
{
    context.Response.ContentType = "application/json";

    int statusCode = 500;

    // 🔥 Handle BusinessException separately
    if (ex is BusinessException)
    {
        statusCode = 400;
    }

    context.Response.StatusCode = statusCode;

    var response = new
    {
        message = ex.Message,
        statusCode = statusCode
    };

    return context.Response.WriteAsJsonAsync(response);
}
    }
}