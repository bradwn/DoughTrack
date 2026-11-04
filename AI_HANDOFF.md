# DoughTrack AI handoff

Updated 2026-11-04 (night). Deadline 2026-11-07. Read PROJECT_REQUIREMENTS.md, DATABASE_STRUCTURE.md, PROGRESS.md, KNOWN_ISSUES.md, TESTING_RESULTS.md with this file.

## Overview
VB.NET Windows Forms, net10.0-windows, MySql.Data 26.10.0, Microsoft.Extensions.Configuration.Json. Solution `DoughTrack.slnx`; project `DoughTrack/DoughTrack.vbproj`; startup form `frmLogin`. MySQL 9.6 at localhost:3306, database `doughtrack_db`, app user `Darwin` (SELECT/INSERT/UPDATE/DELETE only; cannot ALTER). Password lives only in gitignored `appsettings.json`. One Admin user.

## Structure
Data (DatabaseHelper, *Repository modules), Models, Forms (views are UserControls hosted by frmMain; edit dialogs are Forms), Helpers (UiTheme, UnitConverter, StockFormat, PasswordHelper, Session, MenuImageStore), Database/Migrations (001, 002).

## Patterns to follow
- Repositories are VB Modules in `Namespace Data`, using `Param`, `DecimalParam`, `FillTable`, `ExecuteInTransaction`. Parameterized SQL only.
- Views: UserControl built in code, white card on gray background (see MenuItemsView, SettingsView).
- Validation: throw InvalidOperationException with user-ready text; forms show that text, and show a generic friendly message plus Debug.WriteLine for other exceptions. Never show raw database errors.

## Status
All approved modules plus Settings (change password) are built and core-tested by the developer. Build: 0 errors. Migration 002 applied. See TESTING_RESULTS.md for exactly what was and was not run, and KNOWN_ISSUES.md for items to confirm (wording change, file deletions).

## Next steps
1. Confirm the two "to confirm" items in KNOWN_ISSUES.md.
2. Run the "Not yet run" tests if time allows (rollback, Out of Stock, inactive items, filters, expense edit/delete).
3. Recount rows in DATABASE_STRUCTURE.md; final Rebuild Solution.

## Do not
Drop/recreate the database, add Suppliers/Reports/Staff/Customers, add web/cloud/API features, concatenate user input into SQL, put passwords in documents, or mark a test PASS without the developer reporting it.
