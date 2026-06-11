# Data Model: Document Upload and Management

## Entities

### Document

- `DocumentId` (int, PK)
- `Title` (string, required)
- `Description` (string, optional)
- `Category` (string, required)
- `Tags` (string, optional) — simple CSV for search; consider separate tags table if needed
- `ProjectId` (int, FK nullable)
- `UploadedBy` (string, required)
- `UploadedAt` (DateTimeOffset, required)
- `FileName` (string, original filename)
- `FilePath` (string, storage path)
- `FileType` (string, MIME type, nvarchar(255))
- `FileSize` (long)

Indexes: `Title`, `Tags`, `ProjectId`, `UploadedBy`

### DocumentShare

- `DocumentShareId` (int, PK)
- `DocumentId` (int, FK)
- `RecipientUserId` (int, FK nullable)
- `RecipientTeamId` (int, FK nullable)
- `SharedAt` (DateTimeOffset)
- `SharedBy` (string)
- `Permission` (string) — e.g., "read", "write"

Notes: Team shares are recorded as an intent; access is resolved dynamically at authorization time using current team membership.

### DocumentAudit

- `DocumentAuditId` (int, PK)
- `DocumentId` (int, FK)
- `Action` (string) — upload/download/delete/share/edit
- `PerformedBy` (string)
- `PerformedAt` (DateTimeOffset)
- `Details` (string, optional)

## Schema Notes

- `DocumentId` uses integer keys to match existing schema conventions.
- File storage path lives outside `wwwroot` and must be represented in `FilePath`.
- Keep categories as text values for simplicity and internationalization.

---
