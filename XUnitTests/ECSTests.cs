using System;
using System.Collections.Generic;
using DOD;
using Xunit;

namespace XUnitTests
{
  public class ECSTest
  {
    [Exclude(typeof(EComponent2))]
    private class MatchFilter
    {
      public EComponent[] EComponents;
    }

    private class ESystem : IEntitiesSystem, IEntitiesSystemInitialize, IEntitiesSystemPreinitialize, IEntitiesSystemPostInitialize
    {
      private enum Status
      {
        None,
        Pre,
        Init,
        Post
      }

      private Status State;
      private Matcher<MatchFilter> _init_matcher;
      private Matcher<MatchFilter> _pre_matcher;
      private Matcher<MatchFilter> _post_matcher;

      public void OnTick(SystemContext context, EntitiesTick tick)
      {
        var e = context.EntityManager.GetEntities();
        while (e.MoveNext())
        {
          var id = e.Current;
          EComponent component;
          if (context.EntityManager.GetComponent<EComponent>(id, out component))
          {
            component.X += 2;
            component.Y += 3;
            context.EntityManager.SetComponent<EComponent>(id, component);
          }
        }

        var entities = context.Pool.ListPop<EntityId>();
        var entities_pre = context.Pool.ListPop<EntityId>();
        var entities_init = context.Pool.ListPop<EntityId>();
        var entities_post = context.Pool.ListPop<EntityId>();

        var matcher = context.Matcher.CreateMatcher<MatchFilter>();
        
        matcher.GetEntities(entities);

        _pre_matcher.GetEntities(entities_pre);
        _init_matcher.GetEntities(entities_init);
        _post_matcher.GetEntities(entities_post);

        var ents = new[] { entities, entities_pre, entities_init, entities_post };

        foreach (var ent in ents)
        {
          foreach (var ent1 in ents)
          {
            Assert.True(ent.Count == ent1.Count);
          }
        }

        foreach (var ent in ents)
        {
          foreach (var ent1 in ents)
          {
            for (int i = 0; i < ent.Count; i++)
            {
              Assert.True(ent[i] == ent1[i]);
            }
          }
        }

        foreach (var entityId in entities)
        {
          
          var component = matcher.Filter.EComponents[entityId.Index];
          if (!component.NoFirst)
          {
            Assert.True(component.Ticks == tick.Tick - 1);

            component.Ticks = tick.Tick;
          }
          component.NoFirst = true;
          matcher.Filter.EComponents[entityId.Index] = component;
        }

        context.Pool.ListPush(entities);
        context.Pool.ListPush(entities_pre);
        context.Pool.ListPush(entities_init);
        context.Pool.ListPush(entities_post);
      }

      public void OnInitialize(SystemContext context)
      {
        Assert.True(State == Status.Pre);
        State = Status.Init;

        _init_matcher = context.Matcher.CreateMatcher<MatchFilter>();
      }

      public void OnPreinitialize(SystemContext context)
      {
        Assert.True(State == Status.None);
        State = Status.Pre;

        _pre_matcher = context.Matcher.CreateMatcher<MatchFilter>();
      }

      public void OnPostInitialize(SystemContext context)
      {
        Assert.True(State == Status.Init);
        State = Status.Post;

        _post_matcher = context.Matcher.CreateMatcher<MatchFilter>();
      }
    }

    private struct EComponent2 : IComponent
    {

    }

    private struct EComponent : IComponent
    {
      public int X;
      public int Y;
      public int Ticks;
      public bool NoFirst;
    }

    [Fact]
    public void TestSystem()
    {
      var context = EntitiesContext.CreateContext();

      context.MapComponent<EComponent>().MapComponent<EComponent2>();

      var systems = new EntitiesSystems(context, new IEntitiesSystem[] { new ESystem() });

      var manager = context.Manager;

      var entity = manager.CreateEntity();
      manager.AddComponent<EComponent>(entity.Id);

      for (int i = 0; i < 10; i++)
      {
        systems.Tick();

        manager.SetComponent<EComponent>(entity.Id, new EComponent
        {
          X = 1,
          Y = 2
        });

        Assert.True(manager.GetEntities().Count == 1);
        Assert.NotNull(manager.GetEntity(entity.Id));

        EComponent resultComponent;
        Assert.True(manager.GetComponent<EComponent>(entity.Id, out resultComponent));

        Assert.True(manager.GetEntity(entity.Id).Id.Id == 1 + i);

        manager.Dispose(manager.GetEntity(entity.Id).Id);

        manager.CreateEntity();

        Assert.True(manager.GetEntity(entity.Id).Id.Id != 1);
      }
    }
  }
}
