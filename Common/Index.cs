﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Humanizer.Bytes;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Simple;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Support;
using Lucene.Net.Util;
using MimeKit;
using MoreLinq;

namespace Common
{
    public class Index : IDisposable
    {
        private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
        private readonly Analyzer _analyzer;
        private readonly IndexWriter _indexWriter;

        public Index(string indexPath)
        {
            //prepare lucene
            var dir = new MMapDirectory(indexPath, new NativeFSLockFactory());
            _analyzer = new StandardAnalyzer(AppLuceneVersion);
            var indexConfig = new IndexWriterConfig(AppLuceneVersion, _analyzer);
            _indexWriter = new IndexWriter(dir, indexConfig);
        }


        public void Dispose()
        {
            _indexWriter?.Dispose();
        }

        public async Task Build(string path, int batchSize, IProgress<string> progress)
        {
            var root = new DirectoryInfo(path);

            var batches = Crawl(root).Batch(batchSize, r => r.ToList()).ToList();
            var count = 0;

            foreach (var part in batches)
            {
                //Measure speed and progress
                count++;
                var partStart = DateTime.Now;
                var size = ByteSize.FromBits(0);

                progress.Report($"Processing part {count}/{batches.Count}");

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
                        _indexWriter.AddDocument(doc);

                        size += file.Length.Bytes();
                    }
                    catch (Exception e)
                    {
                        progress.Report($"\nError while processing: {file.FullName}\n{e}\n");
                    }

                //At the end of the batch, flush changes to index
                _indexWriter.Flush(false, false);
                //Report speed
                var sincePartStart = DateTime.Now - partStart;
                var speed = size.Per(sincePartStart);
                progress.Report($"Finished part: {speed.Humanize("G03").PadLeft(11)}\t{size.Humanize("G03")}");
            }
        }

        private static IEnumerable<FileInfo> Crawl(DirectoryInfo dir)
        {
            foreach (var file in dir.EnumerateFiles()) yield return file;
            foreach (var d in dir.EnumerateDirectories())
            foreach (var file in Crawl(d))
                yield return file;
        }

        public TopDocs Search(string searchText)
        {
            using var reader = _indexWriter.GetReader(false);
            var searcher = new IndexSearcher(reader);
            var parser = new SimpleQueryParser(_analyzer,
                new Dictionary<string, float> {{"Subject", 0.5f}, {"Body", 0.5f}});
            var query = parser.Parse(searchText);
            return searcher.Search(query, 20);
        }
    }
}