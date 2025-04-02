Feature: User Management

Scenario: Creating a new user
Given I have a clean PostgreSql database
When I create a user with name "Sebastian Villalba" and email "sv@example.com"
Then The user should exist in the database
And The user should have the correct details