using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Analise.ValueObject;
using APIFootballScout.Domain.CatalogoDeJogador;
using APIFootballScout.Domain.SharedKernel;
using APIFootballScout.Infrastructure.SofascoreExternalAdapter.player;

namespace APIFootballScout.Infrastructure.SofascoreExternalAdapter.Acl
{
    internal static class SofascoreTradutor
    {
        private const string MoedaEsperada = "EUR";

        /// <summary>
        /// R9.6 — a fonte não identifica o recorte no retorno. O recorte pedido
        /// é carimbado aqui, de volta no dado traduzido.
        /// </summary>
        public static PerfilDoJogador TraduzirParaPerfilDoJogador(
            SofaPlayerDetailsResponse sofaPlayer,
            SofaSeasonStatsResponse statsPlayer,
            Recorte recorte,
            DateTime lidoEm)
        {
            return new PerfilDoJogador(
                JogadorId: sofaPlayer.Player.Id,
                Nome: sofaPlayer.Player.Name,
                Posicao: TraduzirPosicao(sofaPlayer.Player.Position),
                Clube: sofaPlayer.Player.Team?.Name,
                ValorDeMercado: new Dinheiro(sofaPlayer.Player.ProposedMarketValue, MoedaEsperada),
                MinutosJogados: statsPlayer.Statistics.MinutesPlayed,
                Recorte: recorte,
                LidoEm: lidoEm
            );
        }

        private static Posicao? TraduzirPosicao(string? posicao) => posicao switch
        {
            "G" => Posicao.Goleiro,
            "D" => Posicao.Defesa,
            "M" => Posicao.MeioCampo,
            "F" => Posicao.Ataque,
            _ => null
        };

        /// <summary>
        /// R9.4 — a fonte entrega contagem e valor derivado na mesma estrutura.
        /// A separação em tipos distintos é decidida aqui, na tradução.
        /// </summary>
        public static ConjuntoDeEstatisticas TraduzirParaConjuntoDeEstatisticas(
            SofaSeasonStatsResponse statsPlayer,
            Recorte recorte)
        {
            var estatisticas = statsPlayer.Statistics;
            var minutagem = new Minutagem(estatisticas.MinutesPlayed, recorte);

            return new ConjuntoDeEstatisticas(
                recorte: recorte,
                acumulaveis:
                [
                    new EstatisticaAcumulavel(TipoDeAtributo.Gols, estatisticas.Goals, minutagem),
                    new EstatisticaAcumulavel(TipoDeAtributo.Assistencias, estatisticas.Assists, minutagem),
                    new EstatisticaAcumulavel(TipoDeAtributo.PassesDecisivos, estatisticas.KeyPasses, minutagem),
                    new EstatisticaAcumulavel(TipoDeAtributo.Desarmes, estatisticas.Tackles, minutagem),
                    new EstatisticaAcumulavel(TipoDeAtributo.Interceptacoes, estatisticas.Interceptions, minutagem)
                ],
                derivados:
                [
                    new ValorDerivado(TipoDeAtributo.Rating, (decimal?)estatisticas.Rating, minutagem),
                    new ValorDerivado(TipoDeAtributo.PrecisaoDePasse, (decimal?)estatisticas.AccuratePassesPercentage, minutagem)
                ]);
        }
    }
}
