using System;
using System.Collections.Generic;
using System.Text;

namespace PreviewService.Core.Entities
{
    public class ResultPreview
    {
        public (string path, string body)[] Results { get; set; }

        public class Request
        {
            public string[] path { get; set; }
        }
    }
}
