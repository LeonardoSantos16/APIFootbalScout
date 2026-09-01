using APIFootballScout.Domain.RelatorioScouting.Agreggate;
using APIFootballScout.Domain.RelatorioScouting.ValueObject;
using APIFootballScout.Infrastructure.Persistence.Documents;

namespace APIFootballScout.Infrastructure.Persistence.Mappers
{
    internal static class RelatorioMapper
    {
        public static Relatorio MapToDomain(RelatorioDocument document)
        {
            return Relatorio.Restaurar(
                id: document.ID,
                jogadorId: document.JogadorId,
                olheiroId: document.OlheiroId,
                status: ParseOuFalhar<StatusRelatorio>(document.Status, document.ID),
                texto: document.Texto,
                observadoEm: document.ObservadoEm,
                escritoEm: document.Escrito_em,
                pontosNegativos: document.PontosNegativos,
                pontosPositivos: document.PontosPositivos,
                nota: Nota.FromDecimal(document.Nota),
                parecer: ParseOuNulo<Parecer>(document.Parecer, document.ID),
                finalizadoEm: document.FinalizadoEm,
                corrigeRelatorioId: document.CorrigeRelatorioId
                );
        }

        public static RelatorioDocument MapToEntity(Relatorio relatorio) => new()
            {
                ID = relatorio.Id,
                JogadorId = relatorio.JogadorId,
                OlheiroId = relatorio.OlheiroId,
                Status = relatorio.Status.ToString(),
                Texto = relatorio.Texto,
                ObservadoEm = relatorio.ObservadoEm.UtcDateTime,
                Escrito_em = relatorio.EscritoEm.UtcDateTime,
                PontosPositivos = [.. relatorio.PontosPositivos],
                PontosNegativos = [.. relatorio.PontosNegativos],
                Nota = relatorio.Nota?.Valor,
                Parecer = relatorio.Parecer?.ToString(),
                FinalizadoEm = relatorio.FinalizadoEm?.UtcDateTime,
                CorrigeRelatorioId = relatorio.CorrigeRelatorioId
            };

        private static T ParseOuFalhar<T>(string valor, Guid documentoId) where T : struct, Enum
        => Enum.TryParse<T>(valor, ignoreCase: true, out var resultado) && Enum.IsDefined(resultado)
            ? resultado
            : throw new InvalidOperationException(
                $"Valor '{valor}' inválido para {typeof(T).Name} no documento {documentoId}.");

        private static T? ParseOuNulo<T>(string? valor, Guid documentoId) where T : struct, Enum
            => valor is null ? null : ParseOuFalhar<T>(valor, documentoId);
    }
}
