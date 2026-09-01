using APIFootballScout.Domain.Base;
using APIFootballScout.Domain.Base.Exceptions;

namespace APIFootballScout.Domain.RelatorioScouting.Agreggate
{
    public class Relatorio : AggregateRootBase<Guid>
    {
        private readonly List<string> _pontosPositivos = [];
        private readonly List<string> _pontosNegativos = [];

        public int JogadorId { get; private set; }
        public Guid OlheiroId { get; private set; }
        public StatusRelatorio Status { get; private set; }
        public decimal? Nota { get; private set; }
        public IReadOnlyList<string> PontosPositivos => _pontosPositivos;
        public IReadOnlyList<string> PontosNegativos => _pontosNegativos;
        public string Texto { get; private set; }
        public string? Parecer { get; private set; }
        public DateTimeOffset ObservadoEm { get; private set; }
        public DateTimeOffset EscritoEm { get; private set; }
        public DateTimeOffset? FinalizadoEm { get; private set; }
        public Guid? CorrigeRelatorioId { get; private set; }

        private Relatorio(int jogadorId, Guid olheiroId, string texto,
            DateTimeOffset observadoEm, DateTimeOffset agora, Guid? corrigeRelatorioId)
            : base(Guid.NewGuid())
        {
            if (jogadorId <= 0)
                throw new ConflitoDeDominioException("relatorio.jogador_invalido", "jogador inválido");
            if (observadoEm > agora)
                throw new ConflitoDeDominioException("relatorio.observacao_futura", "observação no futuro");

            JogadorId = jogadorId;
            OlheiroId = olheiroId;
            Texto = texto;
            ObservadoEm = observadoEm;
            EscritoEm = agora;
            Status = StatusRelatorio.Rascunho;
            FinalizadoEm = null;
            CorrigeRelatorioId = corrigeRelatorioId;
        }

        public static Relatorio AbrirRascunho(int jogadorId, Guid olheiroId, string texto,
            DateTimeOffset observadoEm, DateTimeOffset agora)
            => new(jogadorId, olheiroId, texto, observadoEm, agora, null);

        public void AlterarTexto(string texto)
            => throw new NotImplementedException();

        public void AtribuirNota(decimal nota)
            => throw new NotImplementedException();

        public void AdicionarPontoPositivo(string ponto)
            => throw new NotImplementedException();

        public void AdicionarPontoNegativo(string ponto)
            => throw new NotImplementedException();

        public void DefinirParecer(string parecer)
            => throw new NotImplementedException();

        public void Finalizar(ISpecification<Relatorio> conteudoMinimo, DateTime escritoEm)
            => throw new NotImplementedException();
    }
}
