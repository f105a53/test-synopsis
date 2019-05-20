using System;
using System.Collections.Generic;
using System.Text;
using MimeKit;

namespace Common.Models
{
    public class Email
    {
        public string[] From { get; set; }
        public string[] To { get; set; }
        public string[] Cc { get; set; }
        public string[] Bcc { get; set; }
        public string Subject { get; set; }
        public DateTimeOffset Date { get; set; }
        public string Path { get; set; }
    }
}
