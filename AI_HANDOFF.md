# DoughTrack AI Handoff

Updated 2026-11-04 after development prep. Read `DATABASE_STRUCTURE.md` and `PROGRESS.md` with this file.

`PROJECT_REQUIREMENTS.md` was not found. The visual spec is `DESIGN_REFERENCE.md` and `Design/references/`. Follow it for every screen, not only the Dashboard. Do not invent Suppliers, Staff, or Customers. Dashboard, Sales History, Low Stock, and Settings are part of the Figma set. There is one account type, Admin, and `users` has no role column.

## Project

- DoughTrack for Doughtown Cafe. VB.NET Windows Forms. Not a web, mobile, cloud, or API app.
- Solution: `DoughTrack.slnx`, branch `master`.
- Project: `DoughTrack/DoughTrack.vbproj`.
- Framework: .NET 10 (`net10.0-windows`). Do not change it unless a later task cannot be done on this target.
- Startup object: `DoughTrack.My.MyApplication`.
- Startup form: `frmLogin`. `Form1` is a leftover connection test and temporary Create Admin button. It is not the startup form.
- Packages: `MySql.Data` 26.10.0, `Microsoft.Extensions.Configuration.Json` 10.0.14.

## Layout

```
DoughTrack.slnx
AI_HANDOFF.md
DATABASE_STRUCTURE.md
PROGRESS.md
Database/Migrations/001_create_doughtrack_dev.sql
DoughTrack/
  DoughTrack.vbproj
  ApplicationEvents.vb
  Form1.vb / Form1.Designer.vb / Form1.resx
  appsettings.json                 gitignored; still uses doughtrack_db
  appsettings.example.json         placeholder password
  Data/
    DatabaseHelper.vb              connection, TestConnection, ExecuteInTransaction
    UserRepository.vb              working user SQL
    MenuItemRepository.vb          shell
    InventoryItemRepository.vb     shell
    MenuItemIngredientRepository.vb shell
    SaleRepository.vb              shell for sales and sale_items
    StockInRepository.vb           shell for stock_ins and stock_in_items
    ExpenseRepository.vb           shell
  Forms/frmLogin.*                 working login; do not redesign it
  Helpers/PasswordHelper.vb
  Helpers/Session.vb
  Models/                          User plus one class per remaining table
  My Project/Application.myapp     MainForm = frmLogin
  Services/                        empty
  Resources/                       empty
```

New SQL belongs in the matching repository. Multi-table saves use `DatabaseHelper.ExecuteInTransaction`. Use parameters. Do not store plain passwords.

## Database

- MySQL 9.6.0, localhost, port **3306**. Do not use 33060.
- Original database: `doughtrack_db`. Do not drop, recreate, reset, or delete its data.
- App user: `Darwin`@`localhost`, DML only on `doughtrack_db`.
- Password: `appsettings.json` only.
- `doughtrack_dev` is the intended test database. It was not created here because this login cannot `CREATE DATABASE`, and `root` without a password was denied.
- Before writing feature data, run `Database/Migrations/001_create_doughtrack_dev.sql` in Workbench as an administrator, then change `Database:Name` to `doughtrack_dev`. Until that happens, keep the app on `doughtrack_db`.
- Schema, keys, checks, and the decision that no columns are missing: `DATABASE_STRUCTURE.md`.
- Do not import the OneDrive dumps. They contain `DROP TABLE`.

`users` has one admin row. Every other table was empty at inspection. This prep pass did not write to MySQL.

## Login

Reviewed, not redesigned, and not clicked in this session.

- Blank username or password: "Please enter your username and password."
- Unknown user and wrong password share "Incorrect username or password."
- Password characters are masked. Enter submits. Exit calls `Application.Exit()`.
- Success sets `Session.CurrentUser` and shows "Welcome, {FullName}."
- Database errors show "Unable to log in. Please check the database connection." and write the exception with `Debug.WriteLine`.
- Lookup SQL is parameterized. Passwords use PBKDF2 in `PasswordHelper`.

The next UI task is a main form opened from that success path. Leave the login controls where they are.

## Completed

- Connection helper and live login against `doughtrack_db`.
- User model, user repository, password hashing, session.
- Schema for menu, inventory, recipes, sales, stock-in, and expenses.
- Model classes and repository shells for those modules.
- Transaction helper ready for sales and stock-in.

## Still to build

1. Run the dev-database script and point `appsettings.json` at `doughtrack_dev`.
2. Main form after login, with logout through `Session.Logout()`.
3. Menu.
4. Inventory.
5. Recipes (`menu_item_ingredients`).
6. Stock-in, including quantity and cost updates in one transaction.
7. Sales, including recipe deduction in one transaction, refused when quantity would go below 0.
8. Expenses.

`Form1` Create Admin is temporary. The admin row already exists. Do not add another account type.

## Rules

- Keep this project.
- Keep `frmLogin` working.
- Keep `doughtrack_db`.
- Parameterized SQL only.
- No web, mobile, cloud, API, or online services.
- No Suppliers, Reports, Staff, or Customer modules.
