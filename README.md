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
 - `notifier-rocketchat` - Implements Slack client for notifier to send out the notifications
 
# Roadmap

- VCS has a possibility to suggest preferred reviwer. In bitbucket it takes them from reviewers list. However the bitbucket implementation uses random. It should use lastAssignments to equalize number of assignments