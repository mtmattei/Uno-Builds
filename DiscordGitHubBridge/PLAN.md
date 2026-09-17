# Discord → GitHub Forum Bridge — Implementation Plan

Source spec: `SPEC.md` (converted from `Discord-GitHub-Bridge.docx`).
Status: plan for V1, ready to implement.

## Assumptions

- The bridge lives in `DiscordGitHubBridge/` at the repo root, following the one-folder-per-project convention used by `vtrack/`, `matrix/`, etc.
- Target is `net10.0`. The `.NET 10.0.112` SDK is available and NuGet is reachable from the build environment.
- One Discord guild, one GitHub repository for V1 (multi-repo routing stays in V2).
- Target guild: `1182775715242967050`. Target forum channels (provided 2026-09-17):
  - https://discord.com/channels/1182775715242967050/1547279690664771605
  - https://discord.com/channels/1182775715242967050/1549796979914313788
  - https://discord.com/channels/1182775715242967050/1549786519886368869
- Tag names and label maps in the config example are placeholders until the real forum tags are known.
- Forum tags are configured by **name**, matched case-insensitively. Tag IDs are more stable but hurt readability; renames are a documented limitation.
- The build environment cannot reach Discord or GitHub gateways, so the integration test is a manual runbook step. Unit tests run with `dotnet test`.

## Verified facts (from the packages, 2026-09-17)

| Item | Value | Note |
|---|---|---|
| Discord.Net | 3.20.1 | Ships a `net10.0` target. |
| Octokit | 14.0.0 | `netstandard2.0`, fine on net10. |
| EF Core SQLite | 10.0.12 | Stable line matching the SDK. |
| Microsoft.Extensions.Hosting | 10.0.12 | Worker host. |
| Polly.Core | 8.8.0 | Bounded retry with backoff. |
| `BaseSocketClient.ThreadCreated` | `Func<SocketThreadChannel, Task>` | Fires on creation **and** when the bot is added to a thread. Expect a double fire per forum post. |
| `SocketThreadChannel.AppliedTags` | `IReadOnlyCollection<ulong>` | Tag IDs; resolve names via `SocketForumChannel.Tags`. |
| `SocketThreadChannel.ParentChannel` | `SocketGuildChannel` | Cast to `SocketForumChannel` to confirm it is a forum. |
| Starter message | `thread.GetMessageAsync(thread.Id)` | The starter message ID equals the thread ID. Can be `null` briefly after the event. |
| `IGitHubAppsClient.CreateInstallationToken(long)` | present | Requires a client authenticated with an App JWT. |
| `IIssuesClient.Create(owner, repo, NewIssue)` | present | `NewIssue.Labels` carries label names. |

Docs sites (`docs.discordnet.dev`, `octokitnet.readthedocs.io`) are blocked from this environment. Facts above were read from the package XML docs.

## Architecture Brief

**App structure.** One Worker Service project plus one xUnit test project, central package management like the rest of the repo.

