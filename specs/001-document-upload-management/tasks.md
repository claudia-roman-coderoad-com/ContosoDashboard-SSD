# Tasks: Document Upload and Management

**Input**: Design documents from `/specs/001-document-upload-management`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

## Phase 1: Setup (Shared Infrastructure)

- [x] T001 Create the feature folder structure in `specs/001-document-upload-management/`
- [x] T002 Confirm `.github/copilot-instructions.md` references `specs/001-document-upload-management/plan.md`
- [x] T003 [P] Update `ContosoDashboard/ContosoDashboard.csproj` with EF Core SQLite and local file storage dependencies
- [x] T004 [P] Add `IFileStorageService` interface and `LocalFileStorageService` implementation in `ContosoDashboard/Services/`
- [x] T005 [P] Add feature documentation placeholders for `research.md`, `data-model.md`, `quickstart.md`, and `contracts/api.md`

---

## Phase 2: Foundational (Blocking Prerequisites)

- [x] T006 Add `Document` entity to `ContosoDashboard/Data/ApplicationDbContext.cs` and model class in `ContosoDashboard/Models/Document.cs`
- [x] T007 Add `DocumentShare` entity to `ContosoDashboard/Data/ApplicationDbContext.cs` and model class in `ContosoDashboard/Models/DocumentShare.cs`
- [x] T008 Add `DocumentAudit` entity to `ContosoDashboard/Data/ApplicationDbContext.cs` and model class in `ContosoDashboard/Models/DocumentAudit.cs`
- [x] T009 Implement EF Core migration for document management entities using `dotnet ef migrations add AddDocumentManagement`
- [x] T010 Implement local file storage path strategy in `ContosoDashboard/Services/LocalFileStorageService.cs` using `AppData/uploads/{userId}/{projectId|personal}/{guid}.{ext}`
- [x] T011 Implement upload metadata validation and storage in `ContosoDashboard/Services/DocumentService.cs`
- [x] T012 Implement secure file access authorization checks in `ContosoDashboard/Services/DocumentService.cs`
- [x] T013 Add configuration for upload directories in `ContosoDashboard/appsettings.json` and `ContosoDashboard/appsettings.Development.json`
- [x] T014 [P] Define `DocumentShare` dynamic permission resolution logic in `ContosoDashboard/Services/DocumentService.cs`
- [x] T015 Add async scan queue enqueue logic to `ContosoDashboard/Services/DocumentService.cs` for Azure Queue Storage integration

---

## Phase 3: User Story 1 - Upload and organize project documents (Priority: P1)

**Goal**: Enable authenticated users to upload supported documents, provide metadata, and associate files with projects.

**Independent Test**: Upload a valid file with metadata and confirm it appears in the user's document list with correct metadata.

- [x] T016 [US1] Create upload page component `ContosoDashboard/Pages/Documents.razor`
- [x] T017 [US1] Create upload modal/component in `ContosoDashboard/Pages/Documents.razor`
- [x] T018 [US1] Implement `POST /api/documents/upload` endpoint in `ContosoDashboard/Pages/Documents.razor.cs` or API controller
- [x] T019 [US1] Implement file validation for supported types and 25 MB maximum size in `ContosoDashboard/Services/DocumentService.cs`
- [x] T020 [US1] Implement metadata persistence in `ContosoDashboard/Services/DocumentService.cs`
- [x] T021 [US1] Add success/error messaging to `ContosoDashboard/Pages/Documents.razor`
- [ ] T022 [US1] Add unit tests for upload validation in `ContosoDashboard.Tests/DocumentServiceTests.cs`
- [ ] T023 [US1] Add integration test for upload flow in `ContosoDashboard.Tests/DocumentUploadTests.cs`

---

## Phase 4: User Story 2 - Browse and search documents (Priority: P2)

**Goal**: Provide document browsing, filtering, sorting, and search for accessible documents.

**Independent Test**: Search by title or tag and verify returned documents are accessible to the user.

- [x] T024 [US2] Create `My Documents` view in `ContosoDashboard/Pages/Documents.razor`
- [ ] T025 [US2] Create project documents view in `ContosoDashboard/Pages/ProjectDetails.razor`
- [x] T026 [US2] Implement document list retrieval in `ContosoDashboard/Services/DocumentService.cs`
- [x] T027 [US2] Implement search/filter query support in `ContosoDashboard/Services/DocumentService.cs`
- [x] T028 [US2] Implement list sorting by title, date, category, and size in `ContosoDashboard/Pages/Documents.razor`
- [ ] T029 [US2] Add unit tests for search and filter logic in `ContosoDashboard.Tests/DocumentServiceTests.cs`
- [ ] T030 [US2] Add integration test for document list search in `ContosoDashboard.Tests/DocumentSearchTests.cs`

---

## Phase 5: User Story 3 - Manage and share documents (Priority: P3)

**Goal**: Enable document metadata editing, replacement, deletion, and sharing with dynamic team/project membership.

**Independent Test**: Edit a document's metadata and verify the updated metadata is visible without changing the file reference.

- [ ] T031 [US3] Implement document metadata edit UI in `ContosoDashboard/Pages/Documents.razor`
- [ ] T032 [US3] Implement metadata update endpoint in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T033 [US3] Implement file replacement workflow in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T034 [US3] Implement document deletion with authorization checks in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T035 [US3] Implement document share API in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T036 [US3] Add notification generation for shared document recipients in `ContosoDashboard/Services/NotificationService.cs`
- [ ] T037 [US3] Add unit tests for metadata edit, replace, delete, and share logic in `ContosoDashboard.Tests/DocumentServiceTests.cs`
- [ ] T038 [US3] Add integration test for share and permission behavior in `ContosoDashboard.Tests/DocumentShareTests.cs`

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final improvements, security, and documentation.

- [ ] T039 [P] Add `Recent Documents` widget to `ContosoDashboard/Pages/Index.razor`
- [ ] T040 [P] Add document count summaries to dashboard cards in `ContosoDashboard/Pages/Index.razor`
- [ ] T041 [P] Add upload and share event auditing via `DocumentAudit` in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T042 [P] Add async Azure Functions queue-triggered virus scan architecture notes to `specs/001-document-upload-management/plan.md`
- [ ] T043 [P] Add/verify documentation in `specs/001-document-upload-management/quickstart.md`
- [ ] T044 [P] Update `.github/copilot-instructions.md` to reference the feature plan and docs
- [ ] T045 [P] Perform manual UX verification and security review of document pages
- [ ] T046 [P] Final cleanup and refactor of document management code

---

## Dependencies & Execution Order

- Phase 1 tasks can begin immediately.
- Phase 2 foundational tasks block all story work until complete.
- User Story phases can proceed in parallel once foundational tasks are complete.
- Phase 6 polish can run after all user stories are implemented.

## Parallel Opportunities

- T003, T004, and T005 can run in parallel during Phase 1.
- T009, T010, T011, and T014 are parallelizable within the foundational phase.
- T016-T023, T024-T030, and T031-T038 can be assigned in parallel once foundation is done.
- T039-T046 are cross-cutting improvements that can run in parallel with story validation.
