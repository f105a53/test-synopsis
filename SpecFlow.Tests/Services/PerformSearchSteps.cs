using System;
using TechTalk.SpecFlow;
using SearchService.Core;
using SpellCheckService.Core;
using PreviewService.Core;
using System.IO;
using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Linq;
using System.IO.Abstractions.TestingHelpers;

namespace SpecFlow.Tests.Services
{
    [Binding]
    public class PerformSearchSteps
    {
        #region annoying fakeFileInput string
        private static string fakeFileInput = "Message-ID: <13389240.1075849630582.JavaMail.evans@thyme>\nDate: Fri, 15 Dec 2000 00:27:00 -0800 (PST)\nFrom: gary.waxman @enron.com\nTo: matt.harris @enron.com, glenn.surowiec @enron.com\nSubject: FW: SAP information for your proposal\nCc: jennifer.medcalf @enron.com\nMime-Version: 1.0\nContent-Type: text/plain; charset=us-ascii\nContent-Transfer-Encoding: 7bit\nBcc: jennifer.medcalf @enron.com\nX-From: Gary Waxman\nX-To: Matt Harris, Glenn Surowiec\nX-cc: Jennifer Medcalf\nX-bcc:\nX-Folder: \\John_Arnold_Nov2001\\Notes Folders\\Sap\nX-Origin: ARNOLD-J\nX-FileName: jarnold.nsf\n\nLooks like we are dead in the water based on this message and the one Glenn got. Let's meet to review this morning.\n\n-------------------------------------------------------------\nGary Waxman\nDirector, Enterprise Group\nEnron Broadband Services\n2100 SW River Parkway\nPortland, OR 97201\nMobile: 503-807-8923\nDesk:   503-886-0196\nFax:     503-886-0441\n-------------------------------------------------------------\n----- Forwarded by Gary Waxman/Enron Communications on 12/15/00 08:32 AM -----\n\ncurtis.meyer @sap.com\n12/15/00 05:54 AM\nTo: Gary Waxman/Enron Communications@Enron Communications\ncc:\nSubject: FW: SAP information for your proposal\nGary,\nFYI....\n-----Original Message-----\nFrom: Bruder, Dietmar\nSent: Friday, December 15, 2000 6:44 AM\nTo: Meyer, Curtis\nCc: Lingner, Annett\nSubject: RE: SAP information for your proposal\nHello Curtis,\nsorry for the late response.\nAn upgrade from E1 to E3 is not an option for the mentioned locations, for\nthe foreseeable future.\nThe 2. proposal is also not an option for us, because we buy bandwidth on\ndemand and the demands change very dynamically, therefore such a plan would\nnot fit our requirements.\nbest regards Dietmar\n-----------------------\n-----Original Message-----\nFrom: Meyer, Curtis\nSent: Montag, 11. Dezember 2000 17:22\nTo: Bruder, Dietmar\nCc: Gooden, Dennis; Bannon, Michael\nSubject: FW: SAP information for your proposal\nImportance: High\n\n\nDietmar,\nAttached is a message from Enron Broadband Services on sizing the\nproposal back to SAP.\n\nHopefully his attached message is clear in terms of finding a way to\nstructure it\nfor growth based on your needs.This would increase the size of our\narrangement if it\nmakes good sense for you.Please get back to me with your thoughts.\n\n-----Original Message-----\nFrom: Gary_Waxman @enron.net[mailto:Gary_Waxman@enron.net]\nSent: Friday, December 08, 2000 2:19 PM\nTo: Meyer, Curtis\nCc: Glenn_Surowiec @enron.net; Matt_Harris @enron.net\nSubject: Re: SAP information for your proposal\n\n\n\n\nCurtis - following our phone call this morning I met with Matt Harris and\nour\nstructuring person (Glenn Surowiec) to review the deal.After some quick\nanalysis it looks like we overestimated the opportunity.\n\nThe circuits Dietmar is requesting is a european E1 which equates to a US\nT1.\nThe hub (Wallfdorf) -and-spoke (Hungary, Netherlands, UK) model SAP requires\nworks out to roughly a $2.09M deal.\n\nClearly this is smaller than both parties would prefer.Thus we would like to\nknow if there are growth plans for one or more of these connections so that\nwe\ncan we propose a larger pipe (i.e.European E3/US DS3).\n\nFor example if just one of the connections can be an E3 for the entire\ncontract\n(ten years) it would bump the value up to $8.5M. Please review this and\nlet's\nplan on talking later today.Another model to consider would be to\nprogressively\nbump up the pipe size across all the connections at set intervals(i.e.\nyears\n1-3 = E1, years 4-6 = E2, years 7-10 = E3).\n\nPlease let me know what your thoughts are and let's talk ASAP.\n\n-------------------------------------------------------------\nGary Waxman\nDirector, Enterprise Group\nEnron Broadband Services\n2100 SW River Parkway\nPortland, OR 97201\nMobile: 503-807-8923\nDesk:   503-886-0196\nFax:     503-886-0441\n-------------------------------------------------------------\n\n\n|--------+----------------------->\n|        |          curtis.meyer@|\n|        |          sap.com      |\n|        |                       |\n|        |          12/08/00     |\n|        |          09:54 AM     |\n|        |                       |\n|--------+----------------------->\n\n>---------------------------------------------------------------------------\n-|\n  |\n|\n  |       To:     Gary Waxman/Enron Communications@Enron Communications\n|\n  |       cc:     dennis.gooden @sap.com, michael.bannon @sap.com\n|\n  |       Subject:     SAP information for your proposal\n|\n\n>---------------------------------------------------------------------------\n-|";
        #endregion

