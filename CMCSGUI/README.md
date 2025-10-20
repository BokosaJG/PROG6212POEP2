
# CMCSGUI - Updated for Part 2 requirements

This branch contains updates to satisfy the PROG6212 Part 2 checklist:
- In-memory / JSON storage of claims (`App_Data/claims.json`).
- MVC-only ASP.NET Core app (no WPF).
- File upload with encryption (AES) stored in `App_Data/uploads`. Files are encrypted with a generated key stored in `App_Data/key.bin`.
- Allowed file types: `.pdf`, `.docx`, `.xlsx`. Max size: 5 MB.
- Lecturer Create form allows uploading supporting documents and entering hours, rate, notes.
- Claims progress/status (Submitted, Verified, Approved, Rejected) and simple progress bar on index.
- Download action decrypts file on the fly.
- Simple unit tests added (xUnit + Moq) under `Tests/`.
- Updated Views for Create, Index, Details.
- No authentication/roles implemented (requirement allows this).

## How to run
1. Ensure you have .NET 9 SDK installed (project targets net9.0).
2. From project folder, run:
   ```
   dotnet run --project CMCSGUI.csproj
   ```
3. To run tests:
   ```
   dotnet test Tests/CMCSGUI.Tests.csproj
   ```

## Notes and limitations
- For assignment purposes, an AES key is generated once and stored in `App_Data/key.bin`. In production, a secure key store should be used.
- File downloads are streamed decrypted; files are not left decrypted on disk.
- The UI focuses on clarity and minimal styling; please adapt CSS in `wwwroot/css/site.css` as needed.
- Video link: (please record an unlisted YouTube demo and paste the URL here)

## What I changed (summary)
- Implemented `Data/ClaimStore.cs`, `Services/FileService.cs`.
- Reworked `Controllers/ClaimsController.cs`.
- Updated Views in `Views/Claims/`.
- Added unit tests and test project.
