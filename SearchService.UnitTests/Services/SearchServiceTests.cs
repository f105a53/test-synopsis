using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SearchService.UnitTests.Services
{
    public class SearchServiceTests
    {
        [InlineData("deal", "FW: SAP information for your proposal")]
        [Theory]
        public async void Test_GetSearchResults(string input, string expected)
        {
            string fakeFileInput = "Message-ID: <13389240.1075849630582.JavaMail.evans@thyme>\nDate: Fri, 15 Dec 2000 00:27:00 -0800 (PST)\nFrom: gary.waxman @enron.com\nTo: matt.harris @enron.com, glenn.surowiec @enron.com\nSubject: FW: SAP information for your proposal\nCc: jennifer.medcalf @enron.com\nMime-Version: 1.0\nContent-Type: text/plain; charset=us-ascii\nContent-Transfer-Encoding: 7bit\nBcc: jennifer.medcalf @enron.com\nX-From: Gary Waxman\nX-To: Matt Harris, Glenn Surowiec\nX-cc: Jennifer Medcalf\nX-bcc:\nX-Folder: \\John_Arnold_Nov2001\\Notes Folders\\Sap\nX-Origin: ARNOLD-J\nX-FileName: jarnold.nsf\n\nLooks like we are dead in the water based on this message and the one Glenn got. Let's meet to review this morning.\n\n-------------------------------------------------------------\nGary Waxman\nDirector, Enterprise Group\nEnron Broadband Services\n2100 SW River Parkway\nPortland, OR 97201\nMobile: 503-807-8923\nDesk:   503-886-0196\nFax:     503-886-0441\n-------------------------------------------------------------\n----- Forwarded by Gary Waxman/Enron Communications on 12/15/00 08:32 AM -----\n\ncurtis.meyer @sap.com\n12/15/00 05:54 AM\nTo: Gary Waxman/Enron Communications@Enron Communications\ncc:\nSubject: FW: SAP information for your proposal\nGary,\nFYI....\n-----Original Message-----\nFrom: Bruder, Dietmar\nSent: Friday, December 15, 2000 6:44 AM\nTo: Meyer, Curtis\nCc: Lingner, Annett\nSubject: RE: SAP information for your proposal\nHello Curtis,\nsorry for the late response.\nAn upgrade from E1 to E3 is not an option for the mentioned locations, for\nthe foreseeable future.\nThe 2. proposal is also not an option for us, because we buy bandwidth on\ndemand and the demands change very dynamically, therefore such a plan would\nnot fit our requirements.\nbest regards Dietmar\n-----------------------\n-----Original Message-----\nFrom: Meyer, Curtis\nSent: Montag, 11. Dezember 2000 17:22\nTo: Bruder, Dietmar\nCc: Gooden, Dennis; Bannon, Michael\nSubject: FW: SAP information for your proposal\nImportance: High\n\n\nDietmar,\nAttached is a message from Enron Broadband Services on sizing the\nproposal back to SAP.\n\nHopefully his attached message is clear in terms of finding a way to\nstructure it\nfor growth based on your needs.This would increase the size of our\narrangement if it\nmakes good sense for you.Please get back to me with your thoughts.\n\n-----Original Message-----\nFrom: Gary_Waxman @enron.net[mailto:Gary_Waxman@enron.net]\nSent: Friday, December 08, 2000 2:19 PM\nTo: Meyer, Curtis\nCc: Glenn_Surowiec @enron.net; Matt_Harris @enron.net\nSubject: Re: SAP information for your proposal\n\n\n\n\nCurtis - following our phone call this morning I met with Matt Harris and\nour\nstructuring person (Glenn Surowiec) to review the deal.After some quick\nanalysis it looks like we overestimated the opportunity.\n\nThe circuits Dietmar is requesting is a european E1 which equates to a US\nT1.\nThe hub (Wallfdorf) -and-spoke (Hungary, Netherlands, UK) model SAP requires\nworks out to roughly a $2.09M deal.\n\nClearly this is smaller than both parties would prefer.Thus we would like to\nknow if there are growth plans for one or more of these connections so that\nwe\ncan we propose a larger pipe (i.e.European E3/US DS3).\n\nFor example if just one of the connections can be an E3 for the entire\ncontract\n(ten years) it would bump the value up to $8.5M. Please review this and\nlet's\nplan on talking later today.Another model to consider would be to\nprogressively\nbump up the pipe size across all the connections at set intervals(i.e.\nyears\n1-3 = E1, years 4-6 = E2, years 7-10 = E3).\n\nPlease let me know what your thoughts are and let's talk ASAP.\n\n-------------------------------------------------------------\nGary Waxman\nDirector, Enterprise Group\nEnron Broadband Services\n2100 SW River Parkway\nPortland, OR 97201\nMobile: 503-807-8923\nDesk:   503-886-0196\nFax:     503-886-0441\n-------------------------------------------------------------\n\n\n|--------+----------------------->\n|        |          curtis.meyer@|\n|        |          sap.com      |\n|        |                       |\n|        |          12/08/00     |\n|        |          09:54 AM     |\n|        |                       |\n|--------+----------------------->\n\n>---------------------------------------------------------------------------\n-|\n  |\n|\n  |       To:     Gary Waxman/Enron Communications@Enron Communications\n|\n  |       cc:     dennis.gooden @sap.com, michael.bannon @sap.com\n|\n  |       Subject:     SAP information for your proposal\n|\n\n>---------------------------------------------------------------------------\n-|";
            var tempPath = Path.GetTempPath() + Guid.NewGuid().ToString();
            await IndexTestFiles(fakeFileInput, tempPath);
            var service = new SearchService.Core.Services.SearchService(Path.Combine(tempPath, "lucene-index"));

            var results = service.GetSearchResults(new Core.Entities.SearchRequest { Text = input });

            Assert.Matches(expected, results.Results[0].Result.Subject);
        }

        private async Task IndexTestFiles(string input, string path)
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

        //TestCase 1
        [InlineData("deal", true)]
        //TestCase 2
        [InlineData("dea", false)]
        //TestCase 3
        [InlineData("de", false)]
        //TestCase 4
        [InlineData("d", false)]
        //TestCase 5
        [InlineData("", false)]
        //TestCase 6
        [InlineData(null, false)]
        [Theory]
        public async void Test_InputRange(string input, bool shouldSucceed)
        {
            string fakeFileInput = "Message-ID: <13389240.1075849630582.JavaMail.evans@thyme>\nDate: Fri, 15 Dec 2000 00:27:00 -0800 (PST)\nFrom: gary.waxman @enron.com\nTo: matt.harris @enron.com, glenn.surowiec @enron.com\nSubject: FW: SAP information for your proposal\nCc: jennifer.medcalf @enron.com\nMime-Version: 1.0\nContent-Type: text/plain; charset=us-ascii\nContent-Transfer-Encoding: 7bit\nBcc: jennifer.medcalf @enron.com\nX-From: Gary Waxman\nX-To: Matt Harris, Glenn Surowiec\nX-cc: Jennifer Medcalf\nX-bcc:\nX-Folder: \\John_Arnold_Nov2001\\Notes Folders\\Sap\nX-Origin: ARNOLD-J\nX-FileName: jarnold.nsf\n\nLooks like we are dead in the water based on this message and the one Glenn got. Let's meet to review this morning.\n\n-------------------------------------------------------------\nGary Waxman\nDirector, Enterprise Group\nEnron Broadband Services\n2100 SW River Parkway\nPortland, OR 97201\nMobile: 503-807-8923\nDesk:   503-886-0196\nFax:     503-886-0441\n-------------------------------------------------------------\n----- Forwarded by Gary Waxman/Enron Communications on 12/15/00 08:32 AM -----\n\ncurtis.meyer @sap.com\n12/15/00 05:54 AM\nTo: Gary Waxman/Enron Communications@Enron Communications\ncc:\nSubject: FW: SAP information for your proposal\nGary,\nFYI....\n-----Original Message-----\nFrom: Bruder, Dietmar\nSent: Friday, December 15, 2000 6:44 AM\nTo: Meyer, Curtis\nCc: Lingner, Annett\nSubject: RE: SAP information for your proposal\nHello Curtis,\nsorry for the late response.\nAn upgrade from E1 to E3 is not an option for the mentioned locations, for\nthe foreseeable future.\nThe 2. proposal is also not an option for us, because we buy bandwidth on\ndemand and the demands change very dynamically, therefore such a plan would\nnot fit our requirements.\nbest regards Dietmar\n-----------------------\n-----Original Message-----\nFrom: Meyer, Curtis\nSent: Montag, 11. Dezember 2000 17:22\nTo: Bruder, Dietmar\nCc: Gooden, Dennis; Bannon, Michael\nSubject: FW: SAP information for your proposal\nImportance: High\n\n\nDietmar,\nAttached is a message from Enron Broadband Services on sizing the\nproposal back to SAP.\n\nHopefully his attached message is clear in terms of finding a way to\nstructure it\nfor growth based on your needs.This would increase the size of our\narrangement if it\nmakes good sense for you.Please get back to me with your thoughts.\n\n-----Original Message-----\nFrom: Gary_Waxman @enron.net[mailto:Gary_Waxman@enron.net]\nSent: Friday, December 08, 2000 2:19 PM\nTo: Meyer, Curtis\nCc: Glenn_Surowiec @enron.net; Matt_Harris @enron.net\nSubject: Re: SAP information for your proposal\n\n\n\n\nCurtis - following our phone call this morning I met with Matt Harris and\nour\nstructuring person (Glenn Surowiec) to review the deal.After some quick\nanalysis it looks like we overestimated the opportunity.\n\nThe circuits Dietmar is requesting is a european E1 which equates to a US\nT1.\nThe hub (Wallfdorf) -and-spoke (Hungary, Netherlands, UK) model SAP requires\nworks out to roughly a $2.09M deal.\n\nClearly this is smaller than both parties would prefer.Thus we would like to\nknow if there are growth plans for one or more of these connections so that\nwe\ncan we propose a larger pipe (i.e.European E3/US DS3).\n\nFor example if just one of the connections can be an E3 for the entire\ncontract\n(ten years) it would bump the value up to $8.5M. Please review this and\nlet's\nplan on talking later today.Another model to consider would be to\nprogressively\nbump up the pipe size across all the connections at set intervals(i.e.\nyears\n1-3 = E1, years 4-6 = E2, years 7-10 = E3).\n\nPlease let me know what your thoughts are and let's talk ASAP.\n\n-------------------------------------------------------------\nGary Waxman\nDirector, Enterprise Group\nEnron Broadband Services\n2100 SW River Parkway\nPortland, OR 97201\nMobile: 503-807-8923\nDesk:   503-886-0196\nFax:     503-886-0441\n-------------------------------------------------------------\n\n\n|--------+----------------------->\n|        |          curtis.meyer@|\n|        |          sap.com      |\n|        |                       |\n|        |          12/08/00     |\n|        |          09:54 AM     |\n|        |                       |\n|--------+----------------------->\n\n>---------------------------------------------------------------------------\n-|\n  |\n|\n  |       To:     Gary Waxman/Enron Communications@Enron Communications\n|\n  |       cc:     dennis.gooden @sap.com, michael.bannon @sap.com\n|\n  |       Subject:     SAP information for your proposal\n|\n\n>---------------------------------------------------------------------------\n-|";
            var tempPath = Path.GetTempPath() + Guid.NewGuid().ToString();
            await IndexTestFiles(fakeFileInput, tempPath);
            var service = new SearchService.Core.Services.SearchService(Path.Combine(tempPath, "lucene-index"));

            try
            {
                var results = service.GetSearchResults(new Core.Entities.SearchRequest { Text = input });
                if (shouldSucceed)
                    Assert.Matches("FW: SAP information for your proposal", results.Results[0].Result.Subject);
                else
                    Assert.DoesNotMatch("FW: SAP information for your proposal", results.Results[0].Result.Subject);
            }
            catch
            {
                if (shouldSucceed)
                    Assert.True(false);
                else
                    Assert.True(true);
            }
        }
    }
}
