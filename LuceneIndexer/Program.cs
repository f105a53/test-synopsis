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
using Lucene.Net.Store;
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
            Directory.Delete(indexLocation, true);
            var dir = FSDirectory.Open(indexLocation, new NativeFSLockFactory());

            //create an analyzer to process the text
            var analyzer = new StandardAnalyzer(AppLuceneVersion);

            //create an index writer
            var indexConfig = new IndexWriterConfig(AppLuceneVersion, analyzer);
            var writer = new IndexWriter(dir, indexConfig);

            var root = new DirectoryInfo(@"C:\maildir");
            const int chunkSize = 1000;
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
                            var doc = new Document();

                            doc.Add(new StringField("path", file.FullName, Field.Store.YES));
                            doc.Add(new Int64Field("Date", email.Date.Ticks, Field.Store.YES));
                            doc.Add(new TextField("Subject", email.Subject, Field.Store.YES));
                            foreach (var address in email.To)
                                doc.AddStringField("To", address.ToString(), Field.Store.YES);
                            foreach (var address in email.From)
                                doc.AddStringField("From", address.ToString(), Field.Store.YES);
                            doc.Add(new TextField("Body", email.TextBody, Field.Store.NO));
                            writer.AddDocument(doc);
                            size += file.Length.Bytes();
                        }
                        catch (Exception e)
                        {
                            Console.Error.WriteLine($"\nError while processing: {file.FullName}\n{e}\n");
                        }

                    writer.Flush(true, true);
                    var sincePartStart = DateTime.Now - partStart;
                    var speed = size.Per(sincePartStart);
                    spinner.Succeed($"Finished part: {speed.Humanize("G03").PadLeft(11)}\t{size.Humanize("G03")}");
                });
            }
        }
    }
}