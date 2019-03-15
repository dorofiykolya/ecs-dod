using System;
using System.Collections.Generic;
using System.Text;
using DOD;

namespace Benchmark
{
  class ECSDODBenchmark
  {
    struct A : IComponent { }
    struct B : IComponent
    {
      public int a;
    }

    struct C : IComponent
    {
      public float b;
      public int c;
    }

    struct D : IComponent
    {
      public long d;
    }

    struct E : IComponent
    {
      public bool e;
    }

    class SystemA : IEntitiesSystem
    {
      class Filter
      {
        public A[] Components;
      }

      public void OnTick(SystemContext context, EntitiesTick tick)
      {
        var filter = context.Matcher.CreateMatcher<Filter>();
        var aList = context.Pool.ListPop<EntityId>();
        filter.GetEntities(aList);

        int a = 0;
        foreach (var entityId in aList)
        {
          a += entityId.Id;
        }

        int b = a;

        context.Pool.ListPush(aList);
      }
    }

    class SystemB : IEntitiesSystem, IEntitiesSystemInitialize
    {
      class Filter
      {
        public A[] ComponentsA;
        public B[] ComponentsB;
      }

      private Matcher<Filter> match;

      public void OnTick(SystemContext context, EntitiesTick tick)
      {
        

        var aList = context.Pool.ListPop<EntityId>();
        match.GetEntities(aList);
        var filter = match.Filter;
        foreach (var entityId in aList)
        {
          var component = filter.ComponentsB[entityId.Index];
          component.a = entityId.Id;
          filter.ComponentsB[entityId.Index] = component;
        }

        context.Pool.ListPush(aList);
      }

      public void OnInitialize(SystemContext context)
      {
        match = context.Matcher.CreateMatcher<Filter>();
      }
    }

    class SystemC : IEntitiesSystem, IEntitiesSystemPreinitialize
    {
      [Exclude(typeof(A))]
      class Filter
      {
        public B[] ComponentsB;
        public C[] ComponentsC;
      }

      Matcher<Filter> match;

      public void OnTick(SystemContext context, EntitiesTick tick)
      {
        

        var aList = context.Pool.ListPop<EntityId>();
        match.GetEntities(aList);
        var filter = match.Filter;
        foreach (var entityId in aList)
        {
          var componentB = filter.ComponentsB[entityId.Index];
          var componentC = filter.ComponentsC[entityId.Index];
          componentC.b = componentB.a / 10f + componentC.b;
          componentC.c = entityId.Index + entityId.Id;
          filter.ComponentsB[entityId.Index] = componentB;
          filter.ComponentsC[entityId.Index] = componentC;
        }

        context.Pool.ListPush(aList);
      }

      public void OnPreinitialize(SystemContext context)
      {
        match = context.Matcher.CreateMatcher<Filter>();
      }
    }

    class SystemD : IEntitiesSystem, IEntitiesSystemPostInitialize
    {
      class Filter
      {
        public A[] ComponentsA;
        public B[] ComponentsB;
        public C[] ComponentsC;
        public D[] ComponentsD;
      }

      Matcher<Filter> match;

      public void OnTick(SystemContext context, EntitiesTick tick)
      {
        

        var aList = context.Pool.ListPop<EntityId>();
        match.GetEntities(aList);
        var filter = match.Filter;
        foreach (var entityId in aList)
        {
          var componentB =filter.ComponentsB[entityId.Index];
          var componentC =filter.ComponentsC[entityId.Index];
          var componentD = filter.ComponentsD[entityId.Index];
          componentC.b = componentC.b + entityId.Index;
          componentC.c = (int) (entityId.Index + componentC.b);
          componentD.d = componentD.d + componentC.c;
          filter.ComponentsB[entityId.Index] = componentB;
          filter.ComponentsC[entityId.Index] = componentC;
          filter.ComponentsD[entityId.Index] = componentD;
        }

        context.Pool.ListPush(aList);
      }

      public void OnPostInitialize(SystemContext context)
      {
        match = context.Matcher.CreateMatcher<Filter>();
      }
    }

    class SystemE : IEntitiesSystem, IEntitiesSystemPostInitialize
    {
      [Exclude(typeof(A))]
      class Filter
      {
        public B[] ComponentsB;
        public C[] ComponentsC;
        public D[] ComponentsD;
        public E[] ComponentsE;
      }

      Matcher<Filter> match;

      public void OnTick(SystemContext context, EntitiesTick tick)
      {
        

        var aList = context.Pool.ListPop<EntityId>();
        match.GetEntities(aList);
        var filter = match.Filter;
        foreach (var entityId in aList)
        {
          var componentB = filter.ComponentsB[entityId.Index];
          var componentC = filter.ComponentsC[entityId.Index];
          var componentD = filter.ComponentsD[entityId.Index];
          var componentE = filter.ComponentsE[entityId.Index];
          componentC.b = componentC.b + entityId.Index;
          componentC.c = (int)(entityId.Index + componentC.b);
          componentD.d = componentD.d + componentC.c;
          componentE.e = componentB.a % componentD.d == 0;
          filter.ComponentsB[entityId.Index] = componentB;
          filter.ComponentsC[entityId.Index] = componentC;
          filter.ComponentsD[entityId.Index] = componentD;
          filter.ComponentsE[entityId.Index] = componentE;
        }

        context.Pool.ListPush(aList);
      }

      public void OnPostInitialize(SystemContext context)
      {
        match = context.Matcher.CreateMatcher<Filter>();
      }
    }

    public static void Execute()
    {
      var context = EntitiesContext.CreateContext();
      context.MapComponent<A>().MapComponent<B>().MapComponent<C>().MapComponent<D>().MapComponent<E>();

      var systems = new EntitiesSystems(context, GetSystems());

      for (int i = 0; i < 1000; i++)
      {
        var entity = context.Manager.CreateEntity();

        if(i % 2 == 0) context.Manager.AddComponent<A>(entity.Id);
        context.Manager.AddComponent<B>(entity.Id);
        context.Manager.AddComponent<C>(entity.Id);
        if(i % 3 == 0) context.Manager.AddComponent<D>(entity.Id);
        context.Manager.AddComponent<E>(entity.Id);
      }

      for (int i = 0; i < 5000; i++)
      {
        systems.Tick();
      }
    }

    static IEntitiesSystem[] GetSystems()
    {
      var systems = new IEntitiesSystem[]
      {
        new SystemA(),
        new SystemB(),
        new SystemC(),
        new SystemD(),
        new SystemE(),
      };

      var sysCount = systems.Length;
      var count = 100;
      var index = 0;
      IEntitiesSystem[] result = new IEntitiesSystem[count];
      for (int i = 0; i < count / sysCount; i++)
      {
        foreach (var system in systems)
        {
          result[index] = system;
          ++index;
        }
      }

      return result;
    }
  }
}
