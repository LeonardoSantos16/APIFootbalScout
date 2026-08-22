namespace APIFootballScout.Domain.Base.Exceptions
{
    public sealed class RecursoNaoEncontradoException(string codigo, string mensagem)
        : DomainException(codigo, mensagem);
}
