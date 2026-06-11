# Quickstart: Document Upload and Management (Local)

Prerequisites:

- .NET 10 SDK installed
- SQLite drivers available (EF Core SQLite provider)

Steps:

1. Checkout the feature branch (or work on `main` for local experiments):

```bash
git checkout -b 001-document-upload-management
```

2. Ensure the `DefaultConnection` in `appsettings.json` points to `DataSource=ContosoDashboard.db` (already configured for SQLite in this training repo).

3. Add EF Core migration and update database:

```bash
dotnet tool install --global dotnet-ef # if not installed
dotnet ef migrations add AddDocumentEntities --project ContosoDashboard/ContosoDashboard.csproj
dotnet ef database update --project ContosoDashboard/ContosoDashboard.csproj
```

4. Run the app locally:

```bash
dotnet run --project ContosoDashboard/ContosoDashboard.csproj
```

5. Open the app in a browser at `https://localhost:5001` or the URL shown in the terminal and navigate to `/documents` (page added by feature).

Notes:

- Uploaded files are stored in `AppData/uploads` (or configured path). Ensure this directory is writable.
- For CI, add a headless integration test to verify upload/download flows using an in-memory filesystem or test uploads directory.

---
