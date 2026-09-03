using APIFootballScout.Domain.Base.Exceptions;

namespace APIFootballScout.Domain.ShortlistPersonalizada.ValueObject
{
    public sealed record Prioridade
    {
        public int Valor { get; }

        public Prioridade(int valor)
        {
            if (valor <= 0)
                throw new ValorInvalidoException("prioridade.nao_positiva", "prioridade deve ser um valor positivo");
            Valor = valor;
        }
    };
}
