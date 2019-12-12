﻿Feature: SearchService
	As a user of the system
	I want to be able to find relevant emails 
	by clicking the search button

@mytag
Scenario: Perform a successful search
	When user performs a search for <searchTerm>
	Then first emails subject should be <firstSubject>

Examples:
| searchTerm | firstSubject                          |
| deal       | FW: SAP information for your proposal |
| Deal       | FW: SAP information for your proposal |
| DEAL       | FW: SAP information for your proposal |