namespace DOD
{
  public class EntitiesContext
  {
    protected EntitiesContext()
    {
      Manager = new TrimManager(this);
      Matcher = new MatcherManager(Manager);
    }
    public MatcherManager Matcher { get; private set; }
    public EntityManager Manager { get; private set; }

    public static Initializer CreateContext()
    {
      return new Initializer();
    }

    public class Initializer : EntitiesContext
    {
      public Initializer()
      {

      }

      public Initializer MapComponent<T>() where T : struct, IComponent
      {
        Manager.MapComponent<T>();
        return this;
      }
    }

    public class TrimManager : EntityManager
    {
      public TrimManager(EntitiesContext entitiesContext) : base(entitiesContext)
      {
      }

      public void TrimEntities()
      {
        Trim();
      }
    }
  }
}
