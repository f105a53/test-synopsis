using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

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


        //[Benchmark]
        public List<string> Pipelines()
        {
            var pipe = new Pipe();
            var list = new List<string>();
            var writing = PipelinesFill(pipe.Writer);
            var reading = PipelinesRead(pipe.Reader, list);
            Task.WhenAll(writing, reading).GetAwaiter().GetResult();
            return list;
        }

        private async Task PipelinesFill(PipeWriter pipeWriter)
        {
            foreach (var fileInfo in _files)
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
                    position = buffer.PositionOf((byte) ' ');

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