using APIFootballScout.Domain.Analise.Specifications;
using APIFootballScout.Domain.Analise.ValueObject;

namespace APIFootballScout.Domain.Analise.Services
{
    public sealed class ComparadorDeJogadores
    {
        private readonly PosicoesCompativeisSpecification _posicoesCompativeis;
        private readonly AmostraSuficienteSpecification _amostraSuficiente;

        public ComparadorDeJogadores(
            PosicoesCompativeisSpecification posicoesCompativeis,
            AmostraSuficienteSpecification amostraSuficiente)
        {
            _posicoesCompativeis = posicoesCompativeis;
            _amostraSuficiente = amostraSuficiente;
        }

        public ResultadoDaComparacao Comparar(JogadorParaComparar um, JogadorParaComparar outro)
            => throw new NotImplementedException();
    }
}
