﻿using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using PreviewService.Core;
using System.Text;
using Xunit;
using FluentAssertions;

namespace PreviewService.UnitTests.Services
{
    public class PreviewServicesTests
    {
        string fakeFileInput = "Message-ID: <13389240.1075849630582.JavaMail.evans@thyme>\nDate: Fri, 15 Dec 2000 00:27:00 -0800 (PST)\nFrom: gary.waxman @enron.com\nTo: matt.harris @enron.com, glenn.surowiec @enron.com\nSubject: FW: SAP information for your proposal\nCc: jennifer.medcalf @enron.com\nMime-Version: 1.0\nContent-Type: text/plain; charset=us-ascii\nContent-Transfer-Encoding: 7bit\nBcc: jennifer.medcalf @enron.com\nX-From: Gary Waxman\nX-To: Matt Harris, Glenn Surowiec\nX-cc: Jennifer Medcalf\nX-bcc:\nX-Folder: \\John_Arnold_Nov2001\\Notes Folders\\Sap\nX-Origin: ARNOLD-J\nX-FileName: jarnold.nsf\n\nLooks like we are dead in the water based on this message and the one Glenn got. Let's meet to review this morning.\n\n-------------------------------------------------------------\nGary Waxman\nDirector, Enterprise Group\nEnron Broadband Services\n2100 SW River Parkway\nPortland, OR 97201\nMobile: 503-807-8923\nDesk:   503-886-0196\nFax:     503-886-0441\n-------------------------------------------------------------\n----- Forwarded by Gary Waxman/Enron Communications on 12/15/00 08:32 AM -----\n\ncurtis.meyer @sap.com\n12/15/00 05:54 AM\nTo: Gary Waxman/Enron Communications@Enron Communications\ncc:\nSubject: FW: SAP information for your proposal\nGary,\nFYI....\n-----Original Message-----\nFrom: Bruder, Dietmar\nSent: Friday, December 15, 2000 6:44 AM\nTo: Meyer, Curtis\nCc: Lingner, Annett\nSubject: RE: SAP information for your proposal\nHello Curtis,\nsorry for the late response.\nAn upgrade from E1 to E3 is not an option for the mentioned locations, for\nthe foreseeable future.\nThe 2. proposal is also not an option for us, because we buy bandwidth on\ndemand and the demands change very dynamically, therefore such a plan would\nnot fit our requirements.\nbest regards Dietmar\n-----------------------\n-----Original Message-----\nFrom: Meyer, Curtis\nSent: Montag, 11. Dezember 2000 17:22\nTo: Bruder, Dietmar\nCc: Gooden, Dennis; Bannon, Michael\nSubject: FW: SAP information for your proposal\nImportance: High\n\n\nDietmar,\nAttached is a message from Enron Broadband Services on sizing the\nproposal back to SAP.\n\nHopefully his attached message is clear in terms of finding a way to\nstructure it\nfor growth based on your needs.This would increase the size of our\narrangement if it\nmakes good sense for you.Please get back to me with your thoughts.\n\n-----Original Message-----\nFrom: Gary_Waxman @enron.net[mailto:Gary_Waxman@enron.net]\nSent: Friday, December 08, 2000 2:19 PM\nTo: Meyer, Curtis\nCc: Glenn_Surowiec @enron.net; Matt_Harris @enron.net\nSubject: Re: SAP information for your proposal\n\n\n\n\nCurtis - following our phone call this morning I met with Matt Harris and\nour\nstructuring person (Glenn Surowiec) to review the deal.After some quick\nanalysis it looks like we overestimated the opportunity.\n\nThe circuits Dietmar is requesting is a european E1 which equates to a US\nT1.\nThe hub (Wallfdorf) -and-spoke (Hungary, Netherlands, UK) model SAP requires\nworks out to roughly a $2.09M deal.\n\nClearly this is smaller than both parties would prefer.Thus we would like to\nknow if there are growth plans for one or more of these connections so that\nwe\ncan we propose a larger pipe (i.e.European E3/US DS3).\n\nFor example if just one of the connections can be an E3 for the entire\ncontract\n(ten years) it would bump the value up to $8.5M. Please review this and\nlet's\nplan on talking later today.Another model to consider would be to\nprogressively\nbump up the pipe size across all the connections at set intervals(i.e.\nyears\n1-3 = E1, years 4-6 = E2, years 7-10 = E3).\n\nPlease let me know what your thoughts are and let's talk ASAP.\n\n-------------------------------------------------------------\nGary Waxman\nDirector, Enterprise Group\nEnron Broadband Services\n2100 SW River Parkway\nPortland, OR 97201\nMobile: 503-807-8923\nDesk:   503-886-0196\nFax:     503-886-0441\n-------------------------------------------------------------\n\n\n|--------+----------------------->\n|        |          curtis.meyer@|\n|        |          sap.com      |\n|        |                       |\n|        |          12/08/00     |\n|        |          09:54 AM     |\n|        |                       |\n|--------+----------------------->\n\n>---------------------------------------------------------------------------\n-|\n  |\n|\n  |       To:     Gary Waxman/Enron Communications@Enron Communications\n|\n  |       cc:     dennis.gooden @sap.com, michael.bannon @sap.com\n|\n  |       Subject:     SAP information for your proposal\n|\n\n>---------------------------------------------------------------------------\n-|";
        string expected = "Looks like we are dead in the water based on this message and the one Glenn got. Let's meet to review this morning.\r\n\r\n-------------------------------------------------------------\r\nGary Waxman\r\nDirector, Enterprise Group\r\nEnron Broadband Services\r\n2100 SW River Parkway\r\nPortland, OR 97201\r\nMobile: 503-807-8923\r\nDesk:   503-886-0196\r\nFax:     503-886-0441\r\n-------------------------------------------------------------\r\n----- Forwarded by Gary Waxman/Enron Communications on 12/15/00 08:32 AM -----\r\n\r\ncurtis.meyer @sap.com\r\n12/15/00 05:54 AM\r\nTo: Gary Waxman/Enron Communications@Enron Communications\r\ncc:\r\nSubject: FW: SAP information for your proposal\r\nGary,\r\nFYI....\r\n-----Original Message-----\r\nFrom: Bruder, Dietmar\r\nSent: Friday, December 15, 2000 6:44 AM\r\nTo: Meyer, Curtis\r\nCc: Lingner, Annett\r\nSubject: RE: SAP information for your proposal\r\nHello Curtis,\r\nsorry for the late response.\r\nAn upgrade from E1 to E3 is not an option for the mentioned locations, for\r\nthe foreseeable future.\r\nThe 2. proposal is also not an option for us, because we buy bandwidth on\r\ndemand and the demands change very dynamically, therefore such a plan would\r\nnot fit our requirements.\r\nbest regards Dietmar\r\n-----------------------\r\n-----Original Message-----\r\nFrom: Meyer, Curtis\r\nSent: Montag, 11. Dezember 2000 17:22\r\nTo: Bruder, Dietmar\r\nCc: Gooden, Dennis; Bannon, Michael\r\nSubject: FW: SAP information for your proposal\r\nImportance: High\r\n\r\n\r\nDietmar,\r\nAttached is a message from Enron Broadband Services on sizing the\r\nproposal back to SAP.\r\n\r\nHopefully his attached message is clear in terms of finding a way to\r\nstructure it\r\nfor growth based on your needs.This would increase the size of our\r\narrangement if it\r\nmakes good sense for you.Please get back to me with your thoughts.\r\n\r\n-----Original Message-----\r\nFrom: Gary_Waxman @enron.net[mailto:Gary_Waxman@enron.net]\r\nSent: Friday, December 08, 2000 2:19 PM\r\nTo: Meyer, Curtis\r\nCc: Glenn_Surowiec @enron.net; Matt_Harris @enron.net\r\nSubject: Re: SAP information for your proposal\r\n\r\n\r\n\r\n\r\nCurtis - following our phone call this morning I met with Matt Harris and\r\nour\r\nstructuring person (Glenn Surowiec) to review the deal.After some quick\r\nanalysis it looks like we overestimated the opportunity.\r\n\r\nThe circuits Dietmar is requesting is a european E1 which equates to a US\r\nT1.\r\nThe hub (Wallfdorf) -and-spoke (Hungary, Netherlands, UK) model SAP requires\r\nworks out to roughly a $2.09M deal.\r\n\r\nClearly this is smaller than both parties would prefer.Thus we would like to\r\nknow if there are growth plans for one or more of these connections so that\r\nwe\r\ncan we propose a larger pipe (i.e.European E3/US DS3).\r\n\r\nFor example if just one of the connections can be an E3 for the entire\r\ncontract\r\n(ten years) it would bump the value up to $8.5M. Please review this and\r\nlet's\r\nplan on talking later today.Another model to consider would be to\r\nprogressively\r\nbump up the pipe size across all the connections at set intervals(i.e.\r\nyears\r\n1-3 = E1, years 4-6 = E2, years 7-10 = E3).\r\n\r\nPlease let me know what your thoughts are and let's talk ASAP.\r\n\r\n-------------------------------------------------------------\r\nGary Waxman\r\nDirector, Enterprise Group\r\nEnron Broadband Services\r\n2100 SW River Parkway\r\nPortland, OR 97201\r\nMobile: 503-807-8923\r\nDesk:   503-886-0196\r\nFax:     503-886-0441\r\n-------------------------------------------------------------\r\n\r\n\r\n|--------+----------------------->\r\n|        |          curtis.meyer@|\r\n|        |          sap.com      |\r\n|        |                       |\r\n|        |          12/08/00     |\r\n|        |          09:54 AM     |\r\n|        |                       |\r\n|--------+----------------------->\r\n\r\n>---------------------------------------------------------------------------\r\n-|\r\n  |\r\n|\r\n  |       To:     Gary Waxman/Enron Communications@Enron Communications\r\n|\r\n  |       cc:     dennis.gooden @sap.com, michael.bannon @sap.com\r\n|\r\n  |       Subject:     SAP information for your proposal\r\n|\r\n\r\n>---------------------------------------------------------------------------\r\n-|";

