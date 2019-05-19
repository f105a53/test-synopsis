using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Index = Common.Index;

namespace Benchmark
{
    [CoreJob]
    [RPlotExporter]
    [RankColumn]
    [MemoryDiagnoser]
    //[ReturnValueValidator]
    public class FileIndexing
    {
        private readonly IEnumerable<FileInfo> _files = Crawl(new DirectoryInfo("data/arnold-j"));

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
            foreach (var file in _files)
            foreach (var line in File.ReadAllLines(file.FullName))
            {
                var lineTerms = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                list.AddRange(lineTerms);
            }

            return list;
        }

        [Benchmark]
        public async Task LuceneIndex()
        {
            await new Index(Path.GetTempFileName() + ".d/").Build("data/arnold-j", 10000, new Progress<string>());
        }

        [Benchmark]
        public List<string> StreamReaderSliceByLine()
        {
            var list = new List<string>();
            foreach (var file in _files)
                using (var sr = new StreamReader(file.FullName))
                {
                    while (!sr.EndOfStream)
                    {
                        var text = sr.ReadLine().AsSpan();
                        var index = 0;
                        while (index < text.Length - 1)
                        {
                            var wordLength = 0;
                            if (list.Count == 2103) Debugger.Break();
                            while (char.IsWhiteSpace(text[index]) && index < text.Length - 1) index++;
                            while (index + wordLength < text.Length && !char.IsWhiteSpace(text[index + wordLength]))
                                wordLength++;
                            if (wordLength == 0) continue;
                            var term = text.Slice(index, wordLength);
                            list.Add(term.ToString());
                            index += wordLength;
                        }
                    }
                }

            return list;
        }

        [Benchmark]
        public List<string> StreamReaderSliceWholeFile()
        {
            var list = new List<string>();
            foreach (var file in _files)
                using (var sr = new StreamReader(file.FullName))
                {
                    var text = sr.ReadToEnd().AsSpan();
                    var index = 0;
                    while (index < text.Length - 1)
                    {
                        var wordLength = 0;
                        while (char.IsWhiteSpace(text[index]) && index < text.Length - 1) index++;
                        while (index + wordLength < text.Length && !char.IsWhiteSpace(text[index + wordLength]))
                            wordLength++;
                        if (wordLength == 0) continue;
                        var term = text.Slice(index, wordLength);
                        list.Add(term.ToString());
                        index += wordLength;
                    }
                }

            return list;
        }

        [Benchmark(Baseline = true)]
        public List<string> StreamReaderSplit()
        {
            var list = new List<string>();
            foreach (var file in _files)
                using (var sr = new StreamReader(file.FullName))
                {
                    while (!sr.EndOfStream)
                    {
                        var lineTerms = sr.ReadLine().Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                        list.AddRange(lineTerms);
                    }
                }

            return list;
        }
    }
}