# AI Agents & LangGraph Orchestration Blueprint

## 1. System Overview
This module (`django-ai-worker/`) handles the intelligence layer of the platform. It utilizes LangGraph to create autonomous agents that process language data, translate real-time chat, and build the dynamic educational Knowledge Graph.
Upon user request, the application must be capable of replacing specific words or entire phrases with symbols or symbolic notation. These mappings are defined by the user in Markdown files using a key-value list format (for example: m>=Scorpio and m&=Virgo, where m> represents Scorpio and m& represents Virgo). The system must then use these symbols interchangeably within the content, substituting the defined symbols for the corresponding words or phrases.

## 2. The Karpathy-Style Wiki Ingestion Pipeline
Inspired by Andrej Karpathy's concepts of semantic knowledge mapping, the platform allows users to simply drop Markdown (`.md`) files into a designated directory.

### Agent Workflow:
1. **File Watcher Agent**: Monitors the `WIKI_INGEST_DIR` for new `.md` files.
2. **Sanitization Agent**: Strips malicious code (XSS, script injections) to adhere to S-SDLC principles.
3. **Extraction Agent (LLM)**: Reads the raw text and extracts linguistic concepts.
   - *Example*: Identifies Japanese Kanji, reads furigana, and maps the English/Polish translations.
4. **Graph Constructor Agent**: Links extracted nodes into a NetworkX/Neo4j graph.
   - *Edges*: Connects related words, grammar rules, and difficulty progressions.
5. **Unity Sync Agent**: Serializes the graph into JSON payloads consumed by the Unity frontend to generate gamified levels dynamically.

## 3. Real-Time Translation & Chat Agent
When two remote users communicate in different languages (e.g., User A speaks Polish, User B speaks Japanese):
- **E2EE Intercept**: The chat is end-to-end encrypted via Post-Quantum Cryptography.
- **Secure Enclave Processing**: The AI agent operates within a secure memory enclave. It decrypts the message, translates it using contextual LLMs, re-encrypts it with the recipient's public key, and forwards it.
- **Audio Processing**:
  - **STT (Speech-to-Text)**: Listens to microphone input and converts it to raw text.
  - **TTS (Text-to-Speech)**: Synthesizes translated text into spoken audio (e.g., native Chinese or Japanese voiceovers) to aid pronunciation practice.

## 4. Security & Zero-Trust Metadata
- The AI Agent does **not** know the real-world identities of User A or User B.
- It only processes ephemeral session IDs and transient state data, ensuring that even if the AI backend is compromised, no PII or historical identity logs can be leaked.
