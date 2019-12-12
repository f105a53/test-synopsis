using EasyNetQ;
using EasyNetQ.Logging;
using SearchService.Core.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SearchService.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main";
            var exitEvent = new ManualResetEvent(false);
            System.Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                exitEvent.Set();
            };

            LogProvider.SetCurrentLogProvider(ConsoleLogProvider.Instance);
            var bus = RabbitHutch.CreateBus(Environment.GetEnvironmentVariable("RABBITMQ_CSTRING") ?? "host=localhost");
            var service = new SearchService.Core.Services.SearchService("./lucene-index");

            bus.RespondAsync<SearchRequest, SearchResults<Email>>(request =>
                Task.Factory.StartNew(() => service.GetSearchResults(request)));

            System.Console.WriteLine("Running...\nPress Ctrl+C to exit");
            exitEvent.WaitOne();
        }
    }
}
