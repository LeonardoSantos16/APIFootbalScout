using APIFootballScout.Domain.Aggregate;
using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Domain.Dossie.Services
{
    public class DossieService
    {
        private readonly IDossieRepository _dossieRepository;
        private readonly int _limiteObservacoes;

        public DossieService(IDossieRepository dossieRepository, int limiteObservacoes)
        {
            _dossieRepository = dossieRepository;
            _limiteObservacoes = limiteObservacoes;
        }

        public async Task AdicionarDossie(Dossie dossie, CancellationToken cancellationToken)
        {
            var jogadorAcompanhado = await _dossieRepository.VerificarAcompanhamentoJogador(dossie.OlheiroId, dossie.JogadorId, cancellationToken);

            if (jogadorAcompanhado)
            {
                throw new InvalidOperationException("O jogador já está sendo acompanhado pelo olheiro.");
            }

            var dossiesAtivos = await _dossieRepository.ContarDossiesAtivosAsync(dossie.OlheiroId, cancellationToken);

            if (dossiesAtivos >= _limiteObservacoes)
            {
                throw new InvalidOperationException("Limite de jogadores em observação atingido.");
            }

            await _dossieRepository.AdicionarAsync(dossie, cancellationToken);
        }
    }
}
