# Agent Status Notifier Widget

A retro-futuristic CRT-style desktop widget that monitors AI agent processes and provides visual/audio notifications when an agent completes a task or requires human input.

## Features

- **Real-time Status Display**: Shows current agent state (Idle, Working, Waiting, Finished, Error)
- **Retro CRT Aesthetic**: Skeuomorphic hardware frame with scanlines and glow effects
- **Audio Notifications**: Distinct sounds for waiting, finished, and error states
- **Claude Code Integration**: Hooks system for automatic status updates
- **Always-on-Top**: Stays visible while you work on other tasks

## Quick Start

### 1. Run the Widget

```powershell
cd AgentNotifier
dotnet run -f net10.0-desktop
```

### 2. Install Claude Code Hooks

Run the installer script to configure Claude Code to send status updates:

```powershell
.\Scripts\install-hooks.ps1
```

### 3. Restart Claude Code

Exit and restart Claude Code CLI to load the new hooks.

That's it! The widget will now automatically update when you use Claude Code.

## Manual Status Updates

You can also update the status manually using the CLI script:

```powershell
# Set to working
.\Scripts\agent-notify.ps1 -Status working -Message "Building project..."

# Set to waiting (triggers audio notification)
.\Scripts\agent-notify.ps1 -Status waiting -Message "Approve changes?"

# Set to finished
.\Scripts\agent-notify.ps1 -Status finished -Message "Build complete"

# Set to error
.\Scripts\agent-notify.ps1 -Status error -Message "Build failed"
```

## Widget Controls

- **TEST**: Cycle through status states for testing
- **SND**: Toggle audio notifications on/off
- **RST**: Reset to idle state

## Status File

The widget monitors this JSON file for status changes:

```
%APPDATA%\AgentNotifier\status.json
```

Any process can write to this file to update the widget status.

### Status File Format

```json
{
  "status": "working",
  "label": "PROCESSING",
  "message": "Task in progress...",
  "progress": 45,
  "session": {
    "id": "A7F3",
    "task": "BUILD",
    "elapsed_ms": 12000
  }
}
```

## Uninstalling Hooks

To remove the Claude Code hooks:

```powershell
.\Scripts\uninstall-hooks.ps1
```

## Project Structure

```
AgentNotifier/
├── App.xaml              # Application resources and theme
├── MainPage.xaml         # Main widget UI
├── Controls/             # Custom controls (PixelIcon, SegmentedProgress)
├── Converters/           # Value converters for status colors
├── Models/               # Data models (AgentStatus, StatusPayload)
├── Services/             # Status file watcher, audio service
├── ViewModels/           # MVVM view models
└── Scripts/
    ├── agent-notify.ps1  # CLI for manual status updates
    ├── install-hooks.ps1 # Install Claude Code hooks
    ├── uninstall-hooks.ps1
    └── hooks/            # Hook scripts for Claude Code events
```

## Requirements

- .NET 10.0 SDK
- Windows (for audio notifications via Console.Beep)
- Claude Code CLI (for automatic integration)

## Building

```powershell
dotnet build -f net10.0-desktop
```

## License

MIT
