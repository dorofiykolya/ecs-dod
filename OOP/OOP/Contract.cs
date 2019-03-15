using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OOP
{
  public class Contract
  {
    [Conditional("DEBUG")]
    public static void True(bool condition, string message = null)
    {
      if (!condition) throw new ArgumentException(message ?? "");
    }

    [Conditional("DEBUG")]
    public static void NotDisposed(int index, Entity[] entities)
    {
      True(!entities[index].Disposed);
    }

    [Conditional("DEBUG")]
    public static void HasComponentType(Type componentType, Dictionary<Type, int> map)
    {
      True(map.ContainsKey(componentType));
    }
  }
}
