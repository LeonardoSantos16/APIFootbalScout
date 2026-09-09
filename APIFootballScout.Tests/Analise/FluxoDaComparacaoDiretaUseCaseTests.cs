using APIFootballScout.Application.Analise;
using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.Analise.Services;
using APIFootballScout.Domain.Analise.Specifications;
using APIFootballScout.Domain.Analise.ValueObject;
using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.CatalogoDeJogador;
using APIFootballScout.Domain.SharedKernel;

namespace APIFootballScout.Tests.Analise
{
    public class FluxoDaComparacaoDiretaUseCaseTests
    {
        private static readonly Recorte Brasileirao2024 = new(325, 63814, ContextoDeRecorte.Clube);

        private const int Neymar = 13812;
        private const int Rodrygo = 8256;

        private readonly CatalogoDeDoisJogadoresFake _catalogo = new();
        private readonly ComparacaoDiretaUseCase _useCase;

        public FluxoDaComparacaoDiretaUseCaseTests()
        {
            _useCase = new ComparacaoDiretaUseCase(
                _catalogo,
                new ComparadorDeJogadores(
                    new PosicoesCompativeisSpecification(
                        new Dictionary<Posicao, IReadOnlyCollection<Posicao>>
                        {
                            [Posicao.Ataque] = [Posicao.MeioCampo]
                        }),
                    new AmostraSuficienteSpecification(new AmostraMinima(450))));
        }

        [Fact]
        public async Task A_comparacao_busca_perfil_e_estatisticas_dos_dois_jogadores_no_mesmo_recorte()
        {
            // Arrange
            Registrar(Neymar);
            Registrar(Rodrygo);

            // Act
            await _useCase.Comparar(Requisicao(), CancellationToken.None);

            // Assert
            Assert.Equal(4, _catalogo.Chamadas);
            Assert.All(_catalogo.RecortesPedidos, recorte => Assert.Equal(Brasileirao2024, recorte));
        }

        [Fact]
        public async Task A_comparacao_realizada_atravessa_o_resultado_com_os_atributos()
        {
            // Arrange
            Registrar(Neymar, gols: 12);
            Registrar(Rodrygo, gols: 4);

            // Act
            var result = await _useCase.Comparar(Requisicao(), CancellationToken.None);

            // Assert
            var realizada = Assert.IsType<ComparacaoRealizada>(result.Resultado);
            Assert.Equal(Brasileirao2024, realizada.Recorte);
            var gols = Assert.Single(realizada.Atributos, atributo => atributo.Tipo == TipoDeAtributo.Gols);
            Assert.Equal(0.45m, Assert.IsType<AtributoCalculado>(gols.Valores[Neymar]).Valor);
            Assert.Equal(0.15m, Assert.IsType<AtributoCalculado>(gols.Valores[Rodrygo]).Valor);
        }

        [Fact]
        public async Task O_resultado_declara_os_dois_jogadores_ordenados_pela_identidade()
        {
            // Arrange
            Registrar(Neymar, nome: "Neymar");
            Registrar(Rodrygo, nome: "Rodrygo");

            // Act
            var result = await _useCase.Comparar(Requisicao(), CancellationToken.None);

            // Assert
            Assert.Equal(
                [
                    new JogadorNaComparacao(Rodrygo, "Rodrygo", Posicao.Ataque),
                    new JogadorNaComparacao(Neymar, "Neymar", Posicao.Ataque)
                ],
                result.Jogadores);
        }

        [Fact]
        public async Task A_recusa_do_dominio_atravessa_o_resultado_com_os_jogadores()
        {
            // Arrange
            Registrar(Neymar, posicao: Posicao.Ataque);
            Registrar(Rodrygo, posicao: Posicao.Goleiro);

            // Act
            var result = await _useCase.Comparar(Requisicao(), CancellationToken.None);

            // Assert
            Assert.Equal(
                MotivoDaRecusaDaComparacao.PosicoesIncompativeis,
                Assert.IsType<ComparacaoRecusada>(result.Resultado).Motivo);
            Assert.Equal(2, result.Jogadores.Count);
        }

        [Fact]
        public async Task Jogador_sem_perfil_no_recorte_pedido_nao_e_encontrado()
        {
            // Arrange
            Registrar(Neymar);

            // Act
            var erro = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
                () => _useCase.Comparar(Requisicao(), CancellationToken.None));

            // Assert
            Assert.Equal("jogador.perfil_nao_encontrado", erro.Codigo);
        }

        [Fact]
        public async Task Jogador_sem_estatisticas_no_recorte_pedido_nao_e_encontrado()
        {
            // Arrange
            Registrar(Neymar);
            _catalogo.Registrar(Perfil(Rodrygo, "Rodrygo", Posicao.Ataque), null);

            // Act
            var erro = await Assert.ThrowsAsync<RecursoNaoEncontradoException>(
                () => _useCase.Comparar(Requisicao(), CancellationToken.None));

            // Assert
            Assert.Equal("jogador.estatisticas_nao_encontradas", erro.Codigo);
        }

        private static ComparacaoDiretaRequest Requisicao()
            => new(
                JogadorA: Neymar,
                JogadorB: Rodrygo,
                CompeticaoId: 325,
                TemporadaId: 63814,
                Contexto: ContextoDeRecorte.Clube);

        private void Registrar(
            int jogadorId,
            string nome = "Jogador",
            Posicao? posicao = Posicao.Ataque,
            int gols = 12,
            int minutos = 2400)
        {
            var minutagem = new Minutagem(minutos, Brasileirao2024);

            _catalogo.Registrar(
                Perfil(jogadorId, nome, posicao),
                new ConjuntoDeEstatisticas(
                    Brasileirao2024,
                    [
                        new EstatisticaAcumulavel(TipoDeAtributo.Gols, gols, minutagem),
                        new EstatisticaAcumulavel(TipoDeAtributo.Assistencias, 7, minutagem)
                    ],
                    [new ValorDerivado(TipoDeAtributo.Rating, 7.42m, minutagem)]));
        }

        private static PerfilDoJogador Perfil(int jogadorId, string nome, Posicao? posicao)
            => new(
                JogadorId: jogadorId,
                Nome: nome,
                Posicao: posicao,
                Clube: "Santos",
                ValorDeMercado: new Dinheiro(50_000_000_00, "EUR"),
                MinutosJogados: 2400,
                Recorte: Brasileirao2024,
                LidoEm: new DateTime(2026, 8, 25, 12, 0, 0, DateTimeKind.Utc));
    }
}
