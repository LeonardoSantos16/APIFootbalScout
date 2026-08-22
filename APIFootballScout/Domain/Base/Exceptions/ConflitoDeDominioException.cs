namespace APIFootballScout.Domain.Base.Exceptions
{
    public sealed class ConflitoDeDominioException(string codigo, string mensagem)
        : DomainException(codigo, mensagem);
}
