using APIFootballScout.Domain.Base.Exceptions;

namespace APIFootballScout.Infrastructure.External
{
    public sealed class FonteExternaIndisponivelException(string codigo, string mensagem, Exception? innerException = null)
        : Exception(mensagem, innerException), ICodigoDeErro
    {
        public string Codigo { get; } = codigo;
    }
}
