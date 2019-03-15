using System;

namespace DOD
{
  public interface IEntitiesSystem
  {
    void OnTick(SystemContext context, EntitiesTick tick);
  }

  public interface IEntitiesSystemPreinitialize
  {
    void OnPreinitialize(SystemContext context);
  }

  public interface IEntitiesSystemInitialize
  {
    void OnInitialize(SystemContext context);
  }

  public interface IEntitiesSystemPostInitialize
  {
    void OnPostInitialize(SystemContext context);
  }
}
