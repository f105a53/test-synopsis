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

            //var fileIndexing = new FileIndexing() { path = "data/arnold-j" };
            //fileIndexing.GlobalSetup();
            //var result = fileIndexing.StreamReaderSliceWholeFile();
        }
    }
}
