using System;
using System.Collections.Generic;
using System.Text;

namespace SpellCheckService.Core.Entities
{
    public class Spellings
    {
        public string[] spellings { get; set; }

        public class Request
        {
            public string Text { get; set; }
        }
    }
}
