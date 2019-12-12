using PreviewService.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PreviewService.Core.Interfaces
{
    public interface IPreviewService
    {
        Task<ResultPreview> GetResultPreview(ResultPreview.Request request);
    }
}