```
DiscordGitHubBridge/
├── DiscordGitHubBridge.sln
├── Directory.Build.props            # Nullable, ImplicitUsings, CPM, NoWarn (same shape as vtrack/)
├── Directory.Packages.props
├── global.json                      # sdk 10.0.100, rollForward latestFeature
├── .gitignore
├── PLAN.md / SPEC.md / README.md
├── DiscordGitHubBridge/
│   ├── Program.cs                   # Host builder, DI, options validation, Migrate() on start
│   ├── appsettings.json             # Non-secret defaults only
│   ├── appsettings.example.json     # Full documented shape
│   ├── Configuration/
│   │   ├── BridgeOptions.cs         # Forums[], GitHub, Discord, Behavior
│   │   └── ForumRuleOptions.cs      # ChannelId, RequiredTags, IgnoredTags, LabelMap, DefaultLabels
│   ├── Discord/
│   │   ├── DiscordGatewayService.cs # BackgroundService: login, intents, event wiring, reconnect logging
│   │   ├── ForumThreadHandler.cs    # ThreadCreated → ForumThreadSnapshot → BridgeProcessor
│   │   └── ForumThreadSnapshot.cs   # Plain record; the only Discord shape the core sees
│   ├── GitHub/
│   │   ├── IGitHubIssueClient.cs    # CreateIssueAsync(NewIssueRequest) → CreatedIssue
│   │   ├── OctokitIssueClient.cs    # Octokit + Polly retry
│   │   ├── GitHubCredentialProvider.cs # PAT or App (JWT → installation token, cached until expiry)
│   │   └── IssueFactory.cs          # Pure: snapshot + labels → title/body
│   ├── Bridge/
│   │   ├── BridgeProcessor.cs       # The V1 workflow, steps 3–12 of the spec
│   │   ├── ThreadRuleEvaluator.cs   # Pure: forum allowed? tags qualify? → labels
│   │   └── ProcessOutcome.cs        # Created / SkippedNotConfigured / SkippedRule / AlreadyMapped / Failed
│   ├── Mapping/
│   │   ├── ThreadIssueMapping.cs    # Entity
│   │   ├── IMappingRepository.cs
│   │   └── MappingRepository.cs
│   └── Data/
│       ├── BridgeDbContext.cs
│       └── Migrations/
└── DiscordGitHubBridge.Tests/
    ├── ThreadRuleEvaluatorTests.cs
    ├── IssueFactoryTests.cs
    ├── BridgeProcessorTests.cs      # Fakes for IGitHubIssueClient, IDiscordReplier, repository
    └── MappingRepositoryTests.cs    # SQLite in-memory (DataSource=:memory:)
```

**Boundary rule.** Discord.Net socket types never leave `Discord/`. `ForumThreadHandler` snapshots the thread into `ForumThreadSnapshot` (thread id, guild id, parent channel id, title, applied tag names, starter content, attachment URLs, author display name, thread URL). Everything downstream is plain C# and unit-testable without a gateway. Same on the GitHub side: `IGitHubIssueClient` is the only thing Octokit touches.

**State model.** One table, `ThreadIssueMappings`, unique index on `DiscordThreadId`. The row is written in two phases:

1. Reserve: insert `{ DiscordThreadId, Status = Pending, CreatedAt }` before calling GitHub. A unique-constraint violation here means another event for the same thread already won, so stop.
2. Complete: after the issue is created, update `GitHubIssueNumber`, `GitHubIssueUrl`, `Status = Created`; after the Discord reply, `Status = Notified`.

This extends the spec's four-field model with a `Status` column. It is what makes the partial-failure requirement in spec §13 hold: a crash between "issue created" and "mapping saved" leaves a `Pending` row plus an error log line carrying the issue URL, and the next event for that thread stops at the reservation instead of creating a second issue. `Pending` rows older than a few minutes are logged at startup for manual review.

An in-process `ConcurrentDictionary<ulong, byte>` of in-flight thread IDs absorbs the `ThreadCreated` double fire before it reaches the database.

**Services and DI.**

| Service | Lifetime | Why |
|---|---|---|
| `DiscordSocketClient` | Singleton | One gateway connection. |
| `DiscordGatewayService` | Hosted | Owns login/logout and event subscription. |
| `BridgeProcessor`, `ThreadRuleEvaluator`, `IssueFactory` | Singleton | Stateless. |
| `IGitHubIssueClient` | Singleton | Holds the cached installation token. |
| `BridgeDbContext` | Scoped via `IDbContextFactory` | Event handlers are not request-scoped; create a context per event. |

**Data flow.** `ThreadCreated` → `ForumThreadHandler` (fire-and-forget with `Task.Run`, so the gateway is never blocked; exceptions caught and logged) → `BridgeProcessor.ProcessAsync(snapshot)` → outcome logged with a scope of `{ThreadId, ChannelId, Repo, IssueNumber?}`.

