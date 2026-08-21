namespace APIFootballScout.Domain.Acompanhamento.Services
{
    public interface IAcompanhamentoService
    {
        Task<bool> VerificarLimiteDeAcompanhamentosAsync(Guid olheiroId, int limiteObservacoes, CancellationToken cancellationToken);
        Task<bool> VerificarAcompanhamentoJogadorAsync(Guid olheiroId, int jogadorId, CancellationToken cancellationToken);
    }
}
