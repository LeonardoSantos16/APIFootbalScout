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

        public static Relatorio AbrirCorrecao(Relatorio original, string texto, DateTimeOffset agora)
            => throw new NotImplementedException();

        public void AlterarTexto(string texto)
        {
            GarantirEditavel();
            if (string.IsNullOrWhiteSpace(texto))
                throw new ConflitoDeDominioException("relatorio.texto_obrigatorio", "texto é obrigatório");
            Texto = texto;
        }

        public void AtribuirNota(decimal nota)
        {
            GarantirEditavel();
            Nota = nota;
        }

        public void AdicionarPontoPositivo(string ponto)
        {
            GarantirEditavel();
            _pontosPositivos.Add(ponto);
        }

        public void AdicionarPontoNegativo(string ponto)
        {
            GarantirEditavel();
            _pontosNegativos.Add(ponto);
        }

        public void DefinirParecer(string parecer)
        {
            GarantirEditavel();
            if (string.IsNullOrWhiteSpace(parecer))
                throw new ConflitoDeDominioException("relatorio.parecer_obrigatorio", "parecer é obrigatório");
            Parecer = parecer;
        }

        public void Finalizar(ISpecification<Relatorio> conteudoMinimo, DateTime escritoEm)
        {
            GarantirEditavel();

            if (Nota is null || Parecer is null)
                throw new RegraDeNegocioException(
                    "relatorio.conclusao_ausente",
                    "não é possível finalizar o relatório sem nota e parecer");

            if (!conteudoMinimo.IsSatisfiedBy(this))
                throw new RegraDeNegocioException(
                    "relatorio.conteudo_minimo_nao_atendido",
                    "o relatório não atende ao conteúdo mínimo exigido para a finalização");

            Status = StatusRelatorio.Finalizado;
            EscritoEm = escritoEm;
        }

        private void GarantirEditavel()
        {
            if (Status is StatusRelatorio.Finalizado)
                throw new ConflitoDeDominioException(
                    "relatorio.ja_finalizado",
                    "relatório finalizado é imutável; emita um relatório de correção");
        }
    }
}
