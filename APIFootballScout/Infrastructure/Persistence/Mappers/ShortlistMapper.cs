using APIFootballScout.Domain.SharedKernel;
using APIFootballScout.Domain.ShortlistPersonalizada.Agreggate;
using APIFootballScout.Domain.ShortlistPersonalizada.ValueObject;
using APIFootballScout.Infrastructure.Persistence.Documents;

namespace APIFootballScout.Infrastructure.Persistence.Mappers
{
    internal static class ShortlistMapper
    {
        public static Shortlist MapToDomain(this ShortlistDocument document)
            => Shortlist.Restaurar(id: document.Id, nome: document.Nome, olheiroId: document.OlheiroId, limite: new LimiteDeAlvos(document.LimiteDeAlvos), 
                alvos: [.. document.Alvos.Select(alvo => MapAlvoToDomain(alvo))]);



        public static ShortlistDocument MapToEntity(this Shortlist shortlist) => new()
        {
            Id = shortlist.Id,
            Nome = shortlist.Nome,
            LimiteDeAlvos = shortlist.Limite.Valor,
            Alvos = [.. shortlist.Alvos.Select(alvo => MapAlvoToEntity(alvo))],
            OlheiroId = shortlist.OlheiroId
        };

        private static AlvoDocument MapAlvoToEntity(Alvo alvo) => new()
        {
            JogadorId = alvo.JogadorId,
            Prioridade = alvo.Prioridade.Valor,
            CustoEmCentavos = alvo.CustoEstimado.QuantiaEmCentavos,
            Moeda = alvo.CustoEstimado.Moeda
        };

        private static Alvo MapAlvoToDomain(AlvoDocument alvo)
        {
            return new Alvo(JogadorId: alvo.JogadorId, Prioridade: new Prioridade(alvo.Prioridade), CustoEstimado:new Dinheiro(alvo.CustoEmCentavos, alvo.Moeda));     
        }


    }
}
