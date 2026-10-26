namespace APIFootballScout.Domain.Base.Exceptions
{
    public sealed class ConflitoDeConcorrenciaException(string codigo, string mensagem)
        : DomainException(codigo, mensagem);
}