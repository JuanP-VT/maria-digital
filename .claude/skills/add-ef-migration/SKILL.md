---
name: add-ef-migration
description: Add an EF Core migration after changing a domain entity or AppDbContext. Use when the database model changes.
---

# Add an EF Core migration

1. Change the entity in `backend/MariaDigital/Domain/` and, if needed, `Infrastructure/Persistance/AppDbContext.cs`. Keep entity rules (validation, state changes) in small methods that can be unit tested.
2. From `backend/`, run:
   ```bash
   dotnet tool restore
   dotnet ef migrations add <PascalCaseName> --project MariaDigital
   ```
   Name it after the change (`AddStudentBirthDate`), not the date; EF adds the timestamp.
3. Read the generated migration. Check for unintended drops, renames shown as drop+add, and missing defaults for new non-null columns on existing rows.
4. Never hand-edit `*.Designer.cs` or `AppDbContextModelSnapshot.cs`. If the migration is wrong, remove it with `dotnet ef migrations remove --project MariaDigital` and redo it.
5. Add or update tests for the behavior the new column or table supports. Integration tests run on SQLite via `TestApp`; if the change relies on Postgres-only behavior, stop and tell the user.
6. Do not run `dotnet ef database update` against a real database unless the user asks.
7. Run `dotnet test MariaDigital.slnx -c Release` from `backend/`.
