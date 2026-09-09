using APIFootballScout.Domain.Base;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Domain.Analise.Specifications
{
    public sealed class PosicoesCompativeisSpecification : Specification<(Posicao Uma, Posicao Outra)>
    {
        private readonly HashSet<(Posicao, Posicao)> _declaradas;

        public PosicoesCompativeisSpecification(IReadOnlyDictionary<Posicao, IReadOnlyCollection<Posicao>> mapa)
        {
            _declaradas = [];

            foreach (var (posicao, compativeis) in mapa)
            {
                foreach (var compativel in compativeis)
                {
                    _declaradas.Add((posicao, compativel));
                    _declaradas.Add((compativel, posicao));
                }
            }
        }

        public override bool IsSatisfiedBy((Posicao Uma, Posicao Outra) par)
            => par.Uma == par.Outra || _declaradas.Contains(par);
    }
}
