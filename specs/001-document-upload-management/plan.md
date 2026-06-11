# Implementation Plan: Document Upload and Management

**Branch**: `001-document-upload-management` | **Date**: 2026-06-11 | **Spec**: ../spec.md

## Summary

Implement local document upload and management for ContosoDashboard to provide secure, searchable storage and sharing. This plan uses the existing Blazor Server app, local file storage behind authorization endpoints, and SQLite for metadata storage.

## Technical Context

**Language/Version**: C# / .NET 10
**Primary Dependencies**: ASP.NET Core, Entity Framework Core (SQLite provider), xUnit (tests)
**Storage**: SQLite for metadata; local filesystem for file blobs (AppData/uploads)
**Testing**: xUnit unit tests for services; integration tests for upload/download flows
**Target Platform**: Cross-platform (dev on macOS/ARM64 and Intel; runtime on Windows/Linux)
**Project Type**: Web application (Blazor Server)
**Performance Goals**: Upload <30s for 25MB; list/search <2s for 500 documents
**Constraints**: Offline-first for training, no cloud dependencies; DocumentId must be integer; Category stored as text

## Constitution Check

GATE: This plan follows the ContosoDashboard constitution (Spec-Driven Development, Security by Design, Service-Layer Validation). All features reference `specs/001-document-upload-management/spec.md` and clarify sharing semantics (dynamic team membership). Proceeding.

## Project Structure (this feature)

```
specs/001-document-upload-management/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── api.md
└── checklists/
    └── requirements.md
```

## Implementation Phases

### Phase 0: Research & Clarifications (complete)

- Resolved sharing semantics: dynamic team membership (document shared to team resolves access dynamically).
- No remaining NEEDS CLARIFICATION markers.

### Phase 1: Design & Data Model

- Create entities and EF Core migrations:
  - `Document` (int DocumentId, string Title, string Description, string Category, string Tags, int? ProjectId, string FileName, string FilePath, string FileType, long FileSize, DateTimeOffset UploadedAt, string UploadedBy)
  - `DocumentShare` (int DocumentShareId, int DocumentId, int? RecipientUserId, int? RecipientTeamId, DateTimeOffset SharedAt, string SharedBy, string Permission) — note: team shares resolve dynamically
  - Indexes: Title, Tags, ProjectId, UploadedBy
- Implement `IFileStorageService` with `LocalFileStorageService` using path `{userId}/{projectId|personal}/{guid}.{ext}` stored outside `wwwroot` (e.g., `AppData/uploads`)

### Phase 2: Implementation

- Add EF Core migrations and update `ApplicationDbContext` with `DbSet<Document>` and `DbSet<DocumentShare>`
- Implement `DocumentService` (business logic): upload, validate file, save blob, save metadata, search, share, edit metadata, delete
- Implement `LocalFileStorageService` and interface contract to support future Azure Blob swap
- Implement endpoints/components:
  - Upload page (`Documents.razor`) + upload modal
  - API endpoints for download/preview with authorization checks
  - Project documents view and "My Documents" view
  - Dashboard widget for Recent Documents
- Notifications: hook into `NotificationService` for share notifications
- Add async virus scanning workflow:
  - Upload handler saves file metadata and file blob immediately, then enqueues a scan request to Azure Queue Storage
  - Azure Function `DocumentVirusScanner` triggers on queue messages, downloads the file from storage, scans it, and updates the document record with scan status
  - If the file fails the scan, mark the document as quarantined and notify the uploader; optionally delete the blob after quarantine
  - Keep the upload flow responsive by returning success for storage receipt and pending scan status

### Phase 3: Tests & QA

- Unit tests for `DocumentService` and `LocalFileStorageService`
- Integration tests for upload/download/search flows
- Manual verification: permissions, previews, large-file rejection

### Phase 4: Documentation & Merge

- Add `quickstart.md`, update ADRs if necessary, update `.github/copilot-instructions.md` anchor to reference this plan
- Create PR referencing `specs/001-document-upload-management/spec.md` and `plan.md`

## Rollout

- Feature branch `001-document-upload-management` → PR → code review verifying constitution compliance
- Merge after tests pass

## Rollback

- Remove database migrations and file blobs created during feature if rollback needed; include cleanup script for uploads

## Open Questions

- For local training, should the Azure Function/queue-based scanning flow be simulated or documented as an architecture-only design?
- Decide whether quarantined files are deleted immediately or retained for admin review.
- Virus scanning implementation is out of scope for offline training; document a migration plan for integrating an antivirus scanner in cloud deployments.

---