        private string searchTerm;

        private static SearchService.Core.Services.SearchService searchService;
        private static SpellCheckService.Core.Services.SpellCheckService spellCheckService;

        private SearchService.Core.Entities.SearchResults<SearchService.Core.Entities.Email> searchResults;
        private SpellCheckService.Core.Entities.Spellings spellCheckResults;


        private static async Task IndexTestFiles(string input, string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            string fileName = Guid.NewGuid().ToString() + ".txt";
            using (FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(input);
                    writer.Close();
                }
            }

            var indexer = new Common.Index(Path.Combine(path, "lucene-index"));
            await indexer.Build(path, 10000);
        }

        [BeforeFeature(Order = 0)]
        public static async Task SetupSearchService()
        {
            var tempPath = Path.GetTempPath() + Guid.NewGuid().ToString();
            await IndexTestFiles(fakeFileInput, tempPath);

            searchService = new SearchService.Core.Services.SearchService(Path.Combine(tempPath, "lucene-index"));
        }

        [BeforeFeature(Order = 1)]
        public static async Task SetupSpellCheckService()
        {
            var tempPath = Path.GetTempPath() + Guid.NewGuid().ToString();
            await IndexTestFiles(fakeFileInput, tempPath);

            spellCheckService = new SpellCheckService.Core.Services.SpellCheckService(Path.Combine(tempPath, "lucene-index"));
        }

        [Given(@"I want to search for (.*)")]
        public void GivenIWantToSearchFor(string searchTerm)
        {
            this.searchTerm = searchTerm;
        }
        
        [When(@"pressing search")]
        public void WhenPressingSearch()
        {
            searchResults = searchService.GetSearchResults(new SearchService.Core.Entities.SearchRequest { Text = searchTerm });
            spellCheckResults = spellCheckService.GetSpellings(new SpellCheckService.Core.Entities.Spellings.Request { Text = searchTerm });
        }
        
        [Then(@"first emails subject should be (.*)")]
        public void ThenFirstEmailsSubjectShouldBe(string firstSubject)
        {
            searchResults.Should().NotBeNull();
            searchResults.Results.Should().NotBeEmpty();
            searchResults.Results.First().Result.Subject.Should().Be(firstSubject);
        }
        
        [Then(@"first spelling suggestion should be (.*)")]
        public void ThenFirstSpellingSuggestionShouldBe(string firstSuggestion)
        {
            spellCheckResults.Should().NotBeNull();
            spellCheckResults.spellings.Should().NotBeNull().And.NotBeEmpty();
            spellCheckResults.spellings.First().Should().Be(firstSuggestion);
        }
        
        [Then(@"spelling suggestions should not be found")]
        public void ThenSpellingSuggestionsShouldNotBeFound()
        {
            spellCheckResults.spellings.Should().BeNullOrEmpty();
        }
        
        [Then(@"emails should not be found")]
        public void ThenEmailsShouldNotBeFound()
        {
            searchResults.Results.Should().BeNullOrEmpty();
        }
    }
}
