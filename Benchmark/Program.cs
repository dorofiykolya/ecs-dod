using System;

namespace Benchmark
{
  class Program
  {
    static void Main(string[] args)
    {

      Clock.BenchmarkTime(ECSDODBenchmark.Execute, s => Console.WriteLine("DOD: " + s), 1);
      Clock.BenchmarkTime(ECSOOPBenchmark.Execute, s => Console.WriteLine("OOP: " + s), 1);

      
      

      Clock.BenchmarkTime(ECSDODBenchmark.Execute, s => Console.WriteLine("DOD: " + s), 1);
      Clock.BenchmarkTime(ECSOOPBenchmark.Execute, s => Console.WriteLine("OOP: " + s), 1);

      Console.ReadKey();
      Console.ReadKey();
      Console.ReadKey();
    }
  }
}
