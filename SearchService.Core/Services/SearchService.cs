using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Simple;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using SearchService.Core.Entities;
using SearchService.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SearchService.Core.Services
{
    public class SearchService : ISearchService
    {
        public const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
        private readonly Analyzer _analyzer;
        private readonly IndexWriter _indexWriter;

        public SearchService(string path)
        {
            var dir = new MMapDirectory(path, new NativeFSLockFactory());
            _analyzer = new StandardAnalyzer(AppLuceneVersion);
            var indexConfig = new IndexWriterConfig(AppLuceneVersion, _analyzer);
            _indexWriter = new IndexWriter(dir, indexConfig);
        }

        public SearchResults<Email> GetSearchResults(SearchRequest request)
        {
            var reader = _indexWriter.GetReader(false);
            var searcher = new IndexSearcher(reader);
            var parser = new SimpleQueryParser(_analyzer,
                new Dictionary<string, float> { { "Subject", 0.5f }, { "Body", 0.5f } });
            var query = parser.Parse(request.Text);
            var hits = searcher.Search(query, 20);
            var results = from hit in hits.ScoreDocs
                          let document = searcher.Doc(hit.Doc)
                          let email = new Email
                          {
                              From = GetEmails(document, "From"),
                              To = GetEmails(document, "To"),
                              Cc = GetEmails(document, "CC"),
                              Bcc = GetEmails(document, "BCC"),
                              Path = document.Get("path"),
                              Date = new DateTimeOffset(document.GetField("Date").GetInt64Value() ?? 0, TimeSpan.Zero),
                              Subject = document.Get("Subject")
                          }
                          select (hit.Score, email);

            return new SearchResults<Email> { Results = results.ToList() };
        }

        private static string[] GetEmails(Document document, string fieldName)
        {
            return document.GetFields(fieldName).Select(f => f.GetStringValue()).ToArray();
        }
    }
}