**Auth.** GitHub App is the chosen mode for the first deployment (decided 2026-09-17). `GitHub:Auth:Mode` is `App` by default; `Pat` stays available for local development only. App mode builds an RS256 JWT from `AppId` and a PEM private key (`RSA.ImportFromPem` + `SignData`, roughly 25 lines, no extra package), exchanges it for an installation token via `CreateInstallationToken(InstallationId)`, and caches the token until five minutes before expiry. PAT mode is for local development only and the README says so.

**Retry.** Polly `ResiliencePipeline` around the create-issue call: 3 attempts, exponential backoff with jitter from 2s, retry only on `RateLimitExceededException`, `AbuseException`, and `ApiException` with a 5xx status. `AuthorizationException`, `NotFoundException`, and `ApiValidationException` are terminal and logged once.

**Platform constraints.** Console host; runs on Windows or Linux; SQLite file path comes from `ConnectionStrings:Bridge` (default `Data Source=bridge.db` next to the binary). Gateway intents: `Guilds | GuildMessages | MessageContent`. `MessageContent` is privileged and must be enabled on the Developer Portal or starter messages come back empty.

**Testing approach.** Pure classes are tested directly. `BridgeProcessor` is tested with hand-written fakes (no mocking library). The repository is tested against SQLite in-memory with an open connection held for the test lifetime. Integration is a manual runbook against a throwaway Discord server and a test repo.

## Design Brief

The service has no UI. Its "surfaces" are the GitHub issue, the Discord reply, and the log stream.

**GitHub issue body** (Markdown, from spec §9, with two safety changes):

```
{starterMessage, truncated to 60 000 chars with a "[truncated]" marker}

{Attachments: one bullet per starter-message attachment URL, only if any}

---
Reported by: `@{discordAuthor}`
Source: Discord
Discord thread: https://discord.com/channels/{guildId}/{threadId}
```

