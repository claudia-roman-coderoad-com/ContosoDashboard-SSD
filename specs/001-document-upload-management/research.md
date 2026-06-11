# Research: Document Upload and Management

## Clarifications Resolved

- Sharing semantics: dynamic team/project membership (resolved 2026-06-11).

## Technology Rationale

- SQLite chosen for local training to support ARM64 dev machines and simple migration path.
- Local file storage ensures offline capability and straightforward security controls for training.

## Alternatives Considered

- Azure Blob Storage: better for production but requires cloud dependencies; out of scope for offline training.
- Storing files in `wwwroot`: rejected for security reasons.

## Decision

- Use `IFileStorageService` abstraction with `LocalFileStorageService` implementation now; provide `AzureBlobStorageService` in future.

---
