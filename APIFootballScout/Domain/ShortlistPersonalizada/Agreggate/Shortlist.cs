using APIFootballScout.Domain.Base;
using APIFootballScout.Domain.SharedKernel;

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

        public void AdicionarAlvo(int jogadorId, Dinheiro custoEstimado, ISpecification<Shortlist> comVaga)
            => throw new NotImplementedException();
    }
}
