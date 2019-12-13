Feature: SpellCheckService
	As a user of the system
	I want to get similar terms suggested
	When searching for a word

Scenario: Perform a search to get spelling suggestions
	Given I search for <searchTerm>
	When I press search
	Then the first spelling suggestion should be <firstSuggestion>

Examples:
| searchTerm | firstSuggestion |
| Deal       | deal            |
