using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Data;
using LinqToDB;
using LinqToDB.Data;
using ShellProgressBar;

namespace Indexer
{
    internal class Program
    {
        private static IEnumerable<FileInfo> Crawl(DirectoryInfo dir)
        {
            foreach (var file in dir.EnumerateFiles()) yield return file;

            foreach (var d in dir.EnumerateDirectories())
            foreach (var file in Crawl(d))
                yield return file;
        }

        private static void Main(string[] args)
        {
            var docs = new List<Document>();
            var terms = new HashSet<string>();
            var termDocs = new List<TermDoc>();

            var root = new DirectoryInfo(@"../../../../Benchmark/data");
            var files = Crawl(root).ToList();
            var lastReport = DateTime.Now;
            var size = 0L;

            using (var progress = new ProgressBar(files.Count, $"Reading {root.FullName}"))
            {
                foreach (var file in files)
                {
                    docs.Add(new Document {Path = file.FullName, LastModified = file.LastWriteTimeUtc});
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
                            var term = text.Slice(index, wordLength).ToString();
                            terms.Add(term);

                            termDocs.Add(new TermDoc
                            {
                                TermId = term,
                                LineNumber = 0,
                                LinePosition = 0,
                                DocumentPath = file.FullName,
                                Id = Guid.NewGuid()
                            });
                            index += wordLength;
                        }
                    }

                    var sinceLastReport = DateTime.Now - lastReport;
                    if (sinceLastReport > TimeSpan.FromSeconds(1))
                    {
                        var speed = size / sinceLastReport.TotalSeconds / 1024 / 1024;
                        progress.Tick($"Current speed: {speed}MB/s");
                        lastReport = DateTime.Now;
                        size = file.Length;
                    }
                    else
                    {
                        progress.Tick();
                        size += file.Length;
                    }
                }
            }

            return;

            DataConnection.DefaultSettings = new LinqToDbSettings();
            //LinqToDB.Data.DataConnection.TurnTraceSwitchOn();
            //LinqToDB.Data.DataConnection.WriteTraceLine = (message, displayName) => { Console.WriteLine($"{message} {displayName}"); };
            var db = new DbContext();
            db.TermDoc.Delete();
            db.Document.Delete();
            db.Term.Delete();

            using (var progress = new ProgressBar(docs.Count, "Uploading docs"))
            {
                db.BulkCopy(new BulkCopyOptions
                {
                    NotifyAfter = 100,
                    RowsCopiedCallback = r => progress.Tick(progress.CurrentTick + (int) r.RowsCopied, null)
                }, docs);
                progress.Tick(progress.MaxTicks);
            }

            var n = terms.OrderBy(s => s);
            using (var progress = new ProgressBar(terms.Count, "Uploading terms"))
            {
                db.BulkCopy(new BulkCopyOptions
                {
                    NotifyAfter = 100,
                    RowsCopiedCallback = r => progress.Tick(progress.CurrentTick + (int) r.RowsCopied, null)
                }, terms.Select(t => new Term {Value = t}).ToList());
                progress.Tick(progress.MaxTicks);
            }

            using (var progress = new ProgressBar(termDocs.Count, "Uploading docterms"))
            {
                db.BulkCopy(new BulkCopyOptions
                {
                    NotifyAfter = 100,
                    RowsCopiedCallback = r => progress.Tick(progress.CurrentTick + (int) r.RowsCopied, null)
                }, termDocs);
                progress.Tick(progress.MaxTicks);
            }
        }
    }
}