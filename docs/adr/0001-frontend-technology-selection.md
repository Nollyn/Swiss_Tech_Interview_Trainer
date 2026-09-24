# ADR 0001: Frontend Technology Selection - Blazor Server & Interactive Components

## Status
Accepted

## Context
"Swiss Tech Interview Trainer" requires a rich, reactive user interface with syntax highlighting, live evaluation feedback progression (e.g. "Analyzing SOLID principles...", "Calculating deterministic scores..."), code diff rendering ("before/after"), code editing / upload capabilities, and real-time category progress tracking.

The target candidate profile is a Senior .NET Developer / Tech Lead in the Swiss market. The team must choose between:
1. **Blazor Interactive Components (.NET 10)** (Server/Wasm Interactive)
2. **Single Page Application (SPA)** with Angular/React and a separate REST API backend.

## Decision
We select **Blazor (.NET 10)** as the primary UI technology for the application.

### Justification & Trade-offs
1. **Full-Stack C# / .NET 10 Synergy**:
   - Sharing domain models, DTOs, and validation schemas directly between frontend and backend eliminates translation drift and boilerplate.
   - Reinforces the authentic Swiss enterprise tech lead profile where modern .NET full-stack proficiency (including modern Blazor patterns) is highly prized.
2. **Real-time UX with Low Complexity**:
   - Blazor Server / Interactive components provide seamless UI state updates during multi-stage evaluation processes without needing complex separate client-side state stores (NgRx/Redux).
3. **Productivity and Maintainability**:
   - Reduces build pipeline complexity in containerized deployment (`docker-compose up` builds a single unified runtime container or simple multi-stage build without requiring separate Node.js/npm toolchains in production).
4. **Offline / Fast Iteration**:
   - Markdown rendering (Markdig) and code diffs are processed directly with high-performance .NET libraries.

## Consequences
- **Positive**: Single codebase, rapid feature evolution, zero API contract synchronization overhead for internal views, high performance on .NET 10 runtime.
- **Negative/Mitigation**: Requires WebSocket connection for interactive server mode; handled gracefully via resilient connection lifecycles and fallback REST/Minimal API endpoints.
