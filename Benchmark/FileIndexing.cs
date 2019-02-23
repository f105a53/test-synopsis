using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Benchmark
{
    [CoreJob]
    [RPlotExporter]
    [RankColumn]
    [MemoryDiagnoser]
    public class FileIndexing
    {
        [Params("data/arnold-j")]
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

        [Benchmark]
        public List<string> FileReadAllTextSplitLinq()
        {
            return (from file in GetFiles()
                    from line in File.ReadAllLines(file.FullName)
                    from term in line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries)
                    select term.ToLowerInvariant()).ToList();
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

        [Benchmark]
        public async Task<List<string>> Pipelines()
        {
            var pipe = new Pipe();
            var list = new List<string>();
            var writing = PipelinesFill(pipe.Writer);
            var reading = PipelinesRead(pipe.Reader, list);
            await Task.WhenAll(writing, reading);
            return list;
        }

        private async Task PipelinesFill(PipeWriter pipeWriter)
        {
            foreach (var fileInfo in GetFiles())
                using (var stream = fileInfo.OpenRead())
                {
                    while (true)
                    {
                        var memory = pipeWriter.GetMemory();
                        var bytesRead = await stream.ReadAsync(memory);
                        if (bytesRead == 0) break;
                        pipeWriter.Advance(bytesRead);
                        await pipeWriter.FlushAsync();
                    }
                }

            pipeWriter.Complete();
        }

        private async Task PipelinesRead(PipeReader reader, List<string> list)
        {
            while (true)
            {
                var result = await reader.ReadAsync();
                var buffer = result.Buffer;
                SequencePosition? position = null;

                do
                {
                    // Look for a EOL in the buffer
                    position = buffer.PositionOf((byte)' ');

                    if (position != null)
                    {
                        // Process the line
                        list.Add(Encoding.UTF8.GetString(buffer.Slice(0, position.Value).ToArray()));

                        // Skip the line + the \n character (basically position)
                        buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
                    }
                } while (position != null);

                // Tell the PipeReader how much of the buffer we have consumed
                reader.AdvanceTo(buffer.Start, buffer.End);

                // Stop reading if there's no more data coming
                if (result.IsCompleted) break;
            }
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

        [Benchmark(Baseline = true)]
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