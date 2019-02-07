Bot that keeps an eye on your pull requests!

# Project structure

*Core projects* 
 - `notifier-engine` - library containing core functionality of notification engine, should be as extensible as possible and should not contain any specific 3rd party implementation (i.e. no GitLab, Bitbucket, Slack etc)
 - `notifier-contracts` - shared interfaces used to extend Notifier's functions
 - `notifier-console-core` - .NET Core console used to run the app.

*Extension projects*
 - `notifier-bitbucket` - Implements BitBucket client for notifier to retrieve pull requests
 - `notifier-gitlab` - Implements GitLab client for notifier to retrieve pull requests
 - `notifier-jira` - Implements JIRA client for notifier to retrieve ticket statuses
 - `notifier-slack` - Implements Slack client for notifier to send out the notifications
 - `notifier-rocketchat` - Implements Rocketchat client for notifier to send out the notifications
 
# Arekfile

|Property | Type | Description | Example |
|---|---|---|---|
| `primaryReviewers`  | string | Comma-separated list of logins. At least one person from this list will always be a reviewer | `dtrump,aduda`  |
| `secondaryReviewers`  | string | Comma-separated list of logins. This list contains rest of the team. When choosing secondary reviewers, Arek will construct list of all team members (primary + secondary) and randomly choose from this list. It means that it's possible that noone from `secondaryReviewers` will be choosen. | `bobama,bkomorowski` |
| `rules` | object | Key-value map containing configuration for review rules | |

# Roadmap

- VCS has a possibility to suggest preferred reviwer. In bitbucket it takes them from reviewers list. However the bitbucket implementation uses random. It should use lastAssignments to equalize number of assignments