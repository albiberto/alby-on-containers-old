using System;

namespace Catalog.Models
{
    public abstract class EntityBase: IEquatable<EntityBase>
    {
        public Guid Id { get; set; }

        public bool Equals(EntityBase? other)
        {
            if (ReferenceEquals(objA: null, other)) return false;
            return ReferenceEquals(this, other) || Id.Equals(other.Id);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(objA: null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((EntityBase) obj);
        }

        public override int GetHashCode() => Id.GetHashCode();

        public static bool operator ==(EntityBase? left, EntityBase? right) => Equals(left, right);

        public static bool operator !=(EntityBase? left, EntityBase? right) => !Equals(left, right);
    }
}