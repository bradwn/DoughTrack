# DoughTrack

Desktop cafe monitoring system for Doughtown Cafe (VB.NET Windows Forms, .NET 10, MySQL).

## Run
1. Install MySQL, create `doughtrack_db` and the 9 tables (see DATABASE_STRUCTURE.md).
2. Run `DoughTrack/Database/Migrations/002_sale_cogs_snapshot.sql` as an administrator.
3. Copy `DoughTrack/appsettings.example.json` to `appsettings.json` and fill in the database password (never commit it).
4. Open `DoughTrack.slnx` in Visual Studio 2026, press F5, log in with the admin account.

See AI_HANDOFF.md, PROGRESS.md, KNOWN_ISSUES.md, TESTING_RESULTS.md.
