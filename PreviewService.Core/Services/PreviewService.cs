using MimeKit;
using PreviewService.Core.Entities;
using PreviewService.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreviewService.Core.Services
{
    public class PreviewService : IPreviewService
    {
        private IFileSystem _fileSystem;

        public PreviewService(IFileSystem fileSystem)
        {
            this._fileSystem = fileSystem;
        }

        public PreviewService()
        {
            this._fileSystem = new FileSystem();
        }

        public async Task<ResultPreview> GetResultPreview(ResultPreview.Request request)
        {
            // If the requests paths is null or any of the paths are empty, 
            // return an empty ResultPreview
            if (request.path == null || request.path.Any(x => string.IsNullOrEmpty(x)))
                return new ResultPreview();

            var previews = request.path.AsParallel().Select(async p =>
            {
                await using var stream = _fileSystem.File.Open(p, FileMode.Open, FileAccess.Read, FileShare.Read);
                var message = await MimeMessage.LoadAsync(stream);
                return (p, message.TextBody);
            });

            return new ResultPreview { Results = await Task.WhenAll(previews) };
        }
    }
}
