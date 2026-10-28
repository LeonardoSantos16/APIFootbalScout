namespace APIFootballScout.Domain.Acompanhamento.Services
{
    public interface IAcompanhamentoService
    {
        Task<bool> VerificarAcompanhamentoJogadorAsync(Guid olheiroId, int jogadorId, CancellationToken cancellationToken);
    }
}