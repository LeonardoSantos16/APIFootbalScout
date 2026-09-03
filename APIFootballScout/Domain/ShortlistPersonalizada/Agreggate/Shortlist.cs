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
        public IReadOnlyList<Alvo> Alvos => _alvos;

        private Shortlist(Guid id, Guid olheiroId, string nome) : base(id)
        {
            OlheiroId = olheiroId;
            Nome = nome;
        }

        public static Shortlist Criar(Guid olheiroId, string nome)
            => new(Guid.NewGuid(), olheiroId, nome);

        public static Shortlist Restaurar(Guid id, Guid olheiroId, string nome, IEnumerable<Alvo> alvos)
        {
            var shortlist = new Shortlist(id, olheiroId, nome);
            shortlist._alvos.AddRange(alvos);

            return shortlist;
        }

        public void AdicionarAlvo(int jogadorId, Prioridade prioridade, Dinheiro custoEstimado, ISpecification<Shortlist> comVaga)
        {
            VerificarDuplicidade(jogadorId);

            if (!comVaga.IsSatisfiedBy(this))
            {
                throw new RegraDeNegocioException("shortlist.limite_de_alvos_atingido", "Não há vagas disponíveis na shortlist.");
            }


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

            // Mover nao cria posicao nova: as posicoes validas vao de 1 a n, nao a n+1.
            // A checagem vem antes da remocao para que a recusa deixe a ordem intacta.
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

        private void VerificarDuplicidade(int jogadorId)
        {
            if (_alvos.Any(a => a.JogadorId == jogadorId))
                throw new RegraDeNegocioException("shortlist.jogador_ja_na_lista", "O jogador já está presente na shortlist.");

        }
    }
}
