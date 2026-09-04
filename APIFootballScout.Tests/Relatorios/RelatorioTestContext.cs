using APIFootballScout.Application;
using APIFootballScout.Application.Configuration;
using APIFootballScout.Application.RelatorioScouting;
using APIFootballScout.Domain.RelatorioScouting.Agreggate;
using APIFootballScout.Domain.RelatorioScouting.ValueObject;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;

namespace APIFootballScout.Tests.Relatorios
{
    internal sealed class RelatorioTestContext
    {
        public const int JogadorId = 42;
        public const string TextoInicial = "Bom posicionamento sem bola.";

        public static readonly DateTimeOffset ObservadoEm = new(2026, 8, 20, 15, 0, 0, TimeSpan.Zero);

        public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 8, 25, 12, 0, 0, TimeSpan.Zero));
        public InMemoryRelatorioRepository Relatorios { get; } = new();
        public Guid OlheiroId { get; } = Guid.NewGuid();

        public int MinimoDePros { get; set; }
        public int MinimoDeContras { get; set; }
        public int MinimoDeCaracteresDaObservacao { get; set; }

        public DateTimeOffset Agora => Time.GetUtcNow();

        public AbrirRascunhoRelatorioUseCase AbrirRascunho() => new(Relatorios, Time);

        public EditarRascunhoRelatorioUseCase EditarRascunho() => new(Relatorios);

        public FinalizarRelatorioUseCase Finalizar() => new(Relatorios, Especificacoes(), Time);

        public CorrigirRelatorioUseCase Corrigir() => new(Relatorios, Time);

        public ObterRelatorioUseCase Obter() => new(Relatorios);

        public ListarRelatoriosDoJogadorUseCase Listar() => new(Relatorios);

        private ScoutSpecificationFactory Especificacoes()
            => new(Options.Create(new ScoutConfig
            {
                MinimoDePros = MinimoDePros,
                MinimoDeContras = MinimoDeContras,
                MinimoDeCaracteresDaObservacao = MinimoDeCaracteresDaObservacao
            }));

        public AbrirRascunhoRelatorioRequest PedidoDeAbertura(
            Guid? olheiroId = null,
            int? jogadorId = null,
            string? texto = null,
            DateTimeOffset? observadoEm = null)
            => new(
                OlheiroId: olheiroId ?? OlheiroId,
                JogadorId: jogadorId ?? JogadorId,
                Texto: texto ?? TextoInicial,
                ObservadoEm: observadoEm ?? ObservadoEm);

        public EditarRascunhoRelatorioRequest PedidoDeEdicao(
            Guid relatorioId,
            Guid? olheiroId = null,
            string? texto = null,
            decimal? nota = null,
            IReadOnlyList<string>? pontosPositivos = null,
            IReadOnlyList<string>? pontosNegativos = null,
            Parecer? parecer = null)
            => new(
                OlheiroId: olheiroId ?? OlheiroId,
                RelatorioId: relatorioId,
                Texto: texto,
                Nota: nota,
                PontosPositivos: pontosPositivos,
                PontosNegativos: pontosNegativos,
                Parecer: parecer);

        public FinalizarRelatorioRequest PedidoDeFinalizacao(Guid relatorioId, Guid? olheiroId = null)
            => new(OlheiroId: olheiroId ?? OlheiroId, RelatorioId: relatorioId);

        public const string TextoDaCorrecao = "Revisto: erra a saida de bola.";

        public CorrigirRelatorioRequest PedidoDeCorrecao(
            Guid relatorioId, Guid? olheiroId = null, string? texto = null)
            => new(
                OlheiroId: olheiroId ?? OlheiroId,
                RelatorioId: relatorioId,
                Texto: texto ?? TextoDaCorrecao);

        public ObterRelatorioRequest PedidoDeConsulta(Guid relatorioId, Guid? olheiroId = null)
            => new(OlheiroId: olheiroId ?? OlheiroId, RelatorioId: relatorioId);

        public ListarRelatoriosDoJogadorRequest PedidoDeListagem(
            Guid? olheiroId = null, int? jogadorId = null)
            => new(OlheiroId: olheiroId ?? OlheiroId, JogadorId: jogadorId ?? JogadorId);

        public Relatorio Achar(Guid relatorioId)
            => Assert.Single(Relatorios.Todos, r => r.Id == relatorioId);

        /// <summary>
        /// Percorre o fluxo pelos use cases ate o relatorio ficar pronto para a
        /// finalizacao: abre o rascunho e o conclui com nota, pontos e parecer.
        /// </summary>
        public async Task<Guid> RascunhoConcluido(Guid? olheiroId = null)
        {
            var aberto = await AbrirRascunho().AbrirRascunho(
                PedidoDeAbertura(olheiroId), CancellationToken.None);

            await EditarRascunho().EditarRascunho(
                PedidoDeEdicao(
                    aberto.RelatorioId,
                    olheiroId,
                    nota: 8.5m,
                    pontosPositivos: ["Leitura de jogo"],
                    pontosNegativos: ["Fragilidade defensiva"],
                    parecer: Parecer.Contratar),
                CancellationToken.None);

            return aberto.RelatorioId;
        }

        public async Task<Guid> RelatorioFinalizado(Guid? olheiroId = null)
        {
            var relatorioId = await RascunhoConcluido(olheiroId);

            await Finalizar().FinalizarRelatorio(
                PedidoDeFinalizacao(relatorioId, olheiroId), CancellationToken.None);

            return relatorioId;
        }

        /// <summary>
        /// Conclui um rascunho que ja existe (nota, pontos e parecer) e o finaliza.
        /// </summary>
        public async Task ConcluirEFinalizar(Guid relatorioId, Guid? olheiroId = null)
        {
            await EditarRascunho().EditarRascunho(
                PedidoDeEdicao(
                    relatorioId,
                    olheiroId,
                    nota: 8.5m,
                    pontosPositivos: ["Leitura de jogo"],
                    pontosNegativos: ["Fragilidade defensiva"],
                    parecer: Parecer.Contratar),
                CancellationToken.None);

            await Finalizar().FinalizarRelatorio(
                PedidoDeFinalizacao(relatorioId, olheiroId), CancellationToken.None);
        }

        public Relatorio Unico() => Assert.Single(Relatorios.Todos);
    }
}
