using APIFootballScout.Domain.Base;
using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.RelatorioScouting.ValueObject;

namespace APIFootballScout.Domain.RelatorioScouting.Agreggate
{
    public class Relatorio : AggregateRootBase<Guid>
    {
        private readonly List<string> _pontosPositivos = [];
        private readonly List<string> _pontosNegativos = [];

        public int JogadorId { get; private set; }
        public Guid OlheiroId { get; private set; }
        public StatusRelatorio Status { get; private set; }
        public Nota? Nota { get; private set; }
        public IReadOnlyList<string> PontosPositivos => _pontosPositivos;
        public IReadOnlyList<string> PontosNegativos => _pontosNegativos;
        public string Texto { get; private set; }
        public Parecer? Parecer { get; private set; }
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

        private Relatorio(Guid id) : base(id)
        {
            Texto = string.Empty;
        }

        public static Relatorio AbrirRascunho(int jogadorId, Guid olheiroId, string texto,
            DateTimeOffset observadoEm, DateTimeOffset agora)
            => new(jogadorId, olheiroId, texto, observadoEm, agora, corrigeRelatorioId: null);

        public static Relatorio AbrirCorrecao(Relatorio original, string texto, DateTimeOffset agora)
        {
            if (original.Status is not StatusRelatorio.Finalizado)
                throw new ConflitoDeDominioException(
                    "relatorio.correcao_de_rascunho",
                    "só relatório finalizado pode ser corrigido");

            return new Relatorio(original.JogadorId, original.OlheiroId, texto,
                                 original.ObservadoEm, agora, original.Id);
        }

        public static Relatorio Restaurar(
            Guid id, int jogadorId, Guid olheiroId, StatusRelatorio status, string texto,
            Nota? nota, IEnumerable<string> pontosPositivos, IEnumerable<string> pontosNegativos,
            Parecer? parecer, DateTimeOffset observadoEm, DateTimeOffset escritoEm,
            DateTimeOffset? finalizadoEm, Guid? corrigeRelatorioId)
        {
            var relatorio = new Relatorio(id)
            {
                JogadorId = jogadorId,
                OlheiroId = olheiroId,
                Status = status,
                Texto = texto,
                Nota = nota,
                Parecer = parecer,
                ObservadoEm = observadoEm,
                EscritoEm = escritoEm,
                FinalizadoEm = finalizadoEm,
                CorrigeRelatorioId = corrigeRelatorioId
            };

            relatorio._pontosPositivos.AddRange(pontosPositivos);
            relatorio._pontosNegativos.AddRange(pontosNegativos);

            return relatorio;
        }

        public void AlterarTexto(string texto)
        {
            GarantirEditavel();
            if (string.IsNullOrWhiteSpace(texto))
                throw new ValorInvalidoException("relatorio.texto_obrigatorio", "texto é obrigatório");
            Texto = texto;
        }

        public void AtribuirNota(Nota nota)
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

        public void DefinirParecer(Parecer parecer)
        {
            if (!Enum.IsDefined(parecer))
                throw new ValorInvalidoException(
                    "relatorio.parecer_invalido",
                    "o parecer informado não é um valor válido");
            GarantirEditavel();
            Parecer = parecer;
        }

        public void Finalizar(ISpecification<Relatorio> conteudoMinimo, DateTime finalizadoEm)
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
            EscritoEm = finalizadoEm;
            FinalizadoEm = finalizadoEm;
        }

        private void GarantirEditavel()
        {
            if (Status is StatusRelatorio.Finalizado)
                throw new ConflitoDeDominioException(
                    "relatorio.ja_finalizado",
                    "relatório finalizado é imutável; emita um relatório de correção");
        }

        public void SubstituirPontosPositivos(IEnumerable<string> pontos)
        {
            GarantirEditavel();
            _pontosPositivos.Clear();
            _pontosPositivos.AddRange(pontos);
        }

        public void SubstituirPontosNegativos(IEnumerable<string> pontos)
        {
            GarantirEditavel();
            _pontosNegativos.Clear();
            _pontosNegativos.AddRange(pontos);
        }
    }
}
