using System.Net;
using System.Text.Json;
using ApiAuth.Responses;

namespace ApiAuth.Middlewares;

/// <summary>
/// Middleware global para capturar exceções não tratadas e devolver um
/// `ApiResponse` consistente ao cliente. Mantém a API previsível e evita
/// vazar detalhes de implementação em produção.
/// </summary>
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
            context.Response.StatusCode =
                (int)HttpStatusCode.InternalServerError;

            context.Response.ContentType =
                "application/json";

            var response =
                new ApiResponse<object>
                {
                    Success = false,
                    Message = "Ocorreu um erro interno.",
                    Errors = new()
                    {
                        ex.Message
                    }
                };

            var json =
                JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }
    }
}