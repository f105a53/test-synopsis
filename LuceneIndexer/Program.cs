using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Humanizer.Bytes;
using Kurukuru;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Support;
using Lucene.Net.Util;
using MimeKit;
using MoreLinq;
using Directory = System.IO.Directory;

namespace LuceneIndexer
{
    internal class Program
    {
        private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;

        private static IEnumerable<FileInfo> Crawl(DirectoryInfo dir)
        {
            foreach (var file in dir.EnumerateFiles()) yield return file;
            foreach (var d in dir.EnumerateDirectories())
            foreach (var file in Crawl(d))
                yield return file;
        }

        private static async Task Main(string[] args)
        {
            const string indexLocation = @"./lucene-index";
            var exists = Directory.Exists(indexLocation);
            //Directory.Delete(indexLocation, true);
            var dir = new MMapDirectory(indexLocation, new NativeFSLockFactory());

            //create an analyzer to process the text
            var analyzer = new StandardAnalyzer(AppLuceneVersion);

            //create an index writer
            var indexConfig = new IndexWriterConfig(AppLuceneVersion, analyzer);
            using var writer = new IndexWriter(dir, indexConfig);
            
            if (exists)
            {
                var search = new IndexSearcher(writer.GetReader(true));
                var collectionStatistics = search.CollectionStatistics("Subject");
                var hits = search.Search(new PhraseQuery { new Term("From", "phillip.allen@enron.com") }, 20);
                foreach (var hit in hits.ScoreDocs)
                {
                    var foundDoc = search.Doc(hit.Doc);
                    Console.WriteLine($"{hit.Score}");
                    foreach (var field in foundDoc.Fields)
                    {
                        Console.WriteLine($"{field.Name}:\t{field.GetStringValue() ?? field.GetInt64Value().ToString()}");
                    }

                    Console.WriteLine();
                }
            }
            else
            {
                var root = new DirectoryInfo(@"C:\maildir");
                const int chunkSize = 10000;
                var batches = Crawl(root).Batch(chunkSize, r => r.ToList()).ToList();
                var count = 0;
                foreach (var part in batches)
                {
                    count++;
                    var partStart = DateTime.Now;
                    var size = ByteSize.FromBits(0);

                    await Spinner.StartAsync($"Processing part {count}/{batches.Count}", async spinner =>
                    {
                        foreach (var file in part)
                            try
                            {
                                var email = await MimeMessage.LoadAsync(file.FullName);
                                var fields = new List<IIndexableField>
                                {
                                        new StringField("path", file.FullName, Field.Store.YES),
                                        new Int64Field("Date", email.Date.Ticks, Field.Store.YES),
                                        new TextField("Subject", email.Subject, Field.Store.YES)
                                };
                                fields.AddRange(email.From.Select(address =>
                                    new StringField("To", address.ToString(), Field.Store.YES)));
                                fields.AddRange(email.To.Select(address =>
                                    new StringField("From", address.ToString(), Field.Store.YES)));
                                fields.Add(new TextField("Body", email.TextBody, Field.Store.NO));
                                var doc = new Document();
                                doc.Fields.AddRange(fields);
                                writer.AddDocument(doc);
                                size += file.Length.Bytes();
                            }
                            catch (Exception e)
                            {
                                Console.Error.WriteLine($"\nError while processing: {file.FullName}\n{e}\n");
                            }

                        writer.Flush(false, false);
                        var sincePartStart = DateTime.Now - partStart;
                        var speed = size.Per(sincePartStart);
                        spinner.Succeed(
                            $"Finished part: {speed.Humanize("G03").PadLeft(11)}\t{size.Humanize("G03")}");
                    });
                }

            }
        }
    }
}