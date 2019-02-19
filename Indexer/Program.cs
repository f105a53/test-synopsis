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
            db.Document.Delete();
            List<Document> docs = new List<Document>();
            DirectoryInfo root = new DirectoryInfo(@"G:\Mapper\enron_dataset\maildir\allen-p\");
            List<FileInfo> files = Crawl(root).ToList();
            SortedDictionary<string, List<TermLocation>> index = new SortedDictionary<string, List<TermLocation>>();
            using (ProgressBar progress = new ProgressBar(files.Count, "Reading files"))
            {
                foreach (FileInfo file in files)
                {
                    //// Perhaps change below to a stored procedure later on
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
                            TermLocation loc = new TermLocation { Path = file.FullName, LineNumber = lineNumber++ };
                            string[] terms = sr.ReadLine().Split(' ');
                            foreach (string term in terms)
                            {
                                if (index.ContainsKey(term))
                                {
                                    index[term].Add(loc);
                                }
                                else
                                {
                                    index.Add(term, new List<TermLocation> { loc });
                                }
                            }
                        }
                    }

                    progress.Tick();
                }
            }

            using (ProgressBar progress = new ProgressBar(docs.Count, "Uploading"))
            {
                db.BulkCopy(new BulkCopyOptions()
                {
                    NotifyAfter = 100,
                    RowsCopiedCallback = r => progress.Tick(progress.CurrentTick + (int)r.RowsCopied,null)
                }, docs);
            }

            //Console.WriteLine(index.Count);
            //while (true)
            //{
            //    string search = Console.ReadLine();
            //    if (index.TryGetValue(search, out List<TermLocation> locs))
            //    {
            //        foreach (TermLocation loc in locs)
            //        {
            //            Console.WriteLine($"{loc.Path}@{loc.LineNumber}");
            //        }
            //    }
            //    else
            //    {
            //        foreach ((string term, List<TermLocation> locS) in index)
            //        {
            //            if (term.Contains(search))
            //            {
            //                foreach (TermLocation loc in locS)
            //                {
            //                    Console.WriteLine($"{loc.Path}@{loc.LineNumber}");
            //                }
            //            }
            //        }
            //    }
            //}
        }

        private struct TermLocation
        {
            public string Path;
            public int LineNumber;
        }
    }
}