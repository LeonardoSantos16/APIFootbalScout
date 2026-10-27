using APIFootballScout.Domain.Acompanhamento.Aggregate;
using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Tests.Acompanhamento
{
    internal sealed class InMemoryAcompanhamentoRepository : IAcompanhamentoRepository
    {
        private readonly List<Dossie> _dossies = [];
        private readonly Dictionary<Guid, int> _vagasOcupadas = [];
        public IReadOnlyList<Dossie> Todos => _dossies;
        public int Atualizacoes { get; private set; }
        public Exception? FalhaNaInsercao { get; set; }

        public int VagasOcupadas(Guid olheiroId) => _vagasOcupadas.GetValueOrDefault(olheiroId);

        public Task AdicionarAsync(Dossie dossie, CancellationToken cancellationToken = default)
        {
            if (FalhaNaInsercao is not null)
            {
                throw FalhaNaInsercao;
            }

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

        public Task<bool> ReservarVagaDeAcompanhamentoAsync(Guid olheiroId, int limite, CancellationToken cancellationToken = default)
        {
            if (!_vagasOcupadas.TryGetValue(olheiroId, out var ocupadas))
            {
                ocupadas = _dossies.Count(d => d.OlheiroId == olheiroId && d.Status == StatusDossie.Ativo);
            }

            if (ocupadas >= limite)
            {
                _vagasOcupadas[olheiroId] = ocupadas;
                return Task.FromResult(false);
            }

            _vagasOcupadas[olheiroId] = ocupadas + 1;
            return Task.FromResult(true);
        }

        public Task LiberarVagaDeAcompanhamentoAsync(Guid olheiroId, CancellationToken cancellationToken = default)
        {
            if (_vagasOcupadas.GetValueOrDefault(olheiroId) > 0)
            {
                _vagasOcupadas[olheiroId]--;
            }

            return Task.CompletedTask;
        }

        public Task<List<Dossie>> ObterPorIdAsync(Guid olheiroId, int jogadorId, CancellationToken cancellationToken = default)
        {
            var dossies = _dossies.FindAll(d => d.OlheiroId == olheiroId && d.JogadorId == jogadorId);

            return Task.FromResult(dossies);
        }

        public Task RemoverTodosDoOlheiroAsync(Guid olheiroId, CancellationToken cancellationToken = default)
        {
            _dossies.RemoveAll(d => d.OlheiroId == olheiroId);
            _vagasOcupadas.Remove(olheiroId);

            return Task.CompletedTask;
        }

        public Task<bool> VerificarAcompanhamentoJogador(Guid olheiroId, int jogadorId, CancellationToken cancellationToken = default)
        {
            var dossie = _dossies.FindAll(d => d.OlheiroId == olheiroId && d.JogadorId == jogadorId && d.Status == StatusDossie.Ativo);

            return Task.FromResult(dossie.Count > 0);
        }
    }
}