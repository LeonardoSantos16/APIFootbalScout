using APIFootballScout.Domain.Acompanhamento.Aggregate;
using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Tests.Acompanhamento
{
    internal class InMemoryAcompanhamentoRepository : IAcompanhamentoRepository
    {
        private readonly List<Dossie> _dossies = [];
        public IReadOnlyList<Dossie> Todos => _dossies;
        public int Atualizacoes { get; private set; }

        public Task AdicionarAsync(Dossie dossie, CancellationToken cancellationToken = default)
        {
            _dossies.Add(dossie);
            return Task.CompletedTask;
        }

        public Task AtualizarAsync(Dossie dossie, CancellationToken cancellationToken = default)
        {
            Atualizacoes++;

            int indice = _dossies.FindIndex(d => d.Id == dossie.Id);

            if (indice != -1)
            {
                _dossies[indice] = dossie;
            }

            return Task.CompletedTask;
        }

        public Task<int> ContarDossiesAtivosAsync(Guid olheiroId, CancellationToken cancellationToken = default)
        {
            var contDossie = _dossies.Count(d => d.OlheiroId == olheiroId && d.Status == StatusDossie.Ativo);

            return Task.FromResult(contDossie);
        }

        public Task<Dossie?> ObterPorIdAsync(Guid olheiroId, int jogadorId, CancellationToken cancellationToken = default)
        {
            var dossie = _dossies.Find(d => d.OlheiroId == olheiroId && d.JogadorId == jogadorId);

            return Task.FromResult(dossie);
        }

        public Task RemoverTodosDoOlheiroAsync(Guid olheiroId, CancellationToken cancellationToken = default)
        {
            _dossies.RemoveAll(d => d.OlheiroId == olheiroId);

            return Task.CompletedTask;
        }

        public Task<bool> VerificarAcompanhamentoJogador(Guid olheiroId, int jogadorId, CancellationToken cancellationToken = default)
        {
            var dossie = _dossies.FindAll(d => d.OlheiroId == olheiroId && d.JogadorId == jogadorId && d.Status == StatusDossie.Ativo);

            return Task.FromResult(dossie.Count > 0);           
        }
    }
}
