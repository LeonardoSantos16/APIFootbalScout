using APIFootballScout.Application.Acompanhamento;
using APIFootballScout.Application;
using APIFootballScout.Application.Configuration;
using APIFootballScout.Domain.Acompanhamento.Services;
using APIFootballScout.Domain.Acompanhamento.Specifications;
using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.CatalogoDeJogador;
using APIFootballScout.Domain.SharedKernel;
using Microsoft.Extensions.Time.Testing;

namespace APIFootballScout.Tests.Acompanhamento
{
    internal sealed class AcompanhamentoTestContext
    {
        public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 8, 25, 12, 0, 0, TimeSpan.Zero));
        public InMemoryAcompanhamentoRepository Dossies { get; } = new();
        public CatalogoDeJogadorFake Catalogo { get; } = new();
        public IAcompanhamentoService Servico { get; }
        public int Limite { get; set; } = 5;
        public Guid OlheiroId { get; } = Guid.NewGuid();
        public const int JogadorId = 42;
        public static readonly Recorte RecortePadrao = new(325, 63814, ContextoDeRecorte.Clube);

        public const int LimiarValorDeMercadoPercentual = 10;
        public const int LimiarMinutagemMinutos = 180;

        public AcompanhamentoTestContext()
        {
            Servico = new AcompanhamentoService(Dossies);
        }

        public AbrirAcompanhamentoUseCase AbrirDossie() => new(Dossies, Limite, Servico, Catalogo);

        public ConsultarMudancaAcompanhamentoUseCase ConsultarMudanca()
            => new(Dossies, Catalogo, Aferidor());

        public AferidorDeMudanca Aferidor()
        {
            var config = new ScoutConfig
            {
                LimiarValorDeMercadoPercentual = LimiarValorDeMercadoPercentual,
                LimiarMinutagemMinutos = LimiarMinutagemMinutos
            };

            var fabrica = new ScoutSpecificationFactory(
                Microsoft.Extensions.Options.Options.Create(config));

            return new AferidorDeMudanca(fabrica.MudancaRelevante(), new LeiturasComparaveisSpecification());
        }

        public PerfilDoJogador SeedPerfil(string clube = "Santos", DateTime? lidoEm = null)
            => Catalogo.Perfil = PerfilValido(clube, lidoEm);

        public PerfilDoJogador PerfilValido(string clube = "Santos", DateTime? lidoEm = null) => new(
            JogadorId: JogadorId, Nome: "Neymar", Posicao: "F", Clube: clube,
            ValorDeMercado: new Dinheiro(50_000_000_00, "EUR"),
            MinutosJogados: 900,
            Recorte: RecortePadrao,
            LidoEm: lidoEm ?? Agora.AddMinutes(-3));

        public DateTime Agora => Time.GetUtcNow().UtcDateTime;

        public AbrirAcompanhamentoRequest Pedido(
            Guid? olheiroId = null,
            int? jogadorId = null,
            Recorte? recorte = null)
        {
            var r = recorte ?? RecortePadrao;

            return new AbrirAcompanhamentoRequest(
                OlheiroId: olheiroId ?? OlheiroId,
                JogadorId: jogadorId ?? JogadorId,
                CompeticaoId: r.CompeticaoId,
                TemporadaId: r.TemporadaId,
                Contexto: r.Contexto);
        }

        public ConsultarMudancaAcompanhamentoRequest Consulta(
            Guid? olheiroId = null,
            int? jogadorId = null)
            => new(
                OlheiroId: olheiroId ?? OlheiroId,
                JogadorId: jogadorId ?? JogadorId);

        public async Task<PerfilDoJogador> ComLinhaDeBaseELeituraAtual(
            PerfilDoJogador linhaDeBase,
            PerfilDoJogador leituraAtual)
        {
            Catalogo.Perfil = linhaDeBase;
            await AbrirDossie().AbrirAcompanhamento(Pedido(), CancellationToken.None);

            Catalogo.Perfil = leituraAtual;
            return leituraAtual;
        }
    }
}
