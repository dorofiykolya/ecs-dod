namespace DOD
{
  public class EntitiesSystems
  {
    private int _tick;
    private IEntitiesSystem[] _systems;
    private EntitiesContext _context;
    private SystemContext _systemContext;
    private EntitiesContext.TrimManager _trimManager;

    public EntitiesSystems(EntitiesContext entitiesContext, IEntitiesSystem[] systems)
    {
      _context = entitiesContext;
      _systems = systems;
      _systemContext = new SystemContext(this, entitiesContext);
      _trimManager = (EntitiesContext.TrimManager)entitiesContext.Manager;

      foreach (var system in systems)
      {
        var systemPreinitialize = system as IEntitiesSystemPreinitialize;
        if (systemPreinitialize != null) systemPreinitialize.OnPreinitialize(_systemContext);
      }
      foreach (var system in systems)
      {
        var systemInitialize = system as IEntitiesSystemInitialize;
        if (systemInitialize != null) systemInitialize.OnInitialize(_systemContext);
      }
      foreach (var system in systems)
      {
        var systemPostInitialize = system as IEntitiesSystemPostInitialize;
        if (systemPostInitialize != null) systemPostInitialize.OnPostInitialize(_systemContext);
      }
    }

    public int Ticks { get { return _tick; } }

    public void Tick()
    {
      _trimManager.TrimEntities();

      ++_tick;
      var tick = new EntitiesTick
      {
        Tick = _tick
      };

      foreach (var system in _systems)
      {
        system.OnTick(_systemContext, tick);
      }
    }
  }
}