- The author handle and any `@word` in the starter message are wrapped in backticks so GitHub does not notify unrelated GitHub users who share a handle.
- Title is the thread name, trimmed, capped at 256 chars (GitHub's limit).

**Discord reply** (one message, no embed needed for V1):

```
Tracked on GitHub: <issueUrl>
```

Wrapped in `<>` so Discord does not render a link preview card. Configurable via `Behavior:ReplyTemplate` with `{issueUrl}` and `{issueNumber}` placeholders; `Behavior:ReplyInThread=false` disables the reply.

**Logs.** One structured line per outcome at Information, one at Error per failure, with a logging scope carrying the correlation fields from spec §13. Secrets never appear in log messages: tokens are only read into options and never interpolated.

**Config shape** (`appsettings.example.json`):

```json
{
  "Discord": { "Token": "<env: Discord__Token>" },
  "GitHub": {
    "Owner": "unoplatform",
    "Repository": "uno",
    "Auth": {
      "Mode": "App",
      "Token": "<env: GitHub__Auth__Token, Pat mode only>",
      "AppId": 0,
      "InstallationId": 0,
      "PrivateKeyPath": "<path to .pem, or env: GitHub__Auth__PrivateKeyPem>"
    }
  },
  "Forums": [
    {
      "ChannelId": 1547279690664771605,
      "RequiredTags": [ "Track on GitHub" ],
      "IgnoredTags": [ "Duplicate", "Answered" ],
      "LabelMap": { "Bug": "bug", "Feature Request": "enhancement" },
      "DefaultLabels": [ "from-discord" ]
    },
    {
      "ChannelId": 1549796979914313788,
      "RequiredTags": [ "Track on GitHub" ],
      "IgnoredTags": [],
      "LabelMap": {},
      "DefaultLabels": [ "from-discord" ]
    },
    {
      "ChannelId": 1549786519886368869,
      "RequiredTags": [ "Track on GitHub" ],
      "IgnoredTags": [],
      "LabelMap": {},
      "DefaultLabels": [ "from-discord" ]
    }
  ],
  "Behavior": {
    "ReplyInThread": true,
    "ReplyTemplate": "Tracked on GitHub: <{issueUrl}>"
  },
  "ConnectionStrings": { "Bridge": "Data Source=bridge.db" }
}
```

Options are validated with data annotations plus `ValidateOnStart`, so a missing token or an empty `Forums` array fails fast at boot instead of at the first thread.

## Interaction Brief

**Happy path.** Thread created in a configured forum with a required tag → issue created with mapped labels → mapping saved → reply posted in the thread → Information log with issue number.

**Rule evaluation** (`ThreadRuleEvaluator`, pure):

1. Parent channel not in `Forums` → `SkippedNotConfigured`.
2. Any applied tag in `IgnoredTags` → `SkippedRule`.
3. `RequiredTags` non-empty and none applied → `SkippedRule`.
4. Labels = `DefaultLabels` ∪ `LabelMap[tag]` for each applied tag that has a mapping. Unmapped tags are dropped silently and logged at Debug.

**Empty and missing states.**

- Starter message `null` (event raced the message): retry `GetMessageAsync` 3 times at 500 ms. Still null → create the issue with body `_(Starter message unavailable at creation time.)_` and log a Warning. The spec's "missing starter message is handled" test covers this.
- Starter message has attachments but no text → body is the attachment list only.
- Thread has no applied tags and `RequiredTags` is empty → qualifies, labels = `DefaultLabels`.

**Error states.**

- GitHub terminal error (401/403/404/422) → `Failed`, `Pending` row deleted so a moderator can re-trigger by re-tagging later (V2 command) or the thread can be reprocessed after config is fixed. No Discord reply.
- GitHub transient error after retries → same as terminal, logged as Error with the retry count.
- Persistence failure after issue creation → Error log includes the issue URL; the `Pending` reservation stays so no second issue is created.
- Discord reply failure → Warning; mapping stays `Created`. The issue exists and is linked; the reply is best-effort.
- Gateway disconnect → Discord.Net reconnects on its own; `Disconnected`/`Connected` are logged. Threads created while disconnected are not replayed in V1 (documented limitation; V2 `/github sync`).

**Feedback.** The only user-visible feedback is the thread reply. Moderators can confirm state from the log or the SQLite file.

**Accessibility.** Not applicable beyond the reply being plain text with the URL visible.

**Runtime verification steps** (manual, against a test server and repo):

1. Create a forum post with the required tag → exactly one issue, correct title, body, labels, backlink; reply appears in the thread.
2. Create a post without the required tag → no issue, one Skipped log line.
3. Create a post in a non-configured forum → no issue, one Skipped log line.
4. Restart the service, re-run step 1's thread through the `Pending`-row path by deleting the issue and re-tagging → no second issue, `AlreadyMapped` logged.
5. Revoke the GitHub token → post a qualifying thread → Error logged, worker stays up, no `Created` row.
6. Confirm `git status` shows no token or PEM anywhere in the tree.

## Implementation Plan

Each step is a build-green commit. Conventional commit messages.

1. **`chore: scaffold DiscordGitHubBridge worker and test projects`** — `dotnet new worker`, `dotnet new xunit`, solution, `Directory.Build.props`, `Directory.Packages.props` (pin the versions in the table above), `global.json`, `.gitignore`, `appsettings.example.json`. Verify: `dotnet build`.
2. **`feat: add bridge options with startup validation`** — `BridgeOptions`, `ForumRuleOptions`, data annotations, `ValidateOnStart`, user-secrets id on the csproj. Verify: boot with an empty config fails with a readable message.
3. **`feat: add thread rule evaluator and issue factory`** — pure classes plus tests for allowed/disallowed forum, required/ignored tags, multi-tag label mapping, body construction, backtick escaping, truncation. Verify: `dotnet test`.
4. **`feat: add SQLite mapping store with reservation status`** — entity, `BridgeDbContext`, initial migration, repository with `TryReserveAsync`, `CompleteAsync`, `MarkNotifiedAsync`, `ReleaseAsync`. Tests on in-memory SQLite for uniqueness and status transitions. Verify: `dotnet test`, `dotnet ef migrations list`.
5. **`feat: add GitHub issue client with app auth and retry`** — `GitHubCredentialProvider` (PAT + App JWT), `OctokitIssueClient` with the Polly pipeline, terminal-vs-transient classification. Tests cover the JWT shape (header/claims) and the retry predicate; the Octokit call itself is exercised in the manual run.
6. **`feat: add bridge processor with idempotency and partial-failure handling`** — the spec workflow end to end with fakes-based tests: duplicate event, existing mapping, GitHub failure leaves no `Created` row, reply failure keeps the mapping. Verify: `dotnet test`.
7. **`feat: connect Discord gateway and forum thread handler`** — `DiscordGatewayService`, intents, `ThreadCreated` wiring, snapshot extraction with tag-name resolution, starter-message retry, reply via `SendMessageAsync`. Verify: build, then manual runbook steps 1–3 against a test server.
8. **`docs: add README with Discord and GitHub App setup and runbook`** — portal steps (bot scopes, privileged intent, channel permissions), GitHub App permissions (Metadata: Read, Issues: Read & Write), secret configuration per platform, the manual verification steps above, known limitations.
9. Optional **`chore: add Dockerfile`** if the deployment target is a container. Left out until the target is known.

Estimated effort matches spec §18: functional V1 in roughly 2.5–4 hours of agent-assisted work; hardening and the manual integration pass another 2–4 hours, mostly waiting on portal setup.

## Decisions

Decision: keep Discord.Net and Octokit out of the core by snapshotting into plain records.
Reason: socket types are not mockable and the spec's eight required tests need a fast unit-test path.
Tradeoff: one extra record type and a thin adapter per side.

Decision: reserve the mapping row before calling GitHub (adds a `Status` column to the spec's model).
Reason: it is the only way to satisfy spec §13 "partial failure must not create a second issue" without a GitHub search fallback.
Tradeoff: a crash mid-flight leaves a `Pending` row that needs a manual look; logged at startup.

Decision: hand-roll the RS256 JWT for GitHub App auth instead of adding `GitHubJwt`.
Reason: the package is at 0.0.6 and rarely updated; the JWT is three claims and one `RSA.SignData` call.
Tradeoff: about 25 lines we own and must test.

Decision: Polly.Core for retry.
Reason: spec §13 requires bounded exponential backoff, and Polly is the standard .NET way to express it with jitter.
Tradeoff: one dependency; a hand-written loop would be about the same size but without jitter or a predicate API.

Decision: tags configured by name, not ID.
Reason: readable config for moderators; IDs are only visible via the API.
Tradeoff: renaming a forum tag silently breaks its rule until config is updated. Logged at Warning when an applied tag has no name match.

## GitHub App setup checklist

Needed before the first run. Create the App at GitHub → Settings → Developer settings → GitHub Apps (org-level if the target repo is under an org).

- Permissions: Repository → Metadata: Read-only, Issues: Read and write. Nothing else.
- Webhook: off for V1 (no inbound endpoint).
- Where can this App be installed: only this account/org.
- Install the App on the target repository only.
- Collect for config: **App ID** (App settings page), **Installation ID** (from the installation URL `.../settings/installations/<id>`), and a **private key** (.pem, generated on the App page and downloaded once).
- Store the PEM outside the repo: `GitHub__Auth__PrivateKeyPath` pointing at a file, or `GitHub__Auth__PrivateKeyPem` with the key contents, via user-secrets locally and the platform secret store in production.

## Unresolved Questions

- Which GitHub repo is the target, as `owner/repo`?
- For each of the three forum channels: its name, the tag list, which tag should trigger an issue, and the tag → label map. The example config uses placeholders until then.
- Where does this run in production (Windows service, Linux systemd, container, Azure Container Apps)? Decides whether step 9 happens and where the SQLite file lives.
- Should unmapped Discord tags become GitHub labels verbatim, or be dropped as planned? Dropping is safer for label hygiene.
- Should the starter message author be linked to a GitHub account when one is known (for example via a Discord ↔ GitHub username map), or stay as plain text? Plain text for V1 unless there is a need.
