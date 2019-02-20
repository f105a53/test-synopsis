using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Benchmark
{
    [ClrJob(baseline: true), CoreJob]
    [RPlotExporter, RankColumn]
    public class FileIndexing
    {
        [Params("data/1.")]
        public string path;

        [Benchmark]
        public List<string> StreamReaderSplit()
        {
            var list = new List<string>();
            using (StreamReader sr = new StreamReader(path))
            {

                while (!sr.EndOfStream)
                {
                    string[] lineTerms = sr.ReadLine().Split(new char[0], StringSplitOptions.RemoveEmptyEntries).Select(s => s.ToLowerInvariant()).ToArray();
                    foreach (string term in lineTerms)
                    {
                        list.Add(term);
                    }
                }
            }
            return list;
        }

        [Benchmark]
        public List<string> StreamReaderSlice()
        {
            var list = new List<string>();
            using (StreamReader sr = new StreamReader(path))
            {

                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().AsSpan();
                    int index = 0;
                    while (index < line.Length)
                    {
                        var newIndex = line.Slice(index).IndexOf('\n');
                        if (newIndex < index) break;
                        var term = line.Slice(index, newIndex - index);
                        list.Add(term.ToString());
                        index = newIndex + 1;
                    }
                }
            }
            return list;
        }
    }
}