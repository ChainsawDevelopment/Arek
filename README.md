# Arek 
Bot that keeps an eye on your pull requests!

## Project structure

*Core projects* 
 - `arek-engine` - library containing core functionality of notification engine, should be as extensible as possible and should not contain any specific 3rd party implementation (i.e. no GitLab, Bitbucket, Slack etc).
 - `arek-contracts` - shared interfaces used to extend Arek's functions.
 - `arek-console-core` - .NET Core console used to run the app.

*Extension projects*
 - `arek-bitbucket` - Implements BitBucket client for Arek to retrieve pull requests.
 - `arek-gitlab` - Implements GitLab client for Arek to retrieve pull requests.
 - `arek-jira` - Implements JIRA client for Arek to retrieve ticket statuses.
 - `arek-slack` - Implements Slack client for Arek to send out the notifications.
 - `arek-rocketchat` - Implements Rocketchat client for Arek to send out the notifications.
 
## Arekfile

|Property | Type | Description | Example |
|---|---|---|---|
| `primaryReviewers`  | string | Comma-separated list of logins. At least one person from this list will always be a reviewer | `dtrump,aduda`  |
| `secondaryReviewers`  | string | Comma-separated list of logins. This list contains rest of the team. When choosing secondary reviewers, Arek will construct list of all team members (primary + secondary) and randomly choose from this list. It means that it's possible that noone from `secondaryReviewers` will be choosen. | `bobama,bkomorowski` |
| `rules` | object | Key-value map containing configuration for review rules | |


## Roadmap

- Bitbucket is currently not working properly - it's needs to be retested and fixed.
- VCS has a possibility to suggest preferred reviwer. In bitbucket it takes them from reviewers list. However the bitbucket implementation uses random. It should use lastAssignments to equalize number of assignments.
- Add support for GitLab issues.
- BugTrackerStatusRequestRule can be split so that there can be separate rule on missing ticket status.
- Arekfile could be a js file containing function for both configuring existing rules and defining custom rules.