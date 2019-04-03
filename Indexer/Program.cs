using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Common.Data;
using LinqToDB;
using LinqToDB.Data;

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
            var existingTerms = new HashSet<string>();

            Console.WriteLine("Wiping database");
            DataConnection.DefaultSettings = new LinqToDbSettings();
            DataConnection.TurnTraceSwitchOn();
            DataConnection.WriteTraceLine = (message, displayName) =>
            {
                Debug.WriteLine($"DB {displayName}: {message}");
            };

            var db = new DbContext();
            db.TermDoc.Delete();
            db.Document.Delete();
            db.Term.Delete();

            var root = new DirectoryInfo(@"/home/jghz/maildir");
            var lastReport = DateTime.Now;
            var size = 0L;
            var files = Crawl(root);
            const int chunkSize = 10000;

            while (files.Any())
            {
                var part = files.Take(chunkSize);
                files = files.Skip(chunkSize);
                
                var docs = new List<Document>();
                var terms = new HashSet<string>();
                var termDocs = new List<TermDoc>();

                foreach (var file in part)
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
                            while (index + wordLength < text.Length &&
                                   !char.IsWhiteSpace(text[index + wordLength]))
                                wordLength++;
                            if (wordLength == 0) continue;
                            var term = text.Slice(index, Math.Min(wordLength, 256)).ToString().ToLowerInvariant();
                            if (!existingTerms.Contains(term))
                            {
                                existingTerms.Add(term);
                                terms.Add(term); 
                            }

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
                        Console.WriteLine($"Current speed: {speed:F1}MB/s ({size}B in {sinceLastReport.TotalSeconds}s)");
                        lastReport = DateTime.Now;
                        size = file.Length;
                    }
                    else
                    {
                        size += file.Length;
                    }
                }

                Console.WriteLine("Writing Docs");
                db.BulkCopy(new BulkCopyOptions(), docs);
                Console.WriteLine("Writing Terms");
                db.BulkCopy(terms.Select(t => new Term {Value = t}).ToList());
                Console.WriteLine("Writing TermDocs");
                db.BulkCopy(new BulkCopyOptions(){},termDocs);
            }
        }
    }
}