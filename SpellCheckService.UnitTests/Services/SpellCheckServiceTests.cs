﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SpellCheckService.UnitTests.Services
{
    public class SpellCheckServiceTests
    {
        [Fact]
        public async void GetSpellingSuggestion()
        {
            #region TestData

            string fakeFileInput = @"Message-ID: <13389240.1075849630582.JavaMail.evans@thyme>
Date: Fri, 15 Dec 2000 00:27:00 -0800 (PST)
From: gary.waxman@enron.com
To: matt.harris@enron.com, glenn.surowiec@enron.com
Subject: FW: SAP information for your proposal
Cc: jennifer.medcalf@enron.com
Mime-Version: 1.0
Content-Type: text/plain; charset=us-ascii
Content-Transfer-Encoding: 7bit
Bcc: jennifer.medcalf@enron.com
X-From: Gary Waxman
X-To: Matt Harris, Glenn Surowiec
X-cc: Jennifer Medcalf
X-bcc: 
X-Folder: \John_Arnold_Nov2001\Notes Folders\Sap
X-Origin: ARNOLD-J
X-FileName: jarnold.nsf

Looks like we are dead in the water based on this message and the one Glenn 
got. Let's meet to review this morning.

-------------------------------------------------------------
Gary Waxman
Director, Enterprise Group
Enron Broadband Services
2100 SW River Parkway
Portland, OR 97201
Mobile: 503-807-8923
Desk:   503-886-0196
Fax:     503-886-0441
-------------------------------------------------------------
----- Forwarded by Gary Waxman/Enron Communications on 12/15/00 08:32 AM -----

	curtis.meyer@sap.com
	12/15/00 05:54 AM
		 
		 To: Gary Waxman/Enron Communications@Enron Communications
		 cc: 
		 Subject: FW: SAP information for your proposal



Gary,
   FYI....

-----Original Message-----
From: Bruder, Dietmar
Sent: Friday, December 15, 2000 6:44 AM
To: Meyer, Curtis
Cc: Lingner, Annett
Subject: RE: SAP information for your proposal


Hello Curtis,

sorry for the late response.
An upgrade from E1 to E3 is not an option for the mentioned locations, for
the foreseeable future.
The 2. proposal is also not an option for us, because we buy bandwidth on
demand and the demands change very dynamically, therefore such a plan would
not fit our requirements.

best regards Dietmar

-----------------------


-----Original Message-----
From: Meyer, Curtis
Sent: Montag, 11. Dezember 2000 17:22
To: Bruder, Dietmar
Cc: Gooden, Dennis; Bannon, Michael
Subject: FW: SAP information for your proposal
Importance: High


Dietmar,
   Attached is a message from Enron Broadband Services on sizing the
proposal back to SAP.
Hopefully his attached message is clear in terms of finding a way to
structure it
for growth based on your needs.  This would increase the size of our
arrangement if it
makes good sense for you.  Please get back to me with your thoughts.

-----Original Message-----
From: Gary_Waxman@enron.net [mailto:Gary_Waxman@enron.net]
Sent: Friday, December 08, 2000 2:19 PM
To: Meyer, Curtis
Cc: Glenn_Surowiec@enron.net; Matt_Harris@enron.net
Subject: Re: SAP information for your proposal




Curtis - following our phone call this morning I met with Matt Harris and
our
structuring person (Glenn Surowiec) to review the deal. After some quick
analysis it looks like we overestimated the opportunity.

The circuits Dietmar is requesting is a european E1 which equates to a US
T1.
The hub (Wallfdorf) -and-spoke (Hungary, Netherlands, UK) model SAP requires
works out to roughly a $2.09M deal.

Clearly this is smaller than both parties would prefer.Thus we would like to
know if there are growth plans for one or more of these connections so that
we
can we propose a larger pipe (i.e. European E3/US DS3).

For example if just one of the connections can be an E3 for the entire
contract
(ten years) it would bump the value up to $8.5M. Please review this and
let's
plan on talking later today. Another model to consider would be to
progressively
bump up the pipe size across all the connections at set intervals (i.e.
years
1-3 = E1, years 4-6 = E2, years 7-10 = E3).

Please let me know what your thoughts are and let's talk ASAP.

-------------------------------------------------------------
Gary Waxman
Director, Enterprise Group
Enron Broadband Services
2100 SW River Parkway
Portland, OR 97201
Mobile: 503-807-8923
Desk:   503-886-0196
Fax:     503-886-0441
-------------------------------------------------------------


|--------+----------------------->
|        |          curtis.meyer@|
|        |          sap.com      |
|        |                       |
|        |          12/08/00     |
|        |          09:54 AM     |
|        |                       |
|--------+----------------------->

>---------------------------------------------------------------------------
-|
  |
|
  |       To:     Gary Waxman/Enron Communications@Enron Communications
|
  |       cc:     dennis.gooden@sap.com, michael.bannon@sap.com
|
  |       Subject:     SAP information for your proposal
|

>---------------------------------------------------------------------------
-|";
        
            #endregion

            var tempPath = Path.GetTempPath() + Guid.NewGuid().ToString();

            await IndexTestFiles(fakeFileInput, tempPath);

            var service = new SpellCheckService.Core.Services.SpellCheckService(Path.Combine(tempPath, "lucene-index"));

            var results = service.GetSpellings(new Core.Entities.Spellings.Request { Text = "Deal" });

            Assert.NotEmpty(results.spellings);
            Assert.Equal("deal", results.spellings[0].ToLower());
        }

        private async Task IndexTestFiles(string input, string path)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            using (FileStream stream = new FileStream(path + @"\" + Guid.NewGuid().ToString() + ".txt", FileMode.Create))
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
    }
}
