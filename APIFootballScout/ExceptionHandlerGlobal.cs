using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Infrastructure.External;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace APIFootballScout
{
    public class ExceptionHandlerGlobal(
        ILogger<ExceptionHandlerGlobal> logger,
        IProblemDetailsService problemDetailsService) : IExceptionHandler
    {
        private const string DetalheGenerico = "An unexpected error occurred while processing the request.";

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
            {
                LogRequisicaoCancelada(logger, httpContext.Request.Path, null);
                return true;
            }

            if (httpContext.Response.HasStarted)
            {
                LogRespostaJaIniciada(logger, httpContext.Request.Path, exception);
                return false;
            }

            var (status, titulo) = MapearStatus(exception);

            LogException(status, exception);

            httpContext.Response.StatusCode = status;

            var erroConhecido = exception as ICodigoDeErro;

            var problemDetails = new ProblemDetails
            {
                Status = status,
                Title = titulo,
                Detail = erroConhecido is null ? DetalheGenerico : exception.Message,
                Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}"
            };

            if (erroConhecido is not null)
                problemDetails.Extensions["code"] = erroConhecido.Codigo;

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails
            });
        }

        private static (int status, string titulo) MapearStatus(Exception exception) => exception switch
        {
            NaoAutenticadoException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            RecursoNaoEncontradoException => (StatusCodes.Status404NotFound, "Resource not found"),
            ConflitoDeDominioException => (StatusCodes.Status409Conflict, "Operation conflict"),
            ConflitoDeConcorrenciaException => (StatusCodes.Status412PreconditionFailed, "Precondition failed"),
            RegraDeNegocioException => (StatusCodes.Status422UnprocessableEntity, "Business rule violation"),
            ValorInvalidoException => (StatusCodes.Status400BadRequest, "Invalid request"),
            FonteExternaIndisponivelException => (StatusCodes.Status502BadGateway, "External data source unavailable"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        private void LogException(int status, Exception exception)
        {
            if (status >= StatusCodes.Status500InternalServerError)
                LogErroInesperado(logger, exception.Message, exception);
            else
                LogExcecaoTratada(logger, status, exception.Message, null);
        }

        private static readonly Action<ILogger, PathString, Exception?> LogRequisicaoCancelada =
            LoggerMessage.Define<PathString>(LogLevel.Information, new EventId(1, nameof(LogRequisicaoCancelada)),
                "Request cancelled by the client: {Path}");

        private static readonly Action<ILogger, PathString, Exception?> LogRespostaJaIniciada =
            LoggerMessage.Define<PathString>(LogLevel.Error, new EventId(2, nameof(LogRespostaJaIniciada)),
                "Exception thrown after the response had started: {Path}");

        private static readonly Action<ILogger, string, Exception?> LogErroInesperado =
            LoggerMessage.Define<string>(LogLevel.Error, new EventId(3, nameof(LogErroInesperado)),
                "Unexpected error: {Message}");

        private static readonly Action<ILogger, int, string, Exception?> LogExcecaoTratada =
            LoggerMessage.Define<int, string>(LogLevel.Warning, new EventId(4, nameof(LogExcecaoTratada)),
                "Handled exception [{Status}]: {Message}");
    }
}