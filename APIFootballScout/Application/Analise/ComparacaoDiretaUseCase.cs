using APIFootballScout.Domain.Analise.Services;
using APIFootballScout.Domain.Analise.ValueObject;
using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Application.Analise
{
    public class ComparacaoDiretaUseCase
    {
        private readonly ICatalogoDeJogador _catalogoDeJogador;
        private readonly ComparadorDeJogadores _comparador;

        public ComparacaoDiretaUseCase(
            ICatalogoDeJogador catalogoDeJogador,
            ComparadorDeJogadores comparador)
        {
            _catalogoDeJogador = catalogoDeJogador;
            _comparador = comparador;
        }

        public async Task<ComparacaoDiretaResult> Comparar(
            ComparacaoDiretaRequest request,
            CancellationToken cancellationToken)
        {
            if (request.JogadorA == request.JogadorB)
            {
                throw new ValorInvalidoException(
                    "comparacao.jogador_consigo_mesmo",
                    "A comparacao exige dois jogadores distintos.");
            }

            var recorte = new Recorte(request.CompeticaoId, request.TemporadaId, request.Contexto);

            var um = await Resolver(request.JogadorA, recorte, cancellationToken);
            var outro = await Resolver(request.JogadorB, recorte, cancellationToken);

            var jogadores = new[] { um, outro }
                .Select(jogador => new JogadorNaComparacao(
                    jogador.Perfil.JogadorId,
                    jogador.Perfil.Nome,
                    jogador.Perfil.Posicao))
                .OrderBy(jogador => jogador.JogadorId)
                .ToList();

            return new ComparacaoDiretaResult(jogadores, _comparador.Comparar(um, outro));
        }

        private async Task<JogadorParaComparar> Resolver(
            int jogadorId,
            Recorte recorte,
            CancellationToken cancellationToken)
        {
            var perfil = await _catalogoDeJogador.ObterPerfilDoJogador(jogadorId, recorte, cancellationToken)
                ?? throw new RecursoNaoEncontradoException(
                    "jogador.perfil_nao_encontrado",
                    "perfil do jogador nao encontrado para esse recorte");

            var estatisticas = await _catalogoDeJogador.ObterEstatisticas(jogadorId, recorte, cancellationToken)
                ?? throw new RecursoNaoEncontradoException(
                    "jogador.estatisticas_nao_encontradas",
                    "estatisticas do jogador nao encontradas para esse recorte");

            return new JogadorParaComparar(perfil, estatisticas);
        }
    }
}
