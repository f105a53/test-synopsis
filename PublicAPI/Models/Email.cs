using System;
using AutoMapper;

namespace Server.Models
{
    public class Email
    {
        public string[] Bcc { get; set; }
        public string Body { get; set; }
        public string[] Cc { get; set; }
        public DateTimeOffset Date { get; set; }
        public string[] From { get; set; }
        public string Path { get; set; }
        public string Subject { get; set; }
        public string[] To { get; set; }
    }

    public class EmailProfile : Profile
    {
        public EmailProfile()
        {
            CreateMap<Common.Models.Email, Email>().ForMember(e => e.Body, expression => expression.Ignore());
        }
    }
}