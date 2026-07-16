# Agile User Stories & Backlog

Upon user request, the application must be capable of replacing specific words or entire phrases with symbols or symbolic notation. These mappings are defined by the user in Markdown files using a key-value list format (for example: m>=Scorpio and m&=Virgo, where m> represents Scorpio and m& represents Virgo). The system must then use these symbols interchangeably within the content, substituting the defined symbols for the corresponding words or phrases.

## Epic 1: Gamified Interface & Interaction
**US-1.1: Language Flag Toggle**
- **As a** language learner,
- **I want** to click a prominent button featuring Polish and English colored flags in the Unity UI,
- **So that** I can instantly switch the application's base language and translation context.
- **Acceptance Criteria**: Clicking the Polish flag changes all UI text and AI prompts to Polish. Clicking the English flag reverts it. Context switches emit a secure state-change event to the backend.

**US-1.2: Pronunciation Practice (TTS/STT)**
- **As a** student studying Japanese or Chinese,
- **I want** to press a button to hear the text read aloud (TTS) and then use my microphone to speak it back (STT),
- **So that** I can practice and verify my pronunciation with the AI.
- **Acceptance Criteria**: Unity captures microphone input, securely sends it to the backend for transcription, and returns an accuracy score based on the gamification logic.

## Epic 2: The Karpathy-Style Knowledge Graph
**US-2.1: Markdown Wiki Drop**
- **As a** content creator or teacher,
- **I want** to paste a Markdown file containing vocabulary (e.g., Kanji, translations) into a specific directory,
- **So that** the application automatically digests it and creates new playable game levels.
- **Acceptance Criteria**: The Django LangGraph worker detects the new `.md` file, sanitizes it, extracts the nodes, and updates the active graph database without manual database entry.

## Epic 3: Secure 2-Player Communication
**US-3.1: Zero-Trust AI Translation Chat**
- **As a** user chatting over a public, untrusted Wi-Fi network,
- **I want** to communicate securely with a remote partner speaking a different language,
- **So that** the AI translates our conversation in real-time without exposing our true identities or allowing hackers to intercept the data.
- **Acceptance Criteria**: Connection is established via Post-Quantum Key Exchange (ML-KEM). The AI agent translates messages inside a secure enclave. Neither user knows the other's real identity.

**US-3.2: Chat History Export**
- **As a** language student,
- **I want** to securely save the full bilingual history of my conversation,
- **So that** I can review the vocabulary and translations later for educational purposes.
- **Acceptance Criteria**: The session history is saved locally on the device (or securely retrieved from the backend), encrypted with a local post-quantum symmetrical key.
