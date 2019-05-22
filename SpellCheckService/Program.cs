using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Common.Models;
using EasyNetQ;
using EasyNetQ.Logging;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search.Spell;
using Lucene.Net.Store;
using Index = Common.Index;

namespace SpellCheckService
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main";
            var exitEvent = new ManualResetEvent(false);
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                exitEvent.Set();
            };

            var dir = new MMapDirectory("./lucene-index", new NativeFSLockFactory());
            var analyzer = new StandardAnalyzer(Index.AppLuceneVersion);
            var indexConfig = new IndexWriterConfig(Index.AppLuceneVersion, analyzer);
            var indexWriter = new IndexWriter(dir, indexConfig);
            Debug.WriteLine($"Records in index: {indexWriter.NumDocs}");
            using var reader = indexWriter.GetReader(false);
            var spellChecker = new SpellChecker(dir);
            spellChecker.IndexDictionary(new LuceneDictionary(reader, "Body"), indexConfig, true);

            LogProvider.SetCurrentLogProvider(ConsoleLogProvider.Instance);
            using var bus =
                RabbitHutch.CreateBus(Environment.GetEnvironmentVariable("RABBITMQ_CSTRING") ?? "host=localhost");
            bus.RespondAsync<Spellings.Request, Spellings>(request => Task.Factory.StartNew(() =>
            {
                var similar = spellChecker.SuggestSimilar(request.Text, 6);
                var spellings = new Spellings {spellings = similar};
                return spellings;
            }));

            Console.WriteLine("Running...\nPress Ctrl+C to exit");
            exitEvent.WaitOne();
        }
    }
}