using System;
namespace DOD
{
  public class SystemContext
  {
    public SystemContext(EntitiesSystems systems, EntitiesContext entitiesContext)
    {
      Systems = systems;
      Context = entitiesContext;
      Pool = new SystemsPool();
    }

    public SystemsPool Pool { get; }
    public EntitiesSystems Systems { get; }
    public EntitiesContext Context { get; }
    public MatcherManager Matcher { get { return Context.Matcher; } }
    public EntityManager EntityManager { get { return Context.Manager; } }
  }
}
