using APIFootballScout.Domain.RelatorioScouting.Agreggate;
using APIFootballScout.Domain.Repository;

namespace APIFootballScout.Tests.Relatorios
{
    internal sealed class InMemoryRelatorioRepository : IRelatorioRepository
    {
        private readonly List<Relatorio> _relatorios = [];

        public IReadOnlyList<Relatorio> Todos => _relatorios;
        public int Atualizacoes { get; private set; }

        public Task AdicionarAsync(Relatorio relatorio, CancellationToken cancellationToken = default)
        {
            _relatorios.Add(relatorio);
            return Task.CompletedTask;
        }

        public Task<Relatorio?> ObterPorIdAsync(Guid relatorioId, Guid olheiroId, CancellationToken cancellationToken = default)
            => Task.FromResult(_relatorios.Find(r => r.Id == relatorioId && r.OlheiroId == olheiroId));

        public Task AtualizarAsync(Relatorio relatorio, CancellationToken cancellationToken = default)
        {
            Atualizacoes++;

            var indice = _relatorios.FindIndex(r => r.Id == relatorio.Id);

            if (indice != -1)
                _relatorios[indice] = relatorio;

            return Task.CompletedTask;
        }
    }
}
