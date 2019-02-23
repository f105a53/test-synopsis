using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace Benchmark
{
    [CoreJob]
    [RPlotExporter]
    [RankColumn]
    public class FileIndexing
    {
        [Params("data/singlefile", "data/arnold-j")]
        public string path;

        private char[] whitespace;

        private static IEnumerable<FileInfo> Crawl(DirectoryInfo dir)
        {
            foreach (var file in dir.EnumerateFiles()) yield return file;
            foreach (var d in dir.EnumerateDirectories())
            foreach (var file in Crawl(d))
                yield return file;
        }

        [Benchmark]
        public List<string> FileReadAllTextSplit()
        {
            var list = new List<string>();
            foreach (var file in GetFiles())
            foreach (var line in File.ReadAllLines(file.FullName))
            {
                var lineTerms = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.ToLowerInvariant()).ToArray();
                foreach (var term in lineTerms) list.Add(term);
            }

            return list;
        }

        private IEnumerable<FileInfo> GetFiles()
        {
            return Crawl(new DirectoryInfo(path));
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            var a = new List<char>();
            for (int i = char.MinValue; i <= char.MaxValue; i++)
            {
                var c = Convert.ToChar(i);
                if (!char.IsWhiteSpace(c)) a.Add(c);
            }

            whitespace = a.ToArray();
        }

        //[Benchmark]
        //public List<string> StreamReaderSlice3rdParty()
        //{
        //    var w = new Span<char>(whitespace);
        //    var list = new List<string>();
        //    foreach (var file in GetFiles())
        //        using (var sr = new StreamReader(file.FullName))
        //        {
        //            while (!sr.EndOfStream)
        //            {
        //                var line = sr.ReadLine().AsSpan().Split(w);
        //                foreach (var word in line) list.Add(word.ToString());
        //            }
        //        }

        //    return list;
        //}

        [Benchmark]
        public List<string> StreamReaderSliceCustom()
        {
            var w = new Span<char>(whitespace);
            var list = new List<string>();
            foreach (var file in GetFiles())
                using (var sr = new StreamReader(file.FullName))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine().AsSpan();
                        var index = 0;
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
        public List<string> StreamReaderSplit()
        {
            var list = new List<string>();
            foreach (var file in GetFiles())
                using (var sr = new StreamReader(file.FullName))
                {
                    while (!sr.EndOfStream)
                    {
                        var lineTerms = sr.ReadLine().Split(new char[0], StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.ToLowerInvariant()).ToArray();
                        foreach (var term in lineTerms) list.Add(term);
                    }
                }

            return list;
        }
    }
}