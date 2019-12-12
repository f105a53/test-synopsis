using SpellCheckService.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpellCheckService.Core.Interfaces
{
    public interface ISpellCheckService
    {
        Spellings GetSpellings(Spellings.Request request);
    }
}
