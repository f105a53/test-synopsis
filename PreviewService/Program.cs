using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Models;
using EasyNetQ;
using MimeKit;

namespace PreviewService
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


            using var bus = RabbitHutch.CreateBus(Environment.GetEnvironmentVariable("RABBITMQ_CSTRING") ?? "host=localhost");
            bus.RespondAsync<ResultPreview.Request, ResultPreview>(async request =>
                {
                    var previews = request.path.AsParallel().Select(async p =>
                    {
                        await using var stream =
                            new FileStream(p, FileMode.Open, FileAccess.Read, FileShare.Read);
                        var message = await MimeMessage.LoadAsync(stream);
                        return (p, message.TextBody);
                    });
                    return new ResultPreview {Results = await Task.WhenAll(previews)};
                }
            );

            Console.WriteLine("Running...\nPress Ctrl+C to exit");
            exitEvent.WaitOne();
        }
    }
}