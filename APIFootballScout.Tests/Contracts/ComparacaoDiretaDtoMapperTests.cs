using APIFootballScout.Application.Analise;
using APIFootballScout.Contracts.Acompanhamento;
using APIFootballScout.Contracts.Analise;
using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Analise.ValueObject;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Tests.Contracts
{
    public class ComparacaoDiretaDtoMapperTests
    {
        private static readonly Recorte Brasileirao2024 = new(325, 63814, ContextoDeRecorte.Clube);

        private const int Neymar = 13812;
        private const int Rodrygo = 8256;

        [Fact]
        public void Os_jogadores_saem_fatorados_no_topo_chaveados_pela_identidade()
        {
            // Arrange
            var result = Realizada();

            // Act
            var dto = result.ParaResponse();

            // Assert
            Assert.Equal([Rodrygo, Neymar], dto.Jogadores.Keys.OrderBy(id => id));
            Assert.Equal("Neymar", dto.Jogadores[Neymar].Nome);
            Assert.Equal(PosicaoDto.Ataque, dto.Jogadores[Neymar].Posicao);
        }

        [Fact]
        public void Cada_atributo_expoe_os_valores_chaveados_pela_identidade()
        {
            // Arrange
            var result = Realizada();

            // Act
            var dto = result.ParaResponse();

            // Assert
            var gols = Assert.Single(dto.Atributos!, atributo => atributo.Tipo == TipoDeAtributoDto.Gols);
            Assert.Equal([Rodrygo, Neymar], gols.Valores.Keys.OrderBy(id => id));
            Assert.Equal(0.45m, gols.Valores[Neymar].Valor);
            Assert.Equal(2400, gols.Valores[Neymar].AmostraEmMinutos);
            Assert.Equal(ResultadoDoCalculoDto.Calculada, gols.Valores[Neymar].Resultado);
            Assert.Null(gols.Valores[Neymar].Motivo);
        }

        [Fact]
        public void O_valor_recusado_expoe_o_motivo_e_nenhum_valor()
        {
            // Arrange
            var result = Realizada(
                doOutro: new AtributoRecusado(MotivoDaRecusa.FonteNaoAtribuiu));

            // Act
            var dto = result.ParaResponse();

            // Assert
            var gols = Assert.Single(dto.Atributos!, atributo => atributo.Tipo == TipoDeAtributoDto.Gols);
            Assert.Equal(ResultadoDoCalculoDto.Recusada, gols.Valores[Rodrygo].Resultado);
            Assert.Equal(MotivoDaRecusaDto.FonteNaoAtribuiu, gols.Valores[Rodrygo].Motivo);
            Assert.Null(gols.Valores[Rodrygo].Valor);
            Assert.Null(gols.Valores[Rodrygo].AmostraEmMinutos);
        }

        [Fact]
        public void A_comparacao_realizada_declara_o_recorte_e_nenhum_motivo()
        {
            // Arrange
            var result = Realizada();

            // Act
            var dto = result.ParaResponse();

            // Assert
            Assert.Equal(ResultadoDaComparacaoDto.Realizada, dto.Resultado);
            Assert.Equal(new RecorteDto(325, 63814, ContextoDeRecorteDto.Clube), dto.Recorte);
            Assert.Null(dto.Motivo);
        }

        [Fact]
        public void A_comparacao_recusada_expoe_o_motivo_e_nenhum_atributo()
        {
            // Arrange
            var result = new ComparacaoDiretaResult(
                Jogadores,
                new ComparacaoRecusada(MotivoDaRecusaDaComparacao.PosicoesIncompativeis));

            // Act
            var dto = result.ParaResponse();

            // Assert
            Assert.Equal(ResultadoDaComparacaoDto.Recusada, dto.Resultado);
            Assert.Equal(MotivoDaRecusaDaComparacaoDto.PosicoesIncompativeis, dto.Motivo);
            Assert.Null(dto.Atributos);
            Assert.Null(dto.Recorte);
        }

        [Fact]
        public void A_recusa_por_posicao_desconhecida_expoe_o_jogador_sem_posicao()
        {
            // Arrange
            var result = new ComparacaoDiretaResult(
                [
                    new JogadorNaComparacao(Rodrygo, "Rodrygo", null),
                    new JogadorNaComparacao(Neymar, "Neymar", Posicao.Ataque)
                ],
                new ComparacaoRecusada(MotivoDaRecusaDaComparacao.PosicaoDesconhecida));

            // Act
            var dto = result.ParaResponse();

            // Assert
            Assert.Null(dto.Jogadores[Rodrygo].Posicao);
            Assert.Equal(PosicaoDto.Ataque, dto.Jogadores[Neymar].Posicao);
        }

        private static IReadOnlyCollection<JogadorNaComparacao> Jogadores =>
        [
            new JogadorNaComparacao(Rodrygo, "Rodrygo", Posicao.Ataque),
            new JogadorNaComparacao(Neymar, "Neymar", Posicao.Ataque)
        ];

        private static ComparacaoDiretaResult Realizada(ResultadoDeAtributo? doOutro = null)
            => new(
                Jogadores,
                new ComparacaoRealizada(
                    Brasileirao2024,
                    [
                        new AtributoComparado(
                            TipoDeAtributo.Gols,
                            new Dictionary<int, ResultadoDeAtributo>
                            {
                                [Neymar] = new AtributoCalculado(0.45m, new Minutagem(2400, Brasileirao2024)),
                                [Rodrygo] = doOutro
                                    ?? new AtributoCalculado(0.15m, new Minutagem(2400, Brasileirao2024))
                            })
                    ]));
    }
}
