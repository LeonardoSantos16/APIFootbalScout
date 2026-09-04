namespace APIFootballScout.Domain.Base.Exceptions
{
    public sealed class NaoAutenticadoException(string codigo, string mensagem)
        : DomainException(codigo, mensagem);
}
