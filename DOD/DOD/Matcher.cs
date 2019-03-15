using System.Collections.Generic;

namespace DOD
{
  public class Matcher<T> where T : class, new()
  {
    private T _filter;
    private Entity.Manager.MatcherManager.TypeDescriptor _typeDescriptor;
    private Entity.Manager.MatcherManager.EntityFilter _entityFilter;

    public T Filter
    {
      get { return _filter; }
    }

    public Matcher(T filter, Entity.Manager.MatcherManager.TypeDescriptor typeDescriptor, Entity.Manager.MatcherManager.EntityFilter entityFilter)
    {
      _filter = filter;
      _typeDescriptor = typeDescriptor;
      _entityFilter = entityFilter;
    }

    public void GetEntities(List<EntityId> result)
    {
      var entitiesItems = _entityFilter.Manager.Count;
      var components = _entityFilter.GetComponents();
      var entities = _entityFilter.GetEntities();
      var entitiesId = _entityFilter.GetEntitiesId();

      var tdLength = _typeDescriptor.Descriptors.Length;
      var descriptors = _typeDescriptor.Descriptors;
      for (var ti = 0; ti < tdLength; ti++)
      {
        var descriptor = descriptors[ti];
        var array = components[descriptor.ComponentIndex].GetComponents();
        descriptor.FieldInfo.SetValue(_filter, array);
      }

      var tiLength = _typeDescriptor.IncludeTypes.Length;
      var teLength = _typeDescriptor.ExcludeTypes.Length;
      var includes = _typeDescriptor.IncludeTypes;
      var excludes = _typeDescriptor.ExcludeTypes;

      var entityIterator = 0;
      var entitiesLength = entities.Length;

      for (int i = 0; i < entitiesLength && entityIterator < entitiesItems; ++i)
      {
        if(entities[i] == null) continue;
        ++entityIterator;
        var entity = entitiesId[i];
        var entityIndex = entity.Index;
        var ok = true;
        
        for (var ii = 0; ii < tiLength; ++ii)
        {
          var includeTypeIndex = includes[ii];
          if (!components[includeTypeIndex].Contains[entityIndex])
          {
            ok = false;
            break;
          }
        }

        if (ok)
        {
          for (var ie = 0; ie < teLength; ++ie)
          {
            var excludeTypeIndex = excludes[ie];
            if (components[excludeTypeIndex].Contains[entityIndex])
            {
              ok = false;
              break;
            }
          }

          if (ok)
          {
            result.Add(entity);
          }
        }
      }
    }
  }
}
