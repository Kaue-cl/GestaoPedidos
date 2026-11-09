using System.Text.Json;
using GestaoPedidos.API.Exceptions;

namespace GestaoPedidos.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
            await TratarExcecaoAsync(context, ex);
        }
    }

    private async Task TratarExcecaoAsync(
        HttpContext context,
        Exception exception)
    {
        var statusCode = exception switch
        {
            ClienteNaoEncontradoException => StatusCodes.Status404NotFound,
            ProdutoNaoEncontradoException => StatusCodes.Status404NotFound,
            EmailJaCadastradoException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Erro interno ao processar a requisição.");
        }
        else
        {
            _logger.LogWarning(
                "Erro de negócio: {Mensagem}",
                exception.Message);
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var mensagem = statusCode == StatusCodes.Status500InternalServerError
            ? "Ocorreu um erro interno no servidor."
            : exception.Message;

        var resposta = new
        {
            mensagem
        };

        var json = JsonSerializer.Serialize(resposta);

        await context.Response.WriteAsync(json);
    }
}