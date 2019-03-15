using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DOD;

namespace DOD
{
  public class Entity
  {
    private int _index;
    private int _id;
    private bool _disposed;

    public EntityId Id
    {
      get
      {
        return new EntityId
        {
          Id = _id,
          Index = _index
        };
      }
    }

    public bool Disposed { get { return _disposed; } }

    public class Manager
    {
      private const int INITIAL_SIZE = 64;

      private int _count = 0;
      private int _size = 0;
      private int _version = 0;
      private int _id = 0;
      private int _capacity = INITIAL_SIZE;
      private Entity[] _entities = new Entity[INITIAL_SIZE];
      private EntityId[] _entitiesIds = new EntityId[INITIAL_SIZE];
      private Stack<int> _poolIndex = new Stack<int>();
      private Queue<Entity> _poolEntities = new Queue<Entity>();
      private Chain[] _components = new Chain[INITIAL_SIZE];
      private Dictionary<Type, int> _typeIndex = new Dictionary<Type, int>(INITIAL_SIZE);
      private MatcherManager _matcherManager;

      public Manager()
      {
        _matcherManager = new MatcherManager(this);
      }

      public Manager MapComponent<T>() where T : struct, IComponent
      {
        Type type = typeof(T);

        Contract.True(!_typeIndex.ContainsKey(type));

        var typeIndex = _typeIndex.Count;
        _typeIndex[type] = typeIndex;
        if (_components.Length <= typeIndex + 1)
        {
          Array.Resize(ref _components, _components.Length + (_components.Length >> 1));
        }

        _components[typeIndex] = new Chain<T>
        {
          Components = new T[_capacity],
          Contains = new bool[_capacity]
        };

        return this;
      }

      public Entity CreateEntity()
      {
        ++_id;
        ++_version;
        ++_count;
        int index;
        if (_poolIndex.Count != 0)
        {
          index = _poolIndex.Pop();
        }
        else
        {
          index = _size;
          if (_size == _capacity)
          {
            _capacity = _capacity + (_capacity >> 1);
            Array.Resize(ref _entities, _capacity);
            Array.Resize(ref _entitiesIds, _capacity);
            foreach (var chain in _components)
            {
              if(chain == null) break;
              chain.Resize(_capacity);
            }
          }
          ++_size;
        }
        var entity = _poolEntities.Count != 0 ? _poolEntities.Dequeue() : new Entity();
        entity._id = _id;
        entity._index = index;
        entity._disposed = false;
        _entities[index] = entity;
        _entitiesIds[index] = entity.Id;
        return entity;
      }

      public Entity GetEntity(EntityId id)
      {
        Contract.True(id.Index > -1);
        Contract.NotDisposed(id.Index, _entities);

        if (id.Index >= _capacity) return null;
        var entity = _entities[id.Index];
        Contract.True(!entity._disposed);
        return entity;
      }

      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      private bool SetComponentInternal<T>(EntityId entityId, T value, bool replaceValue) where T : struct, IComponent
      {
        int index = entityId.Index;

        Contract.NotDisposed(index, _entities);
        Contract.HasComponentType(typeof(T), _typeIndex);

        int typeIndex = _typeIndex[typeof(T)];

        var chain = (Chain<T>)_components[typeIndex];
        if (!replaceValue && chain.Contains[index])
        {
          return false;
        }
        chain.Contains[index] = true;
        if (replaceValue)
        {
          chain.Components[index] = value;
        }
        return true;
      }

      public void SetComponent<T>(EntityId entity, T value) where T : struct, IComponent
      {
        SetComponentInternal(entity, value, true);
      }

      public bool AddComponent<T>(EntityId entity) where T : struct, IComponent
      {
        return SetComponentInternal<T>(entity, default(T), false);
      }

      public bool RemoveComponent<T>(EntityId entityId) where T : struct, IComponent
      {
        var index = entityId.Index;
        Contract.NotDisposed(index, _entities);
        Contract.HasComponentType(typeof(T), _typeIndex);

        int typeIndex;
        if (_typeIndex.TryGetValue(typeof(T), out typeIndex))
        {
          var chain = (Chain<T>)_components[typeIndex];
          var result = chain.Contains[index];
          if (result)
          {
            chain.Components[index] = default(T);
            chain.Contains[index] = false;
            return true;
          }
        }
        return false;
      }

      public bool HasComponent<T>(EntityId entityId) where T : struct, IComponent
      {
        var index = entityId.Index;
        Contract.NotDisposed(index, _entities);
        Contract.HasComponentType(typeof(T), _typeIndex);

        int typeIndex;
        if (_typeIndex.TryGetValue(typeof(T), out typeIndex))
        {
          return ((Chain<T>)_components[typeIndex]).Contains[index];
        }
        return false;
      }

      public bool GetComponent<T>(EntityId entityId, out T component) where T : struct, IComponent
      {
        Contract.NotDisposed(entityId.Index, _entities);
        Contract.HasComponentType(typeof(T), _typeIndex);

        int typeIndex = _typeIndex[typeof(T)];
        var index = entityId.Index;
        var chain = (Chain<T>)_components[typeIndex];
        if (chain.Contains[index])
        {
          component = chain.Components[index];
          return true;
        }

        component = default(T);
        return false;
      }

