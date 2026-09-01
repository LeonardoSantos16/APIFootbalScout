using APIFootballScout.Application.RelatorioScouting;
using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.RelatorioScouting.Agreggate;
using APIFootballScout.Domain.RelatorioScouting.ValueObject;

namespace APIFootballScout.Contracts.RelatorioScouting
{
    public static class RelatorioDtoMappers
    {
        public static AbrirRascunhoRelatorioRequest ParaRequest(
            this AbrirRascunhoRelatorioRequestDto dto, Guid olheiroId)
        {
            return (new AbrirRascunhoRelatorioRequest(
                OlheiroId: olheiroId,
                JogadorId: dto.JogadorId,
                Texto: dto.Texto,
                ObservadoEm: dto.ObservadoEm));
        }
        public static EditarRascunhoRelatorioRequest ParaRequest(
            this EditarRascunhoRelatorioRequestDto dto, Guid olheiroId, Guid relatorioId) => (new EditarRascunhoRelatorioRequest(
                OlheiroId: olheiroId,
                RelatorioId: relatorioId,
                Texto: dto.Texto,
                Nota: dto.Nota,
                PontosPositivos: dto.PontosPositivos,
                PontosNegativos: dto.PontosNegativos,
                Parecer: dto.Parecer.ParaDominio()));

        private static ParecerDto? ParaDto(this Parecer? parecer) => parecer switch
        {
            null => null,
            Parecer.Contratar => ParecerDto.Contratar,
            Parecer.Monitorar => ParecerDto.Monitorar,
            Parecer.Reavaliar => ParecerDto.Reavaliar,
            Parecer.Descartar => ParecerDto.Descartar,
            _ => throw new ValorInvalidoException(
                "relatorio.parecer_invalido",
                "o parecer informado não é um valor válido")
        };

        private static StatusRelatorioDto ParaDto(this StatusRelatorio status) => status switch
        {
            StatusRelatorio.Rascunho => StatusRelatorioDto.Rascunho,
            StatusRelatorio.Finalizado => StatusRelatorioDto.Finalizado,
            _ => throw new ValorInvalidoException(
                "relatorio.status_invalido",
                "o status do relatório não é um valor válido")
        };

        private static Parecer? ParaDominio(this ParecerDto? dto) => dto switch
        {
            null => null,
            ParecerDto.Contratar => Parecer.Contratar,
            ParecerDto.Monitorar => Parecer.Monitorar,
            ParecerDto.Reavaliar => Parecer.Reavaliar,
            ParecerDto.Descartar => Parecer.Descartar,
            _ => throw new ValorInvalidoException(
                "relatorio.parecer_invalido",
                "o parecer informado não é um valor válido")
        };

        public static RelatorioResponseDto ParaResponse(this RelatorioResult result)
        {
            return (new RelatorioResponseDto(
                RelatorioId: result.RelatorioId,
                JogadorId: result.JogadorId,
                Status: result.Status.ParaDto(),
                Texto: result.Texto,
                ObservadoEm: result.ObservadoEm,
                EscritoEm: result.EscritoEm,
                PontosNegativos: result.PontosNegativos,
                PontosPositivos: result.PontosPositivos,
                Nota: result.Nota,
                Parecer: result.Parecer.ParaDto(),
                FinalizadoEm: result.FinalizadoEm,
                CorrigeRelatorioId: result.CorrigeRelatorioId
                ));
        }
    }
}
