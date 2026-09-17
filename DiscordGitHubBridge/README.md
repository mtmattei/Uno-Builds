# Discord → GitHub Forum Bridge

Creates a GitHub issue from a Discord forum post when a moderator tags it, and links the two.

A post in a watched forum gets an issue only when it carries the trigger tag. The bridge copies the
title and starter message into the issue, maps Discord tags to GitHub labels, adds a backlink, and
replies in the thread with the issue URL. Each Discord thread maps to at most one issue, forever.

See [SPEC.md](SPEC.md) for the specification and [PLAN.md](PLAN.md) for the design and decisions.

## Requirements

- .NET 10 SDK
- A Discord bot with the Message Content privileged intent
- A GitHub App installed on the target repository

## Setup

### 1. Discord bot

At <https://discord.com/developers/applications>:

1. **New Application**, then open **Bot** and **Reset Token**. Copy the token; it is shown once.
2. Turn **Public Bot** off.
3. Under **Privileged Gateway Intents**, enable **Message Content**. Leave the others off.
   Without it the starter message arrives empty and issue bodies say so.
4. Under **Installation**, choose Guild Install with the `bot` scope and these permissions:
   View Channels, Send Messages, Send Messages in Threads, Read Message History.
   The combined permission integer is `274877975552`.
5. Open the install link and authorize the bot on the server.
6. In each watched forum, confirm the bot's role has View Channel, Send Messages in Threads and
   Read Message History. Category overrides can hide a channel from the bot.

### 2. Trigger tag

In each watched forum, create a tag named exactly `Track on GitHub` and mark it moderator-only.
A post gets an issue when the tag is present at creation or applied later by a moderator.

Tags are matched by name, case-insensitively. Renaming a tag in Discord silently stops its rule
until the configuration is updated, so the startup log names any trigger tag it cannot find.

### 3. GitHub App

At GitHub → Settings → Developer settings → GitHub Apps, create an App:

- Repository permissions: **Metadata: Read-only**, **Issues: Read and write**. Nothing else.
- Webhook: off. V1 has no inbound endpoint.
- Where can this App be installed: only this account or organization.

Install it on the target repository only. Then collect three things:

| Value | Where |
|---|---|
| App ID | The App's settings page |
| Installation ID | The number at the end of the installation URL |
| Private key | Generate on the App page; a `.pem` downloaded once |

The `.pem` never goes in the repository.

### 4. Configuration

Copy the non-secret parts of [appsettings.example.json](DiscordGitHubBridge/appsettings.example.json)
into `appsettings.json` and fill in the forum channel IDs, the repository, and the label maps.
Configure only labels that already exist in the target repository, so a typo shows up as a missing
label rather than as a new one appearing in a repository with an established label scheme.
A forum channel ID is the second number in a channel URL:
`https://discord.com/channels/<guildId>/<channelId>`.

Secrets stay outside the repository. Locally, use user-secrets from the project folder:

```powershell
cd DiscordGitHubBridge
dotnet user-secrets set "Discord:Token" "<bot token>"
dotnet user-secrets set "GitHub:Auth:AppId" "<app id>"
dotnet user-secrets set "GitHub:Auth:InstallationId" "<installation id>"
dotnet user-secrets set "GitHub:Auth:PrivateKeyPath" "C:\keys\bridge.pem"
```

In production, use the platform's secret store through environment variables. Double underscores
separate the levels: `Discord__Token`, `GitHub__Auth__AppId`, `GitHub__Auth__PrivateKeyPem`.

Every option is validated at startup, so a missing token or an empty forum list stops the worker
immediately with a message naming the setting.

### 5. Run

```powershell
dotnet run --project DiscordGitHubBridge
```

The database is created and migrated on first start.

## Configuration reference

| Setting | Meaning |
|---|---|
| `Discord:Token` | Bot token. Secret. |
| `GitHub:Owner`, `GitHub:Repository` | Where issues are created. |
| `GitHub:Auth:Mode` | `App` for production, `Pat` for local development. |
| `GitHub:Auth:AppId`, `InstallationId` | App mode identity. |
| `GitHub:Auth:PrivateKeyPath` or `PrivateKeyPem` | App private key. Secret. |
| `GitHub:Auth:Token` | Personal access token. `Pat` mode only. Secret. |
| `Forums[].ChannelId` | A watched forum channel. |
| `Forums[].RequiredTags` | One must be applied. Empty means every post qualifies. |
| `Forums[].IgnoredTags` | Any one of these blocks issue creation. |
| `Forums[].LabelMap` | Discord tag name to GitHub label name. |
| `Forums[].DefaultLabels` | Applied to every issue from this forum. |
| `Behavior:ReplyInThread` | Post the confirmation reply. Default true. |
| `Behavior:ReplyTemplate` | Supports `{issueUrl}` and `{issueNumber}`. |
| `ConnectionStrings:Bridge` | SQLite connection string. |

## How it behaves

**Duplicates.** Before calling GitHub, the bridge reserves a row keyed by the Discord thread ID.
A second event for the same thread stops at that row. This holds across restarts, because the
reservation is in the database rather than in memory.

**Partial failure.** If the issue is created but the mapping cannot be saved, the reservation stays
and the issue number is logged immediately. The thread will not get a second issue. Rows left in
`Pending` are reported at the next startup.

**Retries.** Rate limits and GitHub 5xx responses retry three times with exponential backoff and
jitter. Authentication, validation and not-found failures stop immediately, since they need a human.

**Reply failures.** If the issue is created but the thread reply fails, the mapping stays and a
warning is logged. The issue exists and carries the backlink.

**Disconnects.** Discord.Net reconnects on its own. Threads created or tagged while the bridge is
offline are not replayed. A moderator can remove and re-apply the tag to trigger them.

## Verifying a deployment

Run these against a test server and a scratch GitHub repository first, then change `GitHub:Owner`
and `GitHub:Repository` to the real target. A bridge pointed at a busy public repository on its
first run has no undo.

1. Create a forum post with the trigger tag. Expect exactly one issue with the thread title, the
   starter message, the mapped labels and the backlink, plus a reply in the thread.
2. Create a post without the tag. Expect no issue. Then apply the tag as a moderator and expect
   exactly one issue.
3. Create a post in a forum that is not configured. Expect no issue.
4. Restart the bridge and re-tag a thread that already has an issue. Expect no second issue.
5. Revoke the GitHub credentials and post a qualifying thread. Expect an error in the log, no
   mapping row, and a worker that stays up.
6. Run `git status`. Expect no token, `.pem` or `.db` file in the tree.

## Tests

```powershell
dotnet test
```

Covers forum and tag filtering, label mapping, issue body construction, duplicate prevention,
the late-tag path, GitHub failures, persistence failure after issue creation, reply failure,
concurrent events for one thread, App JWT shape, token caching, and the retry predicate.

Discord and GitHub are not contacted by any test. Steps 1 through 6 above are the integration test.

## Troubleshooting

| Symptom | Cause |
|---|---|
| Issue bodies say the starter message was unavailable | Message Content intent is off, or the bot cannot read the channel. |
| Log says a forum channel is not visible | The bot's role lacks View Channel on that channel or its category. |
| Gateway never becomes ready after 90 seconds | Wrong token, or the bot was never invited to the server. |
| Log says a forum has no tag with the trigger name | The tag was renamed or never created there. |
| A qualifying post creates no issue | Check for an ignored tag, and check the startup log for the forum's tag list. |

## Not in V1

Mirroring replies or comments in either direction, closing threads when issues close, moderator
slash commands, multi-repository routing, and a GitHub webhook endpoint. See the backlog in
[SPEC.md](SPEC.md).