        [Fact]
        public async void GetResultPreview()
        {
            var mockFileSystem = new MockFileSystem();

            var mockInputFile = new MockFileData(fakeFileInput);

            mockFileSystem.AddFile(@"C:\temp\in.txt", mockInputFile);

            var previewService = new Core.Services.PreviewService(mockFileSystem);

            var searchResults = await previewService.GetResultPreview(new Core.Entities.ResultPreview.Request { path = new string[] { @"C:\temp\in.txt" } });

            Assert.Matches(expected, searchResults.Results[0].body);
        }

        [Fact]
        public async void GetResultPreview_FileNotFoundException()
        {
            var mockFileSystem = new MockFileSystem();

            var mockInputFile = new MockFileData(fakeFileInput);

            var previewService = new Core.Services.PreviewService(mockFileSystem);
            await Assert.ThrowsAsync<System.IO.FileNotFoundException>(async () =>
            {
                var searchResults = await previewService.GetResultPreview(new Core.Entities.ResultPreview.Request { path = new string[] { @"C:\temp\in.txt" } });
            });
        }


        //TestCase 1
        [InlineData(new string [] {@"C:\temp\in.txt" }, true)]
        //TestCase 2
        [InlineData(new string[] { @"C:\temp\in.txt", "" }, false)]
        //TestCase 3
        [InlineData(new string[] { "", @"C:\temp\in.txt" }, false)]
        //TestCase 4
        [InlineData(new string[] { "" }, false)]
        //TestCase 5
        [InlineData(new string[] { null }, false)]
        //TestCase 6
        [InlineData( null , false)]
        [Theory]
        public async void GetResultPreview_InputRange(string[] input, bool shouldSucceed)
        {
            var mockFileSystem = new MockFileSystem();

            var mockInputFile = new MockFileData(fakeFileInput);

            mockFileSystem.AddFile(@"C:\temp\in.txt", mockInputFile);

            var previewService = new Core.Services.PreviewService(mockFileSystem);

            var searchResults = await previewService.GetResultPreview(new Core.Entities.ResultPreview.Request { path = input });

            if (shouldSucceed)
                searchResults.Results.Should().NotBeEmpty().And.NotBeNull();
            else
                searchResults.Results.Should().BeNullOrEmpty();
        }
    }
}
