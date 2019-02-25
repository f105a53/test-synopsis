using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;

namespace Benchmark
{
    internal class Program
    {
        private static void Main()
        {
            BenchmarkRunner.Run<FileIndexing>();
        }
    }
}