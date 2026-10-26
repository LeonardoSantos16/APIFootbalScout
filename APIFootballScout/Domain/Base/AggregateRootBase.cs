namespace APIFootballScout.Domain.Base
{
    public abstract class AggregateRootBase<TId> : EntityBase<TId>, IAggregateRoot
    {
        public long Version { get; private set; }

        protected AggregateRootBase(TId id) : base(id)
        {
        }

        internal void AtualizarVersao(long version) => Version = version;
    }
}