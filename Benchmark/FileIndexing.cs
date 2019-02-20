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

        private char[] whitespace;

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

        [GlobalSetup]
        public void GlobalSetup()
        {
            var a = new List<char>();
            for (int i = char.MinValue; i <= char.MaxValue; i++)
            {
                char c = Convert.ToChar(i);
                if (!char.IsWhiteSpace(c))
                {
                    a.Add(c);
                }
            }
            whitespace = a.ToArray();
        }

        [Benchmark]
        public List<string> StreamReaderSliceCustom()
        {
            var w = new Span<char>(whitespace);
            var list = new List<string>();
            using (StreamReader sr = new StreamReader(path))
            {

                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().AsSpan();
                    int index = 0;
                    while (index < line.Length)
                    {
                        var newIndex = line.Slice(index).IndexOf(w);
                        if (newIndex < index) break;
                        var term = line.Slice(index, newIndex - index);
                        list.Add(term.ToString());
                        index = newIndex + 1;
                    }
                }
            }
            return list;
        }

        [Benchmark]
        public List<string> StreamReaderSlice3rdParty()
        {
            var w = new Span<char>(whitespace);
            var list = new List<string>();
            using (StreamReader sr = new StreamReader(path))
            {

                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().AsSpan().Split(w);
                    foreach (var word in line)
                    {
                        list.Add(word.ToString());
                    }
                }
            }
            return list;
        }
    }
}