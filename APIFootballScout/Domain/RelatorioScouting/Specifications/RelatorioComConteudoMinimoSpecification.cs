using APIFootballScout.Domain.Base;
using APIFootballScout.Domain.RelatorioScouting.Agreggate;
namespace APIFootballScout.Domain.RelatorioScouting.Specifications
{
    public sealed class RelatorioComConteudoMinimoSpecification(
        int minimoDePros, int minimoDeContras, int minimoDeCaracteres) : Specification<Relatorio>
    {
        public override bool IsSatisfiedBy(Relatorio conteudo)
        =>
            conteudo.PontosPositivos.Count >= minimoDePros &&
            conteudo.PontosNegativos.Count >= minimoDeContras &&
            conteudo.PontosPositivos.Sum(p => p.Length) + conteudo.PontosNegativos.Sum(c => c.Length) >= minimoDeCaracteres;
        
    }
}
