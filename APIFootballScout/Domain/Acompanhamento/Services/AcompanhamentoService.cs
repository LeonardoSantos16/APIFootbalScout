using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Domain.Acompanhamento.Services
{
    public class AcompanhamentoService : IAcompanhamentoService
    {
        private readonly IAcompanhamentoRepository _acompanhamentoRepository;

        public AcompanhamentoService(IAcompanhamentoRepository acompanhamentoRepository)
        {
            _acompanhamentoRepository = acompanhamentoRepository;
        }

        public async Task<bool> VerificarAcompanhamentoJogadorAsync(Guid olheiroId, int jogadorId, CancellationToken cancellationToken)
        {
            return await _acompanhamentoRepository.VerificarAcompanhamentoJogador(olheiroId, jogadorId, cancellationToken);
        }

        public async Task<bool> VerificarLimiteDeAcompanhamentosAsync(Guid olheiroId, int limiteObservacoes, CancellationToken cancellationToken)
        {
            var dossiesAtivos = await _acompanhamentoRepository.ContarDossiesAtivosAsync(olheiroId, cancellationToken);
            return dossiesAtivos >= limiteObservacoes;
        }
    }
}
