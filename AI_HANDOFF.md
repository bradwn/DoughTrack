# DoughTrack AI handoff

Updated 2026-11-04 (evening). Read PROJECT_REQUIREMENTS.md, DATABASE_STRUCTURE.md, PROGRESS.md, KNOWN_ISSUES.md, TESTING_RESULTS.md with this file.

## Overview
VB.NET Windows Forms, net10.0-windows, MySql.Data 26.10.0, Microsoft.Extensions.Configuration.Json. Solution `DoughTrack.slnx`; project `DoughTrack/DoughTrack.vbproj`; startup form `frmLogin`. MySQL 9.6 at localhost:3306, database `doughtrack_db`, app user `Darwin` (SELECT/INSERT/UPDATE/DELETE only; cannot ALTER or CREATE). Password lives only in gitignored `appsettings.json`. Admin user `admin` exists.

## Folder structure (project)
Data (DatabaseHelper, *Repository modules), Models, Forms (views are UserControls hosted by frmMain; edit dialogs are Forms), Helpers (UiTheme, UnitConverter, StockFormat, PasswordHelper, Session, MenuImageStore), Database/Migrations.

## Patterns to follow
- Repositories are VB Modules in `Namespace Data`, using `Param`, `DecimalParam`, `FillTable`, `ExecuteInTransaction` from DatabaseHelper. Parameterized SQL only.
- Views: UserControl, white card on gray background, toolbar on top, DataGridView with button columns, message label at bottom (see MenuItemsView).
- Validation messages: throw InvalidOperationException with user-ready text; forms show ex.Message for that type and a generic message plus Debug.WriteLine for others.

## Status
See PROGRESS.md. Summary: Login, shell, Dashboard, Menu+recipes, Sales, Sales History existed from Cursor. Inventory Stocks, Stock-In, Low Stock and Expenses were placeholders and were written in this pass but NOT compiled or run (no compiler available to the assistant).

## First steps for the next assistant
1. Ask the developer for the Error List after building the new files; fix compile errors by editing only the named files.
2. Confirm migration 002 was run (`SHOW COLUMNS FROM sales LIKE 'cogs_amount'`).
3. Work through TESTING_RESULTS.md with the developer; record only observed results.
4. Do Prompt 9 integration items and the KNOWN_ISSUES.md cleanup.

## Do not
Drop/recreate the database, add Suppliers/Reports/Staff/Customers, add web/cloud/API features, concatenate user input into SQL, or claim a test passed without an observed result.
