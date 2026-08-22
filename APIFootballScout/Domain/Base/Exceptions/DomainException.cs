namespace APIFootballScout.Domain.Base.Exceptions
{
    public abstract class DomainException(string codigo, string mensagem)
        : Exception(mensagem), ICodigoDeErro
    {
        public string Codigo { get; } = codigo;
    }
}
