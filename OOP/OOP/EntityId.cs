using System;

namespace OOP
{
  public struct EntityId : IEquatable<EntityId>
  {
    public static bool operator ==(EntityId left, EntityId right)
    {
      return left.Index == right.Index && left.Id == right.Id;
    }

    public static bool operator !=(EntityId left, EntityId right)
    {
      return left.Index != right.Index || left.Id != right.Id;
    }

    public int Index;
    public int Id;

    public static readonly EntityId Invalid = new EntityId
    {
      Index = -1,
      Id = -1
    };

    public bool Equals(EntityId other)
    {
      return Index == other.Index && Id == other.Id;
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      return obj is EntityId other && Equals(other);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return (Index * 397) ^ Id;
      }
    }
  }
}
