namespace APIFootballScout.Domain.Base.Exceptions
{
    public sealed class RegraDeNegocioException(string codigo, string mensagem)
        : DomainException(codigo, mensagem);
}
