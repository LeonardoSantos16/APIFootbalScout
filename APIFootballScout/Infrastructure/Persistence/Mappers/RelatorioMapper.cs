using APIFootballScout.Domain.RelatorioScouting.Aggregate;
using APIFootballScout.Domain.RelatorioScouting.ValueObject;
using APIFootballScout.Infrastructure.Persistence.Documents;

namespace APIFootballScout.Infrastructure.Persistence.Mappers
{
    internal static class RelatorioMapper
    {
        public static Relatorio MapToDomain(RelatorioDocument document)
        {
            var relatorio = Relatorio.Restaurar(
                id: document.Id,
                jogadorId: document.JogadorId,
                olheiroId: document.OlheiroId,
                status: ParseOuFalhar<StatusRelatorio>(document.Status, document.Id),
                texto: document.Texto,
                observadoEm: document.ObservadoEm,
                escritoEm: document.EscritoEm,
                pontosNegativos: document.PontosNegativos,
                pontosPositivos: document.PontosPositivos,
                nota: Nota.FromDecimal(document.Nota),
                parecer: ParseOuNulo<Parecer>(document.Parecer, document.Id),
                finalizadoEm: document.FinalizadoEm,
                corrigeRelatorioId: document.CorrigeRelatorioId
                );
            relatorio.AtualizarVersao(document.Version);

            return relatorio;
        }

        public static RelatorioDocument MapToEntity(Relatorio relatorio) => new()
        {
            Id = relatorio.Id,
            JogadorId = relatorio.JogadorId,
            OlheiroId = relatorio.OlheiroId,
            Status = relatorio.Status.ToString(),
            Texto = relatorio.Texto,
            ObservadoEm = relatorio.ObservadoEm.UtcDateTime,
            EscritoEm = relatorio.EscritoEm.UtcDateTime,
            PontosPositivos = [.. relatorio.PontosPositivos],
            PontosNegativos = [.. relatorio.PontosNegativos],
            Nota = relatorio.Nota?.Valor,
            Parecer = relatorio.Parecer?.ToString(),
            FinalizadoEm = relatorio.FinalizadoEm?.UtcDateTime,
            CorrigeRelatorioId = relatorio.CorrigeRelatorioId,
            Version = relatorio.Version
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