      public bool IsDisposed(EntityId entityId)
      {
        var entity = _entities[entityId.Index];
        return entity == null || entity._id != entityId.Id;
      }

      public void Dispose(EntityId entityId)
      {
        var index = entityId.Index;
        Contract.NotDisposed(index, _entities);

        ++_version;
        --_count;

        var entity = _entities[index];
        _poolIndex.Push(index);
        _poolEntities.Enqueue(entity);
        _entities[index] = null;
        _entitiesIds[index] = EntityId.Invalid;
        entity._disposed = true;
        foreach (var chain in _components)
        {
          if (chain == null) break;
          chain.Reset(index);
        }
      }

      protected void Trim()
      {
        var index = 0;
        for (int i = 0; i < _size; i++)
        {
          if (_entities[i] != null)
          {
            if (index != i)
            {
              _entities[index] = _entities[i];
              _entities[i] = null;
              _entitiesIds[index] = _entitiesIds[i];
              _entitiesIds[i] = EntityId.Invalid;
            }
            ++index;
          }
        }
      }

      public int Count
      {
        get { return _count; }
      }

      public Iterator GetEntities()
      {
        return new Iterator(this);
      }

      public class MatcherManager
      {
        public struct FieldDescriptor
        {
          public int ComponentIndex;
          public FieldInfo FieldInfo;
        }

        public class TypeDescriptor
        {
          public FieldDescriptor[] Descriptors;
          public int[] IncludeTypes;
          public int[] ExcludeTypes;
        }

        private Dictionary<Type, TypeDescriptor> _cache;
        private Manager _manager;

        public MatcherManager(Manager manager)
        {
          _manager = manager;
          _cache = new Dictionary<Type, TypeDescriptor>();
        }

        protected Matcher<T> CreateMatcher<T>() where T : class, new()
        {
          TypeDescriptor typeDescriptor;
          if (!_cache.TryGetValue(typeof(T), out typeDescriptor))
          {
            var descriptorList = new List<FieldDescriptor>();
            var includeList = new HashSet<int>();
            var fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetField |
                                             BindingFlags.SetField);
            foreach (var field in fields)
            {
              var fieldType = field.FieldType;
              if (fieldType.IsArray)
              {
                var componentType = fieldType.GetElementType();
                if (typeof(IComponent).IsAssignableFrom(componentType))
                {
                  Contract.HasComponentType(componentType, _manager._typeIndex);
                  var componentIndex = _manager._typeIndex[componentType];
                  includeList.Add(componentIndex);
                  var descriptor = new FieldDescriptor
                  {
                    FieldInfo = field,
                    ComponentIndex = componentIndex
                  };
                  descriptorList.Add(descriptor);
                }
              }
            }

            var exludeList = new HashSet<int>();
            var attributes = typeof(T).GetCustomAttributes(false);
            if (attributes.Length != 0)
            {
              foreach (var attribute in attributes)
              {
                var excludeAttribute = attribute as ExcludeAttribute;
                if (excludeAttribute != null)
                {
                  exludeList.Add(_manager._typeIndex[excludeAttribute.Type]);
                }
              }
            }

            _cache[typeof(T)] = typeDescriptor = new TypeDescriptor
            {
              Descriptors = descriptorList.ToArray(),
              IncludeTypes = includeList.ToArray(),
              ExcludeTypes = exludeList.ToArray()
            };
          }
          
          var filter = new T();
          var matcher = new Matcher<T>(filter, typeDescriptor, new EntityFilter
          {
            Manager = _manager
          });
          return matcher;
        }

        public struct EntityFilter
        {
          public Entity.Manager Manager;

          public Chain[] GetComponents()
          {
            return Manager._components;
          }

          public EntityId[] GetEntitiesId()
          {
            return Manager._entitiesIds;
          }

          public Entity[] GetEntities()
          {
            return Manager._entities;
          }
        }
      }

      public abstract class Chain
      {
        public bool[] Contains;

        public abstract void Resize(int size);
        public abstract void Reset(int index);
        public abstract Array GetComponents();
      }

      public class Chain<T> : Chain where T : struct, IComponent
      {
        public T[] Components;

        public override Array GetComponents()
        {
          return Components;
        }

        public override void Reset(int index)
        {
          if (Contains[index])
          {
            Contains[index] = false;
            Components[index] = default(T);
          }
        }

        public override void Resize(int size)
        {
          if (Components == null)
          {
            Components = new T[size];
            Contains = new bool[size];
          }
          else
          {
            Array.Resize(ref Components, size);
            Array.Resize(ref Contains, size);
          }
        }
      }

      public struct Iterator : IEnumerator<EntityId>
      {
        private Manager _manager;
        private int _index;
        private int _version;
        private int _count;

        public Iterator(Manager manager)
        {
          _index = -1;
          _manager = manager;
          _version = manager._version;
          _count = manager._size;
        }

        public int Count => _count;

        public EntityId Current => _manager._entitiesIds[_index];

        object IEnumerator.Current => _manager._entitiesIds[_index];

        public void Dispose()
        {
          _manager = null;
        }

        public bool MoveNext()
        {
          do
          {
            Contract.True(_version == _manager._version);
            ++_index;
            if (_index >= _count) return false;
          } while (_manager._entitiesIds[_index].Index != _index);

          return true;
        }

        public void Reset()
        {
          _index = -1;
        }
      }
    }
  }
}
