Feature: PerformSearch
	As a user I want to be able to search for relevant emails.
	I also want to see a preview of the relevant emails, 
	as well as recieve spelling suggections for my search.


Scenario: Perform a successful search with suggestions
	Given I want to search for <searchTerm>
	When pressing search
	Then first emails subject should be <firstSubject>
	And first spelling suggestion should be <firstSuggestion>

Examples:
| searchTerm | firstSubject                          | firstSuggestion |
| Deal       | FW: SAP information for your proposal | deal            |
| Change     | FW: SAP information for your proposal | change          |



Scenario: Perform a successful search without suggestions
	Given I want to search for <searchTerm>
	When pressing search
	Then first emails subject should be <firstSubject>
	And spelling suggestions should not be found

Examples:
| searchTerm | firstSubject                          |
| hello      | FW: SAP information for your proposal |


Scenario: Perform an unsuccessful search with suggestions
	Given I want to search for <searchTerm>
	When pressing search
	Then emails should not be found
	And first spelling suggestion should be <firstSuggestion>

Examples:
| searchTerm | firstSuggestion |
| dea        | dead            |


Scenario: Perform an unsuccessful search without suggestions
	Given I want to search for <searchTerm>
	When pressing search
	Then emails should not be found
	And spelling suggestions should not be found

Examples:
| searchTerm           |
| dxydiDuenmOIadwDDWAD |