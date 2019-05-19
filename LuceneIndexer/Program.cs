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
    internal static class Program
    {
        private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
        private const int ChunkSize = 10000;
        private const string FilesToIndex = @"C:\maildir";
        private const string IndexLocation = @"./lucene-index";

        private static IEnumerable<FileInfo> Crawl(DirectoryInfo dir)
        {
            foreach (var file in dir.EnumerateFiles()) yield return file;
            foreach (var d in dir.EnumerateDirectories())
            foreach (var file in Crawl(d))
                yield return file;
        }

        private static async Task Main(string[] args)
        {
            var indexFolderExists = Directory.Exists(IndexLocation);

            //prepare lucene
            var dir = new MMapDirectory(IndexLocation, new NativeFSLockFactory());
            var analyzer = new StandardAnalyzer(AppLuceneVersion);
            var indexConfig = new IndexWriterConfig(AppLuceneVersion, analyzer);
            using var writer = new IndexWriter(dir, indexConfig);

            if (indexFolderExists)
            {
                var searcher = new IndexSearcher(writer.GetReader(true));
                var hits = searcher.Search(new PhraseQuery {new Term("From", "phillip.allen@enron.com")}, 20);
                foreach (var hit in hits.ScoreDocs)
                {
                    var foundDoc = searcher.Doc(hit.Doc);
                    Console.WriteLine($"{hit.Score}");
                    foreach (var field in foundDoc.Fields)
                        Console.WriteLine(
                            $"{field.Name}:\t{field.GetStringValue() ?? field.GetInt64Value().ToString()}");

                    Console.WriteLine();
                }
            }
            else
            {
                var root = new DirectoryInfo(FilesToIndex);
                var batches = Crawl(root).Batch(ChunkSize, r => r.ToList()).ToList();
                var count = 0;

                foreach (var part in batches)
                {
                    //Measure speed and progress
                    count++;
                    var partStart = DateTime.Now;
                    var size = ByteSize.FromBits(0);

                    await Spinner.StartAsync($"Processing part {count}/{batches.Count}", async spinner =>
                    {
                        foreach (var file in part)
                            try
                            {
                                //Decode email
                                var email = await MimeMessage.LoadAsync(file.FullName);
                                //Gather fields
                                var fields = new List<IIndexableField>
                                {
                                    new StringField("path", file.FullName, Field.Store.YES),
                                    new Int64Field("Date", email.Date.Ticks, Field.Store.YES),
                                    new TextField("Subject", email.Subject, Field.Store.YES)
                                };
                                var listFields = new[]
                                {
                                    ("To", email.To),
                                    ("From", email.From),
                                    ("CC", email.Cc),
                                    ("BCC", email.Bcc)
                                };
                                foreach (var (s, i) in listFields)
                                foreach (var address in i)
                                    fields.Add(new StringField(s, address.ToString(), Field.Store.YES));
                                fields.Add(new TextField("Body", email.TextBody, Field.Store.NO));
                                //Create lucene document, add fields, add to index
                                var doc = new Document();
                                doc.Fields.AddRange(fields);
                                writer.AddDocument(doc);

                                size += file.Length.Bytes();
                            }
                            catch (Exception e)
                            {
                                Console.Error.WriteLine($"\nError while processing: {file.FullName}\n{e}\n");
                            }

                        //At the end of the batch, flush changes to index
                        writer.Flush(false, false);
                        //Report speed
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