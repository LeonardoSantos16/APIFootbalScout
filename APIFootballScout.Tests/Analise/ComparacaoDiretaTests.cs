using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Analise.Services;
using APIFootballScout.Domain.Analise.Specifications;
using APIFootballScout.Domain.Analise.ValueObject;
using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.CatalogoDeJogador;
using APIFootballScout.Domain.SharedKernel;

namespace APIFootballScout.Tests.Analise
{
    public class ComparacaoDiretaTests
    {
        private static readonly Recorte Brasileirao2024 = new(325, 63814, ContextoDeRecorte.Clube);
        private static readonly Recorte Libertadores2024 = new(384, 57296, ContextoDeRecorte.Clube);

        private const int Neymar = 13812;
        private const int Rodrygo = 8256;

        [Fact]
        public void Jogador_nao_e_comparado_consigo_mesmo()
        {
            // Act
            var erro = Assert.Throws<ValorInvalidoException>(
                () => Comparador().Comparar(Jogador(Neymar), Jogador(Neymar)));

            // Assert
            Assert.Equal("comparacao.jogador_consigo_mesmo", erro.Codigo);
        }

        [Fact]
        public void Posicoes_incompativeis_recusam_a_comparacao()
        {
            // Act
            var resultado = Comparador().Comparar(
                Jogador(Neymar, Posicao.Ataque),
                Jogador(Rodrygo, Posicao.Goleiro));

            // Assert
            Assert.Equal(
                MotivoDaRecusaDaComparacao.PosicoesIncompativeis,
                Assert.IsType<ComparacaoRecusada>(resultado).Motivo);
        }

        [Fact]
        public void Posicoes_incompativeis_recusam_em_qualquer_ordem_da_chamada()
        {
            // Arrange
            var um = Jogador(Neymar, Posicao.Ataque);
            var outro = Jogador(Rodrygo, Posicao.Goleiro);

            // Act
            var umPrimeiro = Comparador().Comparar(um, outro);
            var outroPrimeiro = Comparador().Comparar(outro, um);

            // Assert
            Assert.Equal(umPrimeiro, outroPrimeiro);
            Assert.Equal(
                MotivoDaRecusaDaComparacao.PosicoesIncompativeis,
                Assert.IsType<ComparacaoRecusada>(umPrimeiro).Motivo);
        }

        [Fact]
        public void Posicoes_compativeis_sem_serem_iguais_sustentam_a_comparacao()
        {
            // Act
            var resultado = Comparador().Comparar(
                Jogador(Neymar, Posicao.Ataque),
                Jogador(Rodrygo, Posicao.MeioCampo));

            // Assert
            Assert.IsType<ComparacaoRealizada>(resultado);
        }

        [Fact]
        public void Posicao_desconhecida_nao_e_compativel_com_nada()
        {
            // Act
            var resultado = Comparador().Comparar(
                Jogador(Neymar, posicao: null),
                Jogador(Rodrygo, Posicao.Ataque));

            // Assert
            Assert.Equal(
                MotivoDaRecusaDaComparacao.PosicaoDesconhecida,
                Assert.IsType<ComparacaoRecusada>(resultado).Motivo);
        }

        [Fact]
        public void Posicao_desconhecida_recusa_de_qualquer_lado_da_chamada()
        {
            // Act
            var resultado = Comparador().Comparar(
                Jogador(Neymar, Posicao.Ataque),
                Jogador(Rodrygo, posicao: null));

            // Assert
            Assert.Equal(
                MotivoDaRecusaDaComparacao.PosicaoDesconhecida,
                Assert.IsType<ComparacaoRecusada>(resultado).Motivo);
        }

        [Theory]
        [InlineData(384, 63814, ContextoDeRecorte.Clube)]
        [InlineData(325, 57296, ContextoDeRecorte.Clube)]
        [InlineData(325, 63814, ContextoDeRecorte.Selecao)]
        public void Conjuntos_de_recortes_diferentes_recusam_a_comparacao(
            int competicaoId, int temporadaId, ContextoDeRecorte contexto)
        {
            // Act
            var resultado = Comparador().Comparar(
                Jogador(Neymar, recorte: Brasileirao2024),
                Jogador(Rodrygo, recorte: new Recorte(competicaoId, temporadaId, contexto)));

            // Assert
            Assert.Equal(
                MotivoDaRecusaDaComparacao.RecorteDivergente,
                Assert.IsType<ComparacaoRecusada>(resultado).Motivo);
        }

        [Fact]
        public void Recorte_divergente_recusa_em_qualquer_ordem_da_chamada()
        {
            // Arrange
            var um = Jogador(Neymar, recorte: Brasileirao2024);
            var outro = Jogador(Rodrygo, recorte: Libertadores2024);

            // Act
            var umPrimeiro = Comparador().Comparar(um, outro);
            var outroPrimeiro = Comparador().Comparar(outro, um);

            // Assert
            Assert.Equal(umPrimeiro, outroPrimeiro);
            Assert.Equal(
                MotivoDaRecusaDaComparacao.RecorteDivergente,
                Assert.IsType<ComparacaoRecusada>(umPrimeiro).Motivo);
        }

        [Theory]
        [InlineData(200, 2400)]
        [InlineData(2400, 200)]
        public void Lado_sem_amostra_recusa_a_comparacao_inteira(int minutosDeUm, int minutosDoOutro)
        {
            // Act
            var resultado = Comparador().Comparar(
                Jogador(Neymar, minutos: minutosDeUm),
                Jogador(Rodrygo, minutos: minutosDoOutro));

            // Assert
            Assert.Equal(
                MotivoDaRecusaDaComparacao.AmostraInsuficiente,
                Assert.IsType<ComparacaoRecusada>(resultado).Motivo);
        }

