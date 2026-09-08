using APIFootballScout.Domain.Base;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Domain.Analise.Specifications
{
    public sealed class PosicoesCompativeisSpecification : Specification<(Posicao, Posicao)>
    {
        public PosicoesCompativeisSpecification(IReadOnlyDictionary<Posicao, IReadOnlyCollection<Posicao>> mapa)
        {
        }

        public override bool IsSatisfiedBy((Posicao, Posicao) par) => throw new NotImplementedException();
    }
}
