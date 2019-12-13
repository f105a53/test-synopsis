using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search.Spell;
using Lucene.Net.Store;
using Lucene.Net.Util;
using SpellCheckService.Core.Entities;
using SpellCheckService.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SpellCheckService.Core.Services
{
    public class SpellCheckService : ISpellCheckService
    {
        public const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
        private SpellChecker spellChecker;

        public SpellCheckService(string path)
        {
            var dir = new MMapDirectory(path);
            var analyser = new StandardAnalyzer(AppLuceneVersion);
            var indexConfig = new IndexWriterConfig(AppLuceneVersion, analyser);
            var reader = DirectoryReader.Open(dir);
            spellChecker = new SpellChecker(reader.Directory);

            spellChecker.IndexDictionary(new LuceneDictionary(reader, "Body"), indexConfig, true);
        }

        public Spellings GetSpellings(Spellings.Request request)
        {
            // Return empty Spellings if request text is null or empty string.
            if (string.IsNullOrEmpty(request.Text))
                return new Spellings();

            var similar = spellChecker.SuggestSimilar(request.Text, 6);
            var spellings = new Spellings { spellings = similar };
            return spellings;
        }
    }
}
