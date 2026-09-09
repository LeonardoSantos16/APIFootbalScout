using APIFootballScout.Domain.Analise.Specifications;
using APIFootballScout.Domain.Analise.ValueObject;
using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.CatalogoDeJogador;

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
        {
            if (um.Perfil.JogadorId == outro.Perfil.JogadorId)
            {
                throw new ValorInvalidoException(
                    "comparacao.jogador_consigo_mesmo",
                    "A comparacao exige dois jogadores distintos.");
            }

            if (um.Perfil.Posicao is not Posicao posicaoDeUm
                || outro.Perfil.Posicao is not Posicao posicaoDoOutro)
            {
                return new ComparacaoRecusada(MotivoDaRecusaDaComparacao.PosicaoDesconhecida);
            }

            if (!_posicoesCompativeis.IsSatisfiedBy((posicaoDeUm, posicaoDoOutro)))
            {
                return new ComparacaoRecusada(MotivoDaRecusaDaComparacao.PosicoesIncompativeis);
            }

            if (um.Estatisticas.Recorte != outro.Estatisticas.Recorte)
            {
                return new ComparacaoRecusada(MotivoDaRecusaDaComparacao.RecorteDivergente);
            }

            if (!_amostraSuficiente.IsSatisfiedBy(um.Estatisticas.Minutagem)
                || !_amostraSuficiente.IsSatisfiedBy(outro.Estatisticas.Minutagem))
            {
                return new ComparacaoRecusada(MotivoDaRecusaDaComparacao.AmostraInsuficiente);
            }

            var deUm = ResultadosPorTipo(um);
            var doOutro = ResultadosPorTipo(outro);

            var atributos = deUm.Keys
                .Where(doOutro.ContainsKey)
                .OrderBy(tipo => tipo)
                .Select(tipo => new AtributoComparado(
                    tipo,
                    new Dictionary<int, ResultadoDeAtributo>
                    {
                        [um.Perfil.JogadorId] = deUm[tipo],
                        [outro.Perfil.JogadorId] = doOutro[tipo]
                    }))
                .ToList();

            return new ComparacaoRealizada(um.Estatisticas.Recorte, atributos);
        }

        private Dictionary<TipoDeAtributo, ResultadoDeAtributo> ResultadosPorTipo(JogadorParaComparar jogador)
            => jogador.Estatisticas.Acumulaveis
                .Select(estatistica => (estatistica.Tipo, Resultado: estatistica.PorNoventaMinutos(_amostraSuficiente)))
                .Concat(jogador.Estatisticas.Derivados
                    .Select(derivado => (derivado.Tipo, Resultado: derivado.Resultado())))
                .ToDictionary(atributo => atributo.Tipo, atributo => atributo.Resultado);
    }
}