        [Fact]
        public void Sao_comparados_os_atributos_que_os_dois_lados_apresentam()
        {
            // Act
            var resultado = Comparador().Comparar(Jogador(Neymar), Jogador(Rodrygo));

            // Assert
            var realizada = Assert.IsType<ComparacaoRealizada>(resultado);
            Assert.Equal(
                new[] { TipoDeAtributo.Gols, TipoDeAtributo.Assistencias, TipoDeAtributo.Rating },
                realizada.Atributos.Select(atributo => atributo.Tipo));
        }

        [Fact]
        public void A_comparacao_indexa_cada_atributo_pelos_dois_jogadores()
        {
            // Act
            var resultado = Comparador().Comparar(Jogador(Neymar), Jogador(Rodrygo));

            // Assert
            var realizada = Assert.IsType<ComparacaoRealizada>(resultado);
            Assert.NotEmpty(realizada.Atributos);
            Assert.All(
                realizada.Atributos,
                atributo => Assert.Equal([Rodrygo, Neymar], atributo.Valores.Keys.OrderBy(id => id)));
        }

        [Fact]
        public void A_comparacao_realizada_declara_o_recorte_dos_dois_lados()
        {
            // Act
            var resultado = Comparador().Comparar(Jogador(Neymar), Jogador(Rodrygo));

            // Assert
            Assert.Equal(Brasileirao2024, Assert.IsType<ComparacaoRealizada>(resultado).Recorte);
        }

        [Fact]
        public void Os_valores_de_cada_jogador_seguem_a_identidade_e_nao_a_ordem_da_chamada()
        {
            // Arrange
            var um = Jogador(Neymar, gols: 12);
            var outro = Jogador(Rodrygo, gols: 4);

            // Act
            var realizada = Assert.IsType<ComparacaoRealizada>(Comparador().Comparar(um, outro));

            // Assert
            var gols = Assert.Single(realizada.Atributos, atributo => atributo.Tipo == TipoDeAtributo.Gols);
            Assert.Equal(0.45m, Assert.IsType<AtributoCalculado>(gols.Valores[Neymar]).Valor);
            Assert.Equal(0.15m, Assert.IsType<AtributoCalculado>(gols.Valores[Rodrygo]).Valor);
        }

        [Fact]
        public void A_ordem_dos_jogadores_na_chamada_nao_altera_o_resultado()
        {
            // Arrange
            var um = Jogador(Neymar, gols: 12);
            var outro = Jogador(Rodrygo, gols: 4);

            // Act
            var umPrimeiro = Comparador().Comparar(um, outro);
            var outroPrimeiro = Comparador().Comparar(outro, um);

            // Assert
            Assert.Equal(Achatar(umPrimeiro), Achatar(outroPrimeiro));
        }

        [Fact]
        public void Derivado_que_a_fonte_nao_atribuiu_entra_no_par_como_recusa_sem_derrubar_a_comparacao()
        {
            // Act
            var resultado = Comparador().Comparar(
                Jogador(Neymar, rating: null),
                Jogador(Rodrygo, rating: 7.42m));

            // Assert
            var realizada = Assert.IsType<ComparacaoRealizada>(resultado);
            var rating = Assert.Single(realizada.Atributos, atributo => atributo.Tipo == TipoDeAtributo.Rating);
            Assert.Equal(
                MotivoDaRecusa.FonteNaoAtribuiu,
                Assert.IsType<AtributoRecusado>(rating.Valores[Neymar]).Motivo);
            Assert.Equal(7.42m, Assert.IsType<AtributoCalculado>(rating.Valores[Rodrygo]).Valor);
        }

        private static IEnumerable<(TipoDeAtributo Tipo, int JogadorId, ResultadoDeAtributo Valor)> Achatar(
            ResultadoDaComparacao resultado)
            => Assert.IsType<ComparacaoRealizada>(resultado).Atributos
                .SelectMany(atributo => atributo.Valores
                    .OrderBy(valor => valor.Key)
                    .Select(valor => (atributo.Tipo, valor.Key, valor.Value)))
                .ToList();

        private static ComparadorDeJogadores Comparador()
            => new(
                new PosicoesCompativeisSpecification(
                    new Dictionary<Posicao, IReadOnlyCollection<Posicao>>
                    {
                        [Posicao.Ataque] = [Posicao.MeioCampo]
                    }),
                new AmostraSuficienteSpecification(new AmostraMinima(450)));

        private static JogadorParaComparar Jogador(
            int jogadorId,
            Posicao? posicao = Posicao.Ataque,
            int minutos = 2400,
            Recorte? recorte = null,
            decimal? rating = 7.42m,
            int gols = 12)
        {
            var doRecorte = recorte ?? Brasileirao2024;
            var minutagem = new Minutagem(minutos, doRecorte);

            return new JogadorParaComparar(
                new PerfilDoJogador(
                    JogadorId: jogadorId,
                    Nome: "Jogador " + jogadorId,
                    Posicao: posicao,
                    Clube: "Santos",
                    ValorDeMercado: new Dinheiro(50_000_000_00, "EUR"),
                    MinutosJogados: minutos,
                    Recorte: doRecorte,
                    LidoEm: new DateTime(2026, 8, 25, 12, 0, 0, DateTimeKind.Utc)),
                new ConjuntoDeEstatisticas(
                    doRecorte,
                    [
                        new EstatisticaAcumulavel(TipoDeAtributo.Gols, gols, minutagem),
                        new EstatisticaAcumulavel(TipoDeAtributo.Assistencias, 7, minutagem)
                    ],
                    [new ValorDerivado(TipoDeAtributo.Rating, rating, minutagem)]));
        }
    }
}
