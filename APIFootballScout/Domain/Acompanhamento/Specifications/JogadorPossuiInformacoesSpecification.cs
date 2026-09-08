using APIFootballScout.Domain.Base;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Domain.Acompanhamento.Specifications
{
    public sealed class JogadorPossuiInformacoesSpecification : Specification<PerfilDoJogador>
    {
        public override bool IsSatisfiedBy(PerfilDoJogador perfil)
        {
            return !string.IsNullOrWhiteSpace(perfil.Nome)
                && perfil.Posicao is not null
                && !string.IsNullOrWhiteSpace(perfil.Clube);
        }
    }
}
