using APIFootballScout.Domain.Analise.ValueObject;
using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Tests.Analise
{
    internal sealed class CatalogoDeDoisJogadoresFake : ICatalogoDeJogador
    {
        private readonly Dictionary<int, PerfilDoJogador> _perfis = [];
        private readonly Dictionary<int, ConjuntoDeEstatisticas> _conjuntos = [];

        public List<Recorte> RecortesPedidos { get; } = [];
        public int Chamadas { get; private set; }

        public void Registrar(PerfilDoJogador perfil, ConjuntoDeEstatisticas? estatisticas)
        {
            _perfis[perfil.JogadorId] = perfil;

            if (estatisticas is not null)
            {
                _conjuntos[perfil.JogadorId] = estatisticas;
            }
        }

        public Task<PerfilDoJogador?> ObterPerfilDoJogador(int jogadorId, Recorte recorte, CancellationToken ct = default)
        {
            Chamadas++;
            RecortesPedidos.Add(recorte);
            return Task.FromResult(_perfis.TryGetValue(jogadorId, out var perfil) ? perfil : null);
        }

        public Task<ConjuntoDeEstatisticas?> ObterEstatisticas(int jogadorId, Recorte recorte, CancellationToken ct = default)
        {
            Chamadas++;
            RecortesPedidos.Add(recorte);
            return Task.FromResult(_conjuntos.TryGetValue(jogadorId, out var conjunto) ? conjunto : null);
        }
    }
}
