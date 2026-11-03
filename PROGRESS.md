# DoughTrack progress

Updated 2026-11-04 after the development-prep pass.

## Checked

- `AI_HANDOFF.md` from the earlier inspection.
- `PROJECT_REQUIREMENTS.md`: not found in the repo or under the user profile.
- `DATABASE_STRUCTURE.md`: did not exist. It was created from the live schema.
- VB.NET project: .NET 10 Windows Forms, startup form `frmLogin`, connection in `DatabaseHelper`.
- Live MySQL 9.6. Application port is 3306. Port 33060 is X Protocol and is not the app port.
- `Darwin`@`localhost` has SELECT, INSERT, UPDATE, DELETE on `doughtrack_db` only.
- `doughtrack_db` still has the nine original tables. The earlier count was 1 user and 0 rows in every other table. This pass did not write to that database.
- Login code: blank-field check, one message for unknown user and bad password, PBKDF2 verify, parameterized lookup, startup form still `frmLogin`. No functional defect found in review. The form layout was not changed. The success path still shows the welcome message because there is no main form yet.
- The login screen was not opened or clicked in this session.

## Changed

- `Database/Migrations/001_create_doughtrack_dev.sql`: safe script for a separate dev database. Not executed. `doughtrack_dev` does not exist yet.
- `DatabaseHelper.ExecuteInTransaction`: commit on success, rollback and rethrow on failure. Existing connection methods are unchanged.
- Model classes for menu, inventory, recipe lines, sales, stock-in, and expenses.
- Empty repository modules for those same areas. `UserRepository` is unchanged.
- Removed an unused import in `Session.vb`. Session behavior is unchanged.
- `appsettings.example.json` password is now `your-password`. Live `appsettings.json` still points at `doughtrack_db` on port 3306.
- `DATABASE_STRUCTURE.md`, this file, and `AI_HANDOFF.md`.

## Unchanged on purpose

- Target framework `net10.0-windows`.
- `frmLogin` layout and login flow.
- `Form1` connection-test and temporary Create Admin button.
- `doughtrack_db` tables and rows.
- No Suppliers, Reports, Staff, or Customer modules.

## Ready next

1. Run `Database/Migrations/001_create_doughtrack_dev.sql` in MySQL Workbench as an administrator, then set `Database:Name` in `appsettings.json` to `doughtrack_dev`.
2. Build the main form and open it from a successful login.

Screen order after that: Menu, Inventory, Recipes, Stock-in, Sales, Expenses. Sales and stock-in need `ExecuteInTransaction` so inventory quantity changes with the document.

## Verified this session

- MySQL accepted `Darwin` on `127.0.0.1:3306` and reported `@@port` 3306, version 9.6.0.
- `SHOW GRANTS` shows DML on `doughtrack_db` only.
- `CREATE DATABASE` was not possible with this login. `root` with no password was denied.
- `dotnet build DoughTrack.slnx` succeeded with 0 warnings and 0 errors. The login screen was not run.
