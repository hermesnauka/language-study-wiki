# System Requirements Document (SRD)
Priorities use the MoSCoW method: **M**ust have, **S**hould have, **C**ould have, **W**on't have.
Upon user request, the application must be capable of replacing specific words or entire phrases with symbols or symbolic notation. These mappings are defined by the user in Markdown files using a key-value list format (for example: m>=Scorpio and m&=Virgo, where m> represents Scorpio and m& represents Virgo). The system must then use these symbols interchangeably within the content, substituting the defined symbols for the corresponding words or phrases.

## 1. Functional Requirements

| ID | Description | Priority | Traceability |
|----|--------------|----------|---------------|
| FR-1 | **Unity Gamification**: The frontend must be built in Unity, providing a 3D or 2D gamified experience for language learning. | Must | `unity-frontend` |
| FR-2 | **Multi-Language Support**: Must map cross-language relationships across English, Spanish, German, French, Polish, Russian, Japanese, and Chinese. | Must | `django-ai-worker` |
| FR-3 | **Real-Time AI Chat Translation**: Support 2 remote users chatting in different languages with instant, AI-driven translation. | Must | `spring-backend` / `django` |
| FR-4 | **Flag Toggle UI**: Provide an interactive UI button with colored flags to instantly toggle the interface between Polish and English. | Must | `unity-frontend` |
| FR-5 | **Markdown Wiki Ingestion**: Allow users to drop `.md` files into a directory, which the system automatically parses into a LangGraph knowledge base. | Must | `django-ai-worker` |
| FR-6 | **TTS Synthesizer**: Native Text-to-Speech to read foreign texts (e.g., Japanese, Chinese) out loud. | Must | `unity-frontend` |
| FR-7 | **STT Transcription**: Capture microphone input and transcribe it into text for AI processing and pronunciation checks. | Must | `unity-frontend` |
| FR-8 | **Cross-Language Extensibility**: Architecture must cleanly separate C#, C++, Python, and Java codebases into isolated, maintainable directories. | Must | System Architecture |

## 2. Security & S-SDLC Requirements (Post-Quantum & Zero-Trust)

| ID | Description | Priority | Traceability |
|----|--------------|----------|---------------|
| SR-1 | **Post-Quantum Cryptography (PQC)**: All network traffic must be encrypted against quantum computing threats using algorithms like ML-KEM (Kyber). | Must | `cpp-crypto-core` |
| SR-2 | **Zero-Identity Trust**: The system must authenticate users and allow secure 2-party communication without revealing PII, IP addresses, or real identities. | Must | `spring-backend` |
| SR-3 | **Anti-Spoofing & Tampering**: All audio and text packets must be cryptographically signed to prevent MITM manipulation on public networks. | Must | `cpp-crypto-core` |
| SR-4 | **Secure Chat History**: Store the full chat history of the 2-user interaction locally or securely on the backend, fully encrypted at rest. | Must | `spring-backend` |
| SR-5 | **Parser Sandboxing**: The Markdown parser must execute in an isolated sandbox to prevent RCE/XSS injections via user-uploaded Wiki files. | Must | `django-ai-worker` |

## 3. Educational Explainer Requirements
| ID | Description | Priority | Traceability |
|----|--------------|----------|---------------|
| EDU-1 | **Trust Mechanism Explainer**: The app must visually explain to users how 2-party trust is established securely without identity disclosure. | Should | `unity-frontend` |
| EDU-2 | **Decentralization Primer**: The app serves as a bridge head to a larger, decentralized educational system; it must include UI tooltips explaining this vision. | Should | `unity-frontend` |
