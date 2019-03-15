using System;
using System.Collections;
using System.Collections.Generic;

namespace OOP
{
  public class SystemsPool
  {
    private Dictionary<Type, Stack<IList>> _listPool = new Dictionary<Type, Stack<IList>>();

    public List<T> ListPop<T>()
    {
      Stack<IList> stack;
      if (_listPool.TryGetValue(typeof(T), out stack) && stack.Count != 0)
      {
        return (List<T>)stack.Pop();
      }
      return new List<T>();
    }

    public void ListPush<T>(List<T> list)
    {
      list.Clear();
      Stack<IList> stack;
      if (!_listPool.TryGetValue(typeof(T), out stack))
      {
        _listPool[typeof(T)] = stack = new Stack<IList>();
        stack.Push(list);
      }
      else
      {
        Contract.True(!stack.Contains(list));

        stack.Push(list);
      }
    }
  }
}
