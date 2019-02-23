using System;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;


namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<FileIndexing>();
            //new FileIndexing() { path = "data/arnold-j" }.Pipelines().GetAwaiter().GetResult();
        }
    }
}
