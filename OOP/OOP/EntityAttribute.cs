using System;

namespace OOP
{
  public class ExcludeAttribute : Attribute
  {
    public readonly Type Type;

    public ExcludeAttribute(Type type)
    {
      Type = type;
    }
  }
}
