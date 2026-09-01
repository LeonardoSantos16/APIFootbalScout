using APIFootballScout.Domain.Base.Exceptions;

namespace APIFootballScout.Domain.RelatorioScouting.ValueObject
{
    public sealed record Nota
    {
        public const decimal Minima = 0m;
        public const decimal Maxima = 10m;
        private const int CasasDecimais = 1;

        public decimal Valor { get; }

        public Nota(decimal valor)
        {
            if (valor < Minima || valor > Maxima)
                throw new ValorInvalidoException(
                    "nota.fora_da_faixa",
                    $"a nota deve estar entre {Minima} e {Maxima}");

            if (decimal.Round(valor, CasasDecimais) != valor)
                throw new ValorInvalidoException(
                    "nota.precisao_invalida",
                    "a nota não pode ter mais de uma casa decimal");

            Valor = valor;
        }
    }
}
