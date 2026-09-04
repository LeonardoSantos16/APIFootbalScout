using APIFootballScout.Application.Configuration;
using APIFootballScout.Domain.Acompanhamento.Specifications;
using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.RelatorioScouting.Specifications;
using Microsoft.Extensions.Options;

namespace APIFootballScout.Application
{
    public sealed class ScoutSpecificationFactory(IOptions<ScoutConfig> options)
    {
        private readonly ScoutConfig _config = options.Value;

        public JogadorPrincipalSpecification JogadorPrincipal()
            => new(_config.PrincipaisJogadores);

        public TorneioPrincipalSpecification TorneioPrincipal()
            => new(_config.PrincipaisTorneios);

        public MudancaRelevanteSpecification MudancaRelevante()
        => new(new LimiarPercentual(_config.LimiarValorDeMercadoPercentual),
               new LimiarAbsoluto(_config.LimiarMinutagemMinutos));

        public RelatorioComConteudoMinimoSpecification ConteudoMinimoDoRelatorio()
        => new(_config.MinimoDePros, _config.MinimoDeContras, _config.MinimoDeCaracteresDaObservacao);
    }
}
