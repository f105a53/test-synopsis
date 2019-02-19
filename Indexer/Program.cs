using Common.Data;
using LinqToDB;
using LinqToDB.Data;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Indexer
{
    internal class Program
    {
        private static IEnumerable<FileInfo> Crawl(DirectoryInfo dir)
        {
            foreach (FileInfo file in dir.EnumerateFiles())
            {
                yield return file;
            }

            foreach (DirectoryInfo d in dir.EnumerateDirectories())
            {
                foreach (FileInfo file in Crawl(d))
                {
                    yield return file;
                }
            }
        }

        private static void Main(string[] args)
        {
            DataConnection.DefaultSettings = new LinqToDbSettings();
            //LinqToDB.Data.DataConnection.TurnTraceSwitchOn();
            //LinqToDB.Data.DataConnection.WriteTraceLine = (message, displayName) => { Console.WriteLine($"{message} {displayName}"); };

            DbContext db = new DbContext();
            db.TermDoc.Delete();
            db.Document.Delete();
            db.Term.Delete();

            List<Document> docs = new List<Document>();
            List<Term> terms = new List<Term>();
            List<TermDoc> termDocs = new List<TermDoc>();

            DirectoryInfo root = new DirectoryInfo(@"G:\Mapper\enron_dataset\maildir\allen-p\");
            List<FileInfo> files = Crawl(root).ToList();

            using (ProgressBar progress = new ProgressBar(files.Count, "Reading files"))
            {
                foreach (FileInfo file in files)
                {
                    // Perhaps change below to a stored procedure later on
                    //if (!db.Document.Any(d => d.Path == file.FullName))
                    //{
                    //    db.Document.Insert(() => new Document()
                    //        {LastModified = file.LastWriteTimeUtc, Path = file.FullName});
                    //}
                    docs.Add(new Document() { Path = file.FullName, LastModified = file.LastWriteTimeUtc });

                    using (StreamReader sr = file.OpenText())
                    {
                        int lineNumber = 0;
                        while (!sr.EndOfStream)
                        {
                            string[] lineTerms = sr.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            int wordNumber = 0;
                            foreach (string term in lineTerms)
                            {
                                Term t = new Term() { Value = term };
                                if (!terms.Contains(t))
                                {
                                    terms.Add(t);
                                }

                                termDocs.Add(new TermDoc() { TermId = t.Value, LineNumber = lineNumber, LinePosition = wordNumber, DocumentPath = file.FullName, Id = Guid.NewGuid().ToString() });
                            }
                        }
                    }

                    progress.Tick();
                }
            }

            using (ProgressBar progress = new ProgressBar(docs.Count, "Uploading docs"))
            {
                db.BulkCopy(new BulkCopyOptions()
                {
                    NotifyAfter = 100,
                    RowsCopiedCallback = r => progress.Tick(progress.CurrentTick + (int)r.RowsCopied, null)
                }, docs);
                progress.Tick(progress.MaxTicks);
            }
            using (ProgressBar progress = new ProgressBar(terms.Count, "Uploading terms"))
            {
                db.BulkCopy(new BulkCopyOptions()
                {
                    NotifyAfter = 100,
                    RowsCopiedCallback = r => progress.Tick(progress.CurrentTick + (int)r.RowsCopied, null)
                }, terms);
                progress.Tick(progress.MaxTicks);
            }

            using (ProgressBar progress = new ProgressBar(termDocs.Count, "Uploading docterms"))
            {
                db.BulkCopy(new BulkCopyOptions()
                {
                    NotifyAfter = 100,
                    RowsCopiedCallback = r => progress.Tick(progress.CurrentTick + (int)r.RowsCopied, null)
                }, termDocs);
                progress.Tick(progress.MaxTicks);
            }


        }
    }
}