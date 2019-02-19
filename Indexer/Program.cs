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
            DataConnection.DefaultSettings = new LinqToDbSettings();
            var db = new DbContext();
            var docs = new List<Document>();
            var root = new DirectoryInfo(@"D:\j200g\Documents\IISExpress");
            var files = Crawl(root).ToList();
            var index = new SortedDictionary<string, List<TermLocation>>();
            using (var progresss = new ProgressBar(files.Count, "Reading files"))
            {
                foreach (var file in files)
                {
                    //// Perhaps change below to a stored procedure later on
                    //if (!db.Document.Any(d => d.Path == file.FullName))
                    //{
                    //    db.Document.Insert(() => new Document()
                    //        {LastModified = file.LastWriteTimeUtc, Path = file.FullName});
                    //}
                    docs.Add(new Document(){Path = file.FullName,LastModified = file.LastWriteTimeUtc});
                    using (var sr = file.OpenText())
                    {
                        var lineNumber = 0;
                        while (!sr.EndOfStream)
                        {
                            var loc = new TermLocation {Path = file.FullName, LineNumber = lineNumber++};
                            var terms = sr.ReadLine().Split(' ');
                            foreach (var term in terms)
                                if (index.ContainsKey(term))
                                    index[term].Add(loc);
                                else
                                    index.Add(term, new List<TermLocation> {loc});
                        }
                    }

                    progresss.Tick();
                }
            }

            using (var progresss = new ProgressBar(docs.Count, "Uploading"))
            {
                db.BulkCopy(new BulkCopyOptions() {NotifyAfter = 100, RowsCopiedCallback = r => progresss.Tick(progresss.CurrentTick+100)}, docs);
            }

            Console.WriteLine(index.Count);
            while (true)
            {
                var search = Console.ReadLine();
                if (index.TryGetValue(search, out var locs))
                    foreach (var loc in locs)
                        Console.WriteLine($"{loc.Path}@{loc.LineNumber}");
                else
                    foreach (var (term, locS) in index)
                        if (term.Contains(search))
                            foreach (var loc in locS)
                                Console.WriteLine($"{loc.Path}@{loc.LineNumber}");
            }
        }

        private struct TermLocation
        {
            public string Path;
            public int LineNumber;
        }
    }
}