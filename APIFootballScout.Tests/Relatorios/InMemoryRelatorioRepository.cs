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

        // Sem ordenacao: a ordem da listagem e decisao do caso de uso, e e la que
        // os testes a provam. Aqui a ordem e a de insercao, de proposito.
        public Task<IReadOnlyList<Relatorio>> ListarPorJogadorAsync(
            int jogadorId, Guid olheiroId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Relatorio>>(
                [.. _relatorios.Where(r => r.JogadorId == jogadorId && r.OlheiroId == olheiroId)]);

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
