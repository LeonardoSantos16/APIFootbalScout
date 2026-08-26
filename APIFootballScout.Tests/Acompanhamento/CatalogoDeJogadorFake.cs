using APIFootballScout.Domain.CatalogoDeJogador;

namespace APIFootballScout.Tests.Acompanhamento
{
    internal sealed class CatalogoDeJogadorFake : ICatalogoDeJogador
    {
        public PerfilDoJogador? Perfil { get; set; }
        public Recorte? UltimoRecorte { get; private set; }
        public int Chamadas { get; private set; }
        public Task<PerfilDoJogador?> ObterPerfilDoJogador(int jogadorId, Recorte recorte, CancellationToken ct = default)
        {
            Chamadas++;
            UltimoRecorte = recorte;
            return Task.FromResult(Perfil);
        }
    }
}
