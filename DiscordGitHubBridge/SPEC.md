# Discord → GitHub Forum Bridge

Technical Specification · C# / .NET 10

**Status:** Proposed V1
**Purpose:** Automatically create a GitHub issue from selected newly-created Discord Forum threads and maintain a durable link between the Discord thread and GitHub issue.

> Converted from `Discord-GitHub-Bridge.docx`. See `PLAN.md` for the implementation plan.

## 1. Goals

- Listen for new Discord Forum threads.
- Process only configured forum channels.
- Create issues only when the thread matches configured tags/rules.
- Copy the thread title and starter post into GitHub.
- Map Discord forum tags to GitHub labels.
- Include a backlink to the Discord thread in the GitHub issue.
- Reply to the Discord thread with the newly-created GitHub issue link.
- Store a Discord Thread ID ↔ GitHub Issue mapping to prevent duplicates.
- Use narrowly-scoped credentials and production-grade logging/error handling.

## 2. Non-goals for V1

- Mirroring every Discord reply into GitHub.
- Mirroring every GitHub comment into Discord.
- Automatically closing Discord threads when GitHub issues close.
- AI classification or summarization.
- Full moderation/triage workflow.
- Multi-repository routing unless explicitly configured later.

## 3. Recommended Stack

- .NET 10
- C#
- .NET Worker Service / Generic Host
- Discord.Net
- GitHub REST API via Octokit.NET or typed HttpClient
- EF Core + SQLite
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Logging
- GitHub App authentication for production

## 4. High-level Flow

Discord Forum → Discord.Net Gateway → ThreadCreated event → Rule/Tag Filter → Starter Message Fetch → Issue Factory → GitHub API → Save Mapping → Reply to Discord

## 5. V1 Workflow

1. Discord creates a new forum thread.
2. Bot receives `ThreadCreated`.
3. Confirm parent channel is configured.
4. Check whether the thread has a tag/rule that should create an issue.
5. Check persistence for the Discord Thread ID.
6. Fetch the starter message.
7. Convert Discord tags to configured GitHub labels.
8. Build GitHub issue title/body.
9. Create the GitHub issue.
10. Persist the Discord/GitHub mapping.
11. Reply to the Discord thread with the GitHub issue link.
12. Log success/failure with correlation identifiers.

## 6. Suggested Project Structure

```
DiscordGitHubBridge/
├── Program.cs
├── appsettings.json
├── Configuration/
│   └── BridgeOptions.cs
├── Discord/
│   ├── DiscordService.cs
│   └── ForumThreadHandler.cs
├── GitHub/
│   ├── GitHubService.cs
│   └── IssueFactory.cs
├── Mapping/
│   ├── TagMapper.cs
│   ├── ThreadIssueMapping.cs
│   └── MappingRepository.cs
└── Data/
    └── BridgeDbContext.cs
```

## 7. Configuration

Configuration must support:

- GitHub owner/repository.
- Allowed Discord Forum channel IDs.
- Required or ignored Discord tags.
- Discord tag → GitHub label mappings.
- Bot behavior such as posting confirmation back to Discord.
- Secrets supplied through environment variables/user secrets, never committed.

See `appsettings.example.json`.

## 8. Data Model

```csharp
public sealed class ThreadIssueMapping
{
    public ulong DiscordThreadId { get; set; }
    public long GitHubIssueNumber { get; set; }
    public string GitHubIssueUrl { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
```

`DiscordThreadId` should be unique. This is the primary idempotency guard.

## 9. Issue Format

**Title:** Discord thread title

**Body template:**

```
{starterMessage}

---
Reported by: @{discordAuthor}
Source: Discord
Discord thread: {discordThreadUrl}
```

Mapped Discord tags are applied as GitHub labels.

## 10. Filtering / Triage

Do not blindly mirror an entire community forum. V1 should support explicit creation rules.

An optional safer mode is to require a `Track on GitHub` or `Confirmed` tag before issue creation.

## 11. Discord Requirements

Bot must:

- Connect to Discord Gateway.
- See configured forum channels.
- Read thread/starter-message content.
- Read applied forum tags.
- Send a message into the created thread.

Subscribe to the appropriate Discord.Net thread-created event and validate the parent channel before processing.

## 12. GitHub Requirements

Production authentication should use a GitHub App with the minimum repository permissions needed, primarily:

- Metadata: Read
- Issues: Read & Write

Install the GitHub App only on repositories that the bridge needs.

## 13. Reliability

**Idempotency.** Before creating an issue, query the mapping store using `DiscordThreadId`. If a mapping exists, stop processing.

**Retry.** Transient GitHub/API failures should use bounded retry with exponential backoff. Do not retry permanent validation/authentication failures indefinitely.

**Partial failure.** If GitHub issue creation succeeds but persistence or the Discord confirmation fails, recovery must avoid creating a second GitHub issue. Log the GitHub issue number/URL immediately after creation.

**Logging.** Include:

- Discord thread ID
- Discord forum/channel ID
- GitHub repository
- GitHub issue number when available
- operation/result
- exception details without secrets

## 14. Security

- Never commit Discord bot tokens or GitHub private keys.
- Use environment variables, .NET user-secrets locally, and the deployment platform's secret store in production.
- Use least-privilege Discord permissions.
- Use a GitHub App instead of a developer PAT for production.
- Sanitize/validate generated issue content as required.
- Do not allow Discord content to alter repository/owner configuration.

## 15. Acceptance Criteria

V1 is complete when:

- A qualifying Discord Forum thread creates exactly one GitHub issue.
- A non-qualifying thread creates no issue.
- Issue title matches the Discord thread title.
- Starter message appears in the issue body.
- GitHub issue contains the Discord backlink.
- Configured Discord tags become expected GitHub labels.
- Discord receives the GitHub issue link.
- Reprocessing the same thread does not create another issue.
- Restarting the service preserves mappings.
- API/authentication failures are logged without crashing the worker.
- Secrets are external to source control.

## 16. Tests

Minimum automated coverage:

- Allowed vs disallowed forum.
- Qualifying vs ignored tag.
- Tag-to-label mapping.
- Issue body construction.
- Existing mapping prevents duplicate.
- Multiple tags map correctly.
- Missing starter message is handled.
- GitHub failure does not write a false successful mapping.

Integration test: Test Discord server/forum → test GitHub repository → verify issue and backlink.

## 17. V2 Backlog

- GitHub issue closed/reopened → Discord status message.
- Discord tag changes → GitHub label changes.
- `/github create`, `/github sync`, `/github unlink` moderator commands.
- Moderator triage/confirmation workflow.
- Multi-repository routing.
- GitHub webhook endpoint.
- Selective comment synchronization.
- Metrics/health endpoint.
- Deployment dashboard/alerts.

## 18. Rough Effort

For an experienced .NET developer or agent-assisted build:

- Functional V1: roughly 2.5–4 hours.
- Production hardening, deployment, permissions, integration testing: additional 2–4 hours depending on infrastructure and Discord/GitHub setup.

These are planning estimates, not guarantees.
