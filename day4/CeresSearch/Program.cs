using BenchmarkDotNet.Running;

namespace CeresSearch
{
    class Program
    {
        static void Main()
        {
            //BenchmarkRunner.Run(typeof(Program).Assembly);

            new Runner { PreferCpu = true }.Run();
        }
    }
}
