using APIFootballScout.Application.Configuration;
using APIFootballScout.Domain.Acompanhamento.Specifications;
using APIFootballScout.Domain.Acompanhamento.ValueObject;
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
    }
}
