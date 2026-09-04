using APIFootballScout.Domain.Base.Exceptions;

namespace APIFootballScout.Domain.ShortlistPersonalizada.ValueObject
{
    public sealed record LimiteDeAlvos
    {
        public int Valor { get; }

        public LimiteDeAlvos(int valor)
        {
            if (valor <= 0)
                throw new ValorInvalidoException(
                    "shortlist.limite_nao_positivo",
                    "O limite de alvos deve ser um valor positivo.");

            Valor = valor;
        }

        public bool Comporta(int quantidadeDeAlvos) => quantidadeDeAlvos < Valor;
    }
}
