# Nativefy

**A serverless, local-first, anonymous YouTube client for Windows.**

Nativefy is a native Windows application built with C# and WinUI 3. It brings YouTube content discovery, music and video playback, playlist management, and offline media access into one desktop experience.

Unlike services that depend on their own cloud infrastructure and user accounts, Nativefy is designed to operate without a Nativefy backend. Its application data and personal media organization are handled locally on the user's device.

## Core Features

### 🔎 YouTube Search

Discover videos and playlists through YouTube search without requiring a personal YouTube account for the core experience.

- Search for videos and playlists.
- Retrieve search results asynchronously.
- Distinguish between playable media and playlists.
- Deduplicate results using their respective IDs.
- Convert YouTube search results into Nativefy's internal media models.

### 📰 Local Feed

A locally maintained content feed that gives users a place to discover and revisit content.

- Feed data is managed on the user's device.
- No Nativefy cloud backend is required.
- Designed to complement YouTube search and provide a more integrated discovery experience.

### ▶️ Audio and Video Playback

Nativefy provides an integrated media experience for listening to music and watching videos.

- Play YouTube media directly through the application.
- Resolve audio streams for playback.
- Support playlist-based playback.
- Maintain playback state through a centralized playback controller.
- Provide playback controls for play, pause, previous, next, shuffle, and repeat.
- Display media information such as titles, channels, and thumbnails.

### 📥 Offline Media

Nativefy is designed to support downloading media for local access.

- Keep downloaded media on the user's device.
- Access supported downloaded content without an internet connection.
- Integrate offline media into the local media experience.

### 📚 Local Playlists

Manage personal media collections locally.

- Create and manage playlists.
- Save online playlists locally.
- Like and organize individual media items.
- Maintain a dedicated Liked Media collection.
- Store playlist and liked-media information in a local SQLite database.

### 🔒 Privacy and Architecture

Nativefy follows a serverless, local-first approach.

- No Nativefy-owned backend servers.
- No Nativefy account system.
- No Nativefy cloud-based user profile.
- Local database for application data.
- Local media organization and playlist management.
- Core discovery and playback do not require signing into a personal YouTube account.

Nativefy still communicates with YouTube to search for content and retrieve media. Serverless describes Nativefy's own infrastructure; it does not mean the application operates without network communication.

---

## Technology Stack

| Technology | Purpose |
|---|---|
| C# | Primary programming language |
| .NET 10 | Application runtime |
| WinUI 3 | Native Windows UI |
| Windows App SDK | Windows application platform |
| YoutubeExplode | YouTube search and media stream resolution |
| Entity Framework Core | Database access |
| SQLite | Local data persistence |
| CommunityToolkit.Mvvm | MVVM infrastructure |
| Microsoft.Extensions.DependencyInjection | Dependency injection |
| Windows Media APIs | Media playback infrastructure |

## Architecture

Nativefy is separated into multiple projects to keep application presentation, media functionality, and supporting infrastructure independent.

### Nativefy

The main Windows application.

Responsible for:
- User interface and navigation.
- ViewModels and application state.
- Dependency injection and service registration.
- Local database initialization.
- Playlist and liked-media management.
- Playback coordination.

### Nativefy.Engine

The media engine responsible for YouTube-related operations.

Its responsibilities include:

- Searching for YouTube videos and playlists.
- Converting search results into Nativefy media models.
- Resolving audio stream URLs.
- Providing media-related functionality to the application.

### Nativefy.Test

The testing project for Nativefy.

### CrashReporter

A separate application responsible for handling application crash reports.

---

## Playback Pipeline

Nativefy uses a centralized playback architecture.

```text
            YouTube
               |
        Search / Playlist
               |
               v
       Nativefy.Engine
               |
               v
       PlaybackController
               |
               v
          MediaLoader
               |
       +-------+-------+
       |               |
       v               v
  Audio Media      Video Media
       |               |
       +-------+-------+
               |
               v
        Media Playback
```

### Progressive Playlist Loading

Nativefy's playback architecture supports progressive loading.

Instead of resolving every playlist item before playback begins:

1. The first playable item is resolved.
2. It is added to the playback list.
3. Playback can begin immediately.
4. Remaining playlist items are resolved asynchronously.
5. Successfully resolved items are progressively added to the playback list.

This helps reduce the delay between selecting a playlist and starting playback.

Individual media-loading failures are handled so that one unavailable item does not necessarily prevent the remaining playlist from loading.

---

## Local Data Storage

Nativefy uses SQLite through Entity Framework Core.

The database is stored under:

```text
%LOCALAPPDATA%\Nativefy\nativefy.db
```

The application initializes its database and applies migrations during startup.

Local data includes:
- Liked media.
- Saved playlists.
- Search-related data.

This allows Nativefy to maintain user-managed application data without requiring its own remote database.

---

## Project Structure

```text
Nativefy/
├── Nativefy/
│   ├── Views/
│   ├── ViewModels/
│   ├── Models/
│   ├── Services/
│   ├── Controls/
│   └── ...
│
├── Nativefy.Engine/
│   ├── SearchEngine.cs
│   ├── StreamingEngine.cs
│   └── ...
│
├── Nativefy.Test/
│
├── CrashReporter/
│
└── Nativefy.slnx
```

---

## Design Philosophy

Nativefy is built around a few principles:

**1. Privacy first**

The application does not depend on its own backend or account infrastructure.

**2. Local-first**

Personal organization and application data belong on the user's device.

**3. Native performance**

Built with C# and WinUI 3 rather than wrapping a web application inside a desktop shell.

**4. Unified media experience**

Search, discovery, playlists, audio playback, video playback, and offline media belong in one application.

**5. Simplicity**

No Nativefy account, no Nativefy cloud service, and no unnecessary infrastructure.

---

## Building from Source

### Requirements

- Windows
- Visual Studio
- .NET 10 SDK
- Windows App SDK

### Build

Clone the repository:

```bash
git clone https://github.com/boba1010/Nativefy.git
```

Open `Nativefy.slnx` in Visual Studio and build the solution.

---

**Nativefy — Your media, your device, your experience.**