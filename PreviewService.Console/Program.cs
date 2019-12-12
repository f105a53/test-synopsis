using EasyNetQ;
using EasyNetQ.Logging;
using PreviewService.Core.Entities;
using System;
using System.Threading;

namespace PreviewService.Console
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
            using var bus = RabbitHutch.CreateBus(Environment.GetEnvironmentVariable("RABBITMQ_CSTRING") ?? "host=localhost");
            bus.RespondAsync<ResultPreview.Request, ResultPreview>(async request =>
            {
                var service = new PreviewService.Core.Services.PreviewService();
                return await service.GetResultPreview(request);
            });

            System.Console.WriteLine("Running...\nPress Ctrl+C to exit");
            exitEvent.WaitOne();
        }
    }
}
