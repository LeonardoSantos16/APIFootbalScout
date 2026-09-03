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

        public CriarShortlistUseCase Criar() => new(Shortlists, Politica());

        private IOptions<ScoutConfig> Politica()
            => Options.Create(new ScoutConfig { LimiteDeAlvosDaShortlist = LimiteDeAlvos });

        public CriarShortlistRequest PedidoDeCriacao(Guid? olheiroId = null, string? nome = null)
            => new(OlheiroId: olheiroId ?? OlheiroId, Nome: nome ?? Nome);

        public Shortlist Achar(Guid shortlistId)
            => Assert.Single(Shortlists.Todas, s => s.Id == shortlistId);

        public Shortlist Unica() => Assert.Single(Shortlists.Todas);
    }
}
