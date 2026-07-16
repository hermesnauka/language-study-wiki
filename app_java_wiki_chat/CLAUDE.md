# Post-Quantum Gamified Language Learning Platform — Reference Implementation

Full product spec: see `requirements.md`, `user_stories.md`, and `AGENTS.md` for architecture, API, AI orchestration, threat model, and test plan details.
Upon user request, the application must be capable of replacing specific words or entire phrases with symbols or symbolic notation. These mappings are defined by the user in Markdown files using a key-value list format (for example: m>=Scorpio and m&=Virgo, where m> represents Scorpio and m& represents Virgo). The system must then use these symbols interchangeably within the content, substituting the defined symbols for the corresponding words or phrases.

## Layout
- `cpp-crypto-core/` — C++ cryptographic core using `liboqs`. Implements Post-Quantum Cryptography (PQC) algorithms (ML-KEM/Kyber and ML-DSA/Dilithium) to ensure quantum-safe data-in-transit and zero-identity trust.
- `spring-backend/` — Spring Boot 3 (Java 21), Maven. Core routing, session management, secure chat history persistence, and WebSocket gateways for real-time 2-user communication.
- `django-ai-worker/` — Python 3.12, Django + LangGraph. Handles the "Karpathy-style" Wiki ingestion (Markdown to Knowledge Graph), AI auto-translation, and agentic LLM interactions.
- `unity-frontend/` — Unity (C#). Gamified creator/student portal, 3D interactive learning, UI flag toggles (EN/PL), Text-to-Speech (TTS), and Speech-to-Text (STT) integrations.
- `docs/sdlc/` — Secure SDLC documentation set.

## Build & Test
- **Crypto Core**: `cd cpp-crypto-core && mkdir build && cd build && cmake .. && make`
- **Backend**: `cd spring-backend && mvn clean test` (unit tests only; uses mock storage/chain, no live network required). Requires Java 21.
- **AI Worker**: `cd django-ai-worker && pip install -r requirements.txt && python manage.py test`
- **Frontend**: Open `unity-frontend` in Unity Editor (2022.3 LTS or newer). Run `Play` mode for local testing.

## Orchestration & AI Knowledge Graph (Karpathy Concept)
- `app.ai.wiki.ingest` (env: `WIKI_INGEST_DIR`) = Directory monitored for new `.md` files.
- When users drop Markdown files (e.g., vocabulary lists for Japanese Kanji or Chinese Hanzi), the Django worker automatically parses them via LangGraph, generating semantic nodes and edges for the game's Knowledge Graph.

## Required Environment Variables (Backend & AI, Real end-to-end run only)
- `PQC_PRIVATE_KEY` — Seed for the post-quantum identity generation. Never commit this.
- `OPENAI_API_KEY` or `LLM_PROVIDER_KEY` — API key for the AI translation and conversation model.
- `WIKI_INGEST_DIR` — Local path or S3 bucket path for the Markdown wiki file ingestion.

## Conventions
- **Backend (Java)**: Package root `com.platform.securelearning`; Spring Boot conventions (constructor injection, `@Service`/`@RestController` layering).
- **Backend (Python)**: Django app structure, LangGraph state machines for all AI pipelines.
- **Security**: Zero-Trust architecture. Identity is hidden; trust between two communicating parties is established via ephemeral PQC keys. Data is immune to "harvest now, decrypt later" attacks.
- **Gamification**: All learning paths (nodes) are fed directly to Unity to populate interactive game levels, leveraging cross-language matrices (e.g., mapping Japanese to English to Polish).
