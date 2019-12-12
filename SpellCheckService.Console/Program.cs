using EasyNetQ;
using EasyNetQ.Logging;
using SpellCheckService.Core.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SpellCheckService.Console
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

            bus.RespondAsync<Spellings.Request, Spellings>(request => Task.Factory.StartNew(() =>
            {
                var service = new SpellCheckService.Core.Services.SpellCheckService("./lucene-index");
                return service.GetSpellings(request);
            }));

            System.Console.WriteLine("Running...\nPress Ctrl+C to exit");
            exitEvent.WaitOne();
        }
    }
}
