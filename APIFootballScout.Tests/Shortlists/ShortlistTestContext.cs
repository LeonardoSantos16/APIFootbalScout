using APIFootballScout.Application.Configuration;
using APIFootballScout.Application.ShortlistPersonalizada;
using APIFootballScout.Domain.SharedKernel;
using APIFootballScout.Domain.ShortlistPersonalizada.Agreggate;
using Microsoft.Extensions.Options;

namespace APIFootballScout.Tests.Shortlists
{
    internal sealed class ShortlistTestContext
    {
        public const string Nome = "Laterais esquerdos 2026";

        public InMemoryShortlistRepository Shortlists { get; } = new();
        public Guid OlheiroId { get; } = Guid.NewGuid();

        public int LimiteDeAlvos { get; set; } = 25;

        public static Dinheiro Euros(long milhoes) => new(milhoes * 1_000_000_00, "EUR");

        public static Dinheiro Libras(long milhoes) => new(milhoes * 1_000_000_00, "GBP");

        public CriarShortlistUseCase Criar() => new(Shortlists, Politica());

        public AdicionarAlvoUseCase Adicionar() => new(Shortlists);

        public RemoverAlvoUseCase Remover() => new(Shortlists);

        private IOptions<ScoutConfig> Politica()
            => Options.Create(new ScoutConfig { LimiteDeAlvosDaShortlist = LimiteDeAlvos });

        public CriarShortlistRequest PedidoDeCriacao(Guid? olheiroId = null, string? nome = null)
            => new(OlheiroId: olheiroId ?? OlheiroId, Nome: nome ?? Nome);

        public AdicionarAlvoRequest PedidoDeAdicao(
            Guid shortlistId,
            int jogadorId = 1001,
            int prioridade = 1,
            Guid? olheiroId = null,
            Dinheiro? custoEstimado = null)
            => new(
                OlheiroId: olheiroId ?? OlheiroId,
                ShortlistId: shortlistId,
                JogadorId: jogadorId,
                Prioridade: prioridade,
                CustoEstimado: custoEstimado ?? Euros(5));

        public RemoverAlvoRequest PedidoDeRemocao(
            Guid shortlistId, int jogadorId, Guid? olheiroId = null)
            => new(
                OlheiroId: olheiroId ?? OlheiroId,
                ShortlistId: shortlistId,
                JogadorId: jogadorId);

        public async Task<Guid> ShortlistCriada(Guid? olheiroId = null, string? nome = null)
        {
            var criada = await Criar().CriarShortlist(
                PedidoDeCriacao(olheiroId, nome), CancellationToken.None);

            return criada.ShortlistId;
        }

        public async Task<Guid> ShortlistCom(params int[] jogadores)
        {
            var shortlistId = await ShortlistCriada();

            for (var posicao = 1; posicao <= jogadores.Length; posicao++)
                await Adicionar().AdicionarAlvo(
                    PedidoDeAdicao(shortlistId, jogadores[posicao - 1], posicao),
                    CancellationToken.None);

            return shortlistId;
        }

        public Shortlist Achar(Guid shortlistId)
            => Assert.Single(Shortlists.Todas, s => s.Id == shortlistId);

        public Shortlist Unica() => Assert.Single(Shortlists.Todas);

        public static (int Jogador, int Prioridade)[] Ordem(Shortlist shortlist)
            => [.. shortlist.Alvos.Select(alvo => (alvo.JogadorId, alvo.Prioridade.Valor))];
    }
}
