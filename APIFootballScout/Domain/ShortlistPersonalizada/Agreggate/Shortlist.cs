using APIFootballScout.Domain.Base;
using APIFootballScout.Domain.Base.Exceptions;
using APIFootballScout.Domain.SharedKernel;
using APIFootballScout.Domain.ShortlistPersonalizada.ValueObject;

namespace APIFootballScout.Domain.ShortlistPersonalizada.Agreggate
{
    public sealed class Shortlist : AggregateRootBase<Guid>
    {
        private readonly List<Alvo> _alvos = [];

        public Guid OlheiroId { get; private set; }
        public string Nome { get; private set; }
        public LimiteDeAlvos Limite { get; private set; }
        public IReadOnlyList<Alvo> Alvos => _alvos;

        public Dinheiro? CustoTotal => _alvos.Count == 0 ? null : _alvos.Select(a => a.CustoEstimado).Aggregate((a, b) => a.Somar(b));

        private Shortlist(Guid id, Guid olheiroId, string nome, LimiteDeAlvos limite) : base(id)
        {
            OlheiroId = olheiroId;
            Nome = nome;
            Limite = limite;
        }

        public static Shortlist Criar(Guid olheiroId, string nome, LimiteDeAlvos limite)
            => new(Guid.NewGuid(), olheiroId, nome, limite);

        public static Shortlist Restaurar(
            Guid id, Guid olheiroId, string nome, LimiteDeAlvos limite, IEnumerable<Alvo> alvos)
        {
            var shortlist = new Shortlist(id, olheiroId, nome, limite);
            shortlist._alvos.AddRange(alvos);

            return shortlist;
        }

        public void AdicionarAlvo(int jogadorId, Prioridade prioridade, Dinheiro custoEstimado)
        {
            VerificarDuplicidade(jogadorId);
            VerificarMoeda(custoEstimado);
            VerificarVaga();

            InsercaoAlvos(new Alvo(jogadorId, prioridade, custoEstimado));
        }

        public void RemoverAlvo(int jogadorId)
        {
            _alvos.RemoveAt(IndiceDe(jogadorId));
            RenumerarPrioridades();
        }

        private void RenumerarPrioridades()
        {
            for (int i = 0; i < _alvos.Count; i++)
            {
                var alvo = _alvos[i];
                _alvos[i] = new Alvo(alvo.JogadorId, new Prioridade(i + 1), alvo.CustoEstimado);
            }
        }

        private int IndiceDe(int jogadorId)
        {
            var indice = _alvos.FindIndex(a => a.JogadorId == jogadorId);
            if (indice < 0)
                throw new RecursoNaoEncontradoException(
                    "shortlist.alvo_nao_encontrado",
                    "O jogador nao esta na shortlist.");

            return indice;
        }

        public void AtualizarPrioridade(int jogadorId, Prioridade novaPrioridade)
        {
            var origem = IndiceDe(jogadorId);
            if (novaPrioridade.Valor > _alvos.Count)
                throw new RegraDeNegocioException(
                    "shortlist.prioridade_fora_da_ordem",
                    "A prioridade informada esta fora da ordem da shortlist.");

            var alvo = _alvos[origem];
            _alvos.RemoveAt(origem);

            InsercaoAlvos(new Alvo(jogadorId, novaPrioridade, alvo.CustoEstimado));
        }

        private void InsercaoAlvos(Alvo alvo)
        {
            ValidacaoDePrioridade(alvo.Prioridade);
            var destino = alvo.Prioridade.Valor - 1;

            _alvos.Insert(destino, alvo);
            RenumerarPrioridades();
        }

        private void ValidacaoDePrioridade(Prioridade prioridade)
        {
            if (prioridade.Valor > _alvos.Count + 1)
                throw new RegraDeNegocioException(
                    "shortlist.prioridade_fora_da_ordem",
                    "A prioridade informada esta fora da ordem da shortlist.");
        }

        private void VerificarVaga()
        {
            if (!Limite.Comporta(_alvos.Count))
                throw new RegraDeNegocioException(
                    "shortlist.limite_de_alvos_atingido",
                    "Não há vagas disponíveis na shortlist.");
        }

        private void VerificarMoeda(Dinheiro custoEstimado)
        {
            var moedaDaLista = _alvos.FirstOrDefault()?.CustoEstimado.Moeda;

            if (moedaDaLista is not null && moedaDaLista != custoEstimado.Moeda)
                throw new RegraDeNegocioException(
                    "shortlist.moeda_divergente",
                    $"A shortlist esta em {moedaDaLista} e nao aceita custo em {custoEstimado.Moeda}.");
        }

        private void VerificarDuplicidade(int jogadorId)
        {
            if (_alvos.Any(a => a.JogadorId == jogadorId))
                throw new RegraDeNegocioException("shortlist.jogador_ja_na_lista", "O jogador já está presente na shortlist.");

        }
    }
}
