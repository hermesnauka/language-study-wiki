# unity-frontend

Gamified client for FR-1, FR-4, FR-6, FR-7 / US-1.1, US-1.2, FR-5's "Unity Sync Agent".
Talks to `spring-backend` (sessions, chat WebSocket, chat history) and directly to
`django-ai-worker` (knowledge graph export), per `AGENTS.md`.

## ⚠️ Unverified — no Unity Editor in this environment

This project was written without access to a Unity Editor: nothing here has been
opened, compiled, or Play-tested. Before trusting it:

1. Open the project in **Unity 2022.3 LTS or newer** (`ProjectSettings/ProjectVersion.txt`
   pins `2022.3.50f1` — adjust if you're on a different LTS patch).
2. Let the Package Manager resolve `Packages/manifest.json` (Newtonsoft Json, TextMeshPro,
   Test Framework, UI/UIElements/Audio/UnityWebRequest modules).
3. Watch the Console for compile errors on first open — asmdef package references
   (`Newtonsoft.Json`, `Unity.TextMeshPro`) were written from memory of Unity's
   conventions, not verified against an actual package install.
4. Run `Window > General > Test Runner > EditMode > Run All` — the tests under
   `Assets/Tests/EditMode/` cover pure C# logic (pronunciation scoring, localization,
   language toggle state, graph-to-level mapping, chat DTO JSON shape) and need no
   scene, network, or Play mode, so they're the fastest way to catch anything broken.
5. Build a scene wiring `FlagToggleView`/`LocalizedText` to real UGUI Button/Text
   components before Play-testing the flag toggle end-to-end — no scene/prefab exists
   yet, only the scripts.

## What's a real implementation vs. an interim seam

| Area | Status |
|---|---|
| Session/room REST client, chat WebSocket client, graph export client | Real HTTP/WebSocket calls against the actual spring-backend/django-ai-worker contracts |
| Flag toggle logic, localization, pronunciation scoring, graph→level mapping | Real logic, unit-tested (pending an actual Test Runner run) |
| Text-to-Speech (FR-6), Speech-to-Text (FR-7) | **Interim seam only** — `ITextToSpeechService`/`ISpeechToTextService` interfaces with no-op implementations. Vanilla Unity has no built-in cross-platform TTS/STT; wiring a real one means a native plugin per platform (Android `TextToSpeech`, iOS `AVSpeechSynthesizer`, Windows SAPI) or a cloud speech API, swapped in behind these same interfaces. |
| Scenes, prefabs, 3D/2D game visuals, level generation from `VocabLevel` | Not built — only the C# data layer (`GraphLevelBuilder`) that a level-generation scene would consume |

## Backend configuration

`Assets/Scripts/Api/BackendConfig.cs` is a `ScriptableObject` — create one via
`Assets > Create > SecureLearning > Backend Config` and point it at your running
`spring-backend` (default `http://localhost:8080` / `ws://localhost:8080`) and
`django-ai-worker` (default `http://localhost:8000`).

## Endpoint contract this client assumes

- `POST /api/rooms`, `POST /api/rooms/{roomCode}/join` — spring-backend, session creation/pairing
- `POST /api/rooms/{roomCode}/language` (header `X-Session-Id`) — added to spring-backend alongside
  this client so US-1.1's "emits a secure state-change event to the backend" is real, not just a
  client calling nothing
- `GET /api/rooms/{roomCode}/history` (header `X-Session-Id`) — spring-backend, US-3.2 export
- `ws://.../ws/chat/{roomCode}?sessionId=...` — spring-backend, real-time translated chat
- `GET /api/graph/export` — django-ai-worker, knowledge graph for level generation
