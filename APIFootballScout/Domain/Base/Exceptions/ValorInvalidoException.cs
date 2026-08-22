namespace APIFootballScout.Domain.Base.Exceptions
{
    public sealed class ValorInvalidoException(string codigo, string mensagem)
        : DomainException(codigo, mensagem);
}
