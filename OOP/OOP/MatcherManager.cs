using System;
namespace OOP
{
  public class MatcherManager : Entity.Manager.MatcherManager
  {
    public MatcherManager(Entity.Manager manager) : base(manager)
    {
    }

    new public Matcher<T> CreateMatcher<T>() where T : class, new()
    {
      return base.CreateMatcher<T>();
    }
  }
}
