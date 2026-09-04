namespace APIFootballScout.Domain.Base
{
    public abstract class AggregateRootBase<TId> : EntityBase<TId>, IAggregateRoot
    {
        protected AggregateRootBase(TId id) : base(id)
        {
        }
    }
}
