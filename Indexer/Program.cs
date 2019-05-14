using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Common.Data;
using Kurukuru;
using LinqToDB;
using LinqToDB.Data;
using MoreLinq;

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

            DataConnection.DefaultSettings = new LinqToDbSettings();
            DataConnection.TurnTraceSwitchOn();
            DataConnection.WriteTraceLine = (message, displayName) => Trace.WriteLine($"DB {displayName}: {message}");

            var db = new DbContext();

            Spinner.Start("Wiping Database", spinner =>
            {
                db.CommandTimeout = 600;
                spinner.Text = "Wiping TermDocs";
                db.TermDoc.Delete();
                spinner.Text = "Wiping Documents";
                db.Document.Delete();
                spinner.Text = "Wiping Terms";
                db.Term.Delete();
            });

            var root = new DirectoryInfo(@"C:\maildir");
            var files = Crawl(root).ToList();
            const int chunkSize = 1000;

            while (files.Any())
            {
                var partStart = DateTime.Now;
                var part = files.Take(chunkSize);
                files = files.Skip(chunkSize).ToList();

                var docs = new List<Document>();
                var terms = new HashSet<string>();
                var termDocs = new List<TermDoc>();
                var size = 0L;

                Spinner.Start("Processing part", spinner =>
                {
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
                                    LinePosition = index,
                                    DocumentPath = file.FullName,
                                    Id = Guid.NewGuid()
                                });
                                index += wordLength;
                            }
                        }

                        size += file.Length;
                    }

                    var sincePartStart = DateTime.Now - partStart;
                    var speed = size / sincePartStart.TotalSeconds / 1024 / 1024;
                    spinner.Succeed($"Finished part: {speed:F3}MB/s ({size}B in {sincePartStart.TotalSeconds:F3}s)");
                });


                SendToDb(db, docs);
                SendToDb(db, terms.Select(t => new Term {Value = t}).ToList());
                SendToDb(db, termDocs);
            }
        }

        private static void SendToDb<T>(DbContext db, ICollection<T> data) where T : class
        {
            Spinner.Start($"Writing {data.Count} {typeof(T).Name}",
                spinner =>
                {
                    var count = 0;
                    try
                    {
                        foreach (var partE in data.Batch(100000))
                        {
                            var part = partE.ToList();
                            db.BulkCopy(part);
                            count += part.Count;
                            spinner.Text = $"Writing {data.Count} {typeof(T).Name}: count";
                        }
                    }
                    catch (SqlException ex)
                    {
                        Debugger.Break();
                        Console.Error.WriteLine(ex);
                        spinner.Fail();
                    }
                });
        }
    }
}