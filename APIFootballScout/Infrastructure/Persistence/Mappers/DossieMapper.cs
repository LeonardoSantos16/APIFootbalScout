using APIFootballScout.Domain.CatalagoDeJogador;
using APIFootballScout.Domain.Acompanhamento.Aggregate;
using APIFootballScout.Domain.Acompanhamento.ValueObject;
using APIFootballScout.Domain.SharedKernel;
using APIFootballScout.Infrastructure.Persistence.Documents;

namespace APIFootballScout.Infrastructure.Persistence.Mappers
{
    internal static class DossieMapper
    {
        public static Dossie MapToDomain(DossieDocument document)
        {
          var linhaDeBase = new LinhaDeBase(
                document.LinhaDeBase.MedidaEm,
                document.LinhaDeBase.Clube,
                new Dinheiro(document.LinhaDeBase.ValorEmCentavos, document.LinhaDeBase.Moeda),
                new Minutagem(
                    document.LinhaDeBase.Minutos,
                    new Recorte(
                        document.LinhaDeBase.CompeticaoId,
                        document.LinhaDeBase.TemporadaId,
                        (ContextoDeRecorte)document.LinhaDeBase.Contexto
                    )
                )
            );
            return Dossie.Restaurar(
                document.Id,
                document.JogadorId,
                document.OlheiroId,
                document.AbertoEm,
                (StatusDossie)document.Status,
                linhaDeBase,
                document.EncerradoEm
            );
        }
        public static DossieDocument MapToEntity(Dossie dossie) => new()
        {
            Id = dossie.Id,
            JogadorId = dossie.JogadorId,
            OlheiroId = dossie.OlheiroId,
            AbertoEm = dossie.AbertoEm,
            EncerradoEm = dossie.EncerradoEm,
            Status = (int)dossie.Status,
            LinhaDeBase = new LinhaDeBaseDocument
            {
                MedidaEm = dossie.LinhaDeBase.MedidaEm,
                Clube = dossie.LinhaDeBase.Clube,
                ValorEmCentavos = dossie.LinhaDeBase.ValorDeMercado.QuantiaEmCentavos,
                Moeda = dossie.LinhaDeBase.ValorDeMercado.Moeda,
                Minutos = dossie.LinhaDeBase.Minutagem.Minutos,
                CompeticaoId = dossie.LinhaDeBase.Minutagem.Recorte.CompeticaoId,
                TemporadaId = dossie.LinhaDeBase.Minutagem.Recorte.TemporadaId,
                Contexto = (int)dossie.LinhaDeBase.Minutagem.Recorte.Contexto
            }
        };
    }
}
