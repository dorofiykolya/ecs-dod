using System.Collections.Generic;

namespace OOP
{
  public class EntityManager : Entity.Manager
  {
    private EntitiesContext _entitiesContext;

    public EntityManager(EntitiesContext entitiesContext)
    {
      _entitiesContext = entitiesContext;
    }
  }
}
