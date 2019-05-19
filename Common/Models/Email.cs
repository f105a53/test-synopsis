using System;
using System.Collections.Generic;
using System.Text;
using MimeKit;

namespace Common.Models
{
    public class Email
    {
        public InternetAddressList From { get; set; }
        public InternetAddressList To { get; set; }
        public InternetAddressList Cc { get; set; }
        public InternetAddressList Bcc { get; set; }
        public string Subject { get; set; }
        public DateTimeOffset Date { get; set; }
        public string Path { get; set; }
    }
}
