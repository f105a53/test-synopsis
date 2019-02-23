using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            using (var progress = new ProgressBar(files.Count, $"Reading {root.FullName}"))
            {
                foreach (var file in files)
                {
                    // Perhaps change below to a stored procedure later on
                    //if (!db.Document.Any(d => d.Path == file.FullName))
                    //{
                    //    db.Document.Insert(() => new Document()
                    //        {LastModified = file.LastWriteTimeUtc, Path = file.FullName});
                    //}
                    docs.Add(new Document {Path = file.FullName, LastModified = file.LastWriteTimeUtc});

                    using (var sr = file.OpenText())
                    {
                        var lineNumber = 0;
                        while (!sr.EndOfStream)
                        {
                            var rawLine = sr.ReadLine();
                            var line = rawLine.ToLowerInvariant();
                            var words = line.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                            var wordNumber = 0;
                            lineNumber++;
                            foreach (var term in words)
                            {
                                terms.Add(term);

                                termDocs.Add(new TermDoc
                                {
                                    TermId = term, LineNumber = lineNumber, LinePosition = wordNumber++,
                                    DocumentPath = file.FullName, Id = Guid.NewGuid()
                                });
                            }
                        }
                    }

                    progress.Tick();
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