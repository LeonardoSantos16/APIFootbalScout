namespace APIFootballScout.Domain.Base
{
    public abstract class EntityBase<TId>
    {
        public TId Id { get; protected set; }

        protected EntityBase(TId id)
        {
            Id = id;
        }

        public override bool Equals(object? obj)
            => obj is EntityBase<TId> other
            && other.GetType() == GetType()
            && EqualityComparer<TId>.Default.Equals(Id, other.Id);

        public override int GetHashCode() => HashCode.Combine(GetType(), Id);

        public static bool operator ==(EntityBase<TId>? left, EntityBase<TId>? right)
            => Equals(left, right);

        public static bool operator !=(EntityBase<TId>? left, EntityBase<TId>? right)
            => !Equals(left, right);
    }
}
