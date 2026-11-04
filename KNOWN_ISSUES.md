# DoughTrack known issues

Updated 2026-11-04 (night).

## Resolved
1. Migration 002 applied and verified.
2. New files compile (0 errors).
3. Settings page (change password) built and tested.

## To confirm (developer has not explicitly confirmed these)
4. SaleRepository.DeductStock message changed to "Insufficient stock for <item>." (two lines near 450 and 459). Check with Ctrl+F for "Not enough" in Data/SaleRepository.vb: it should find nothing.
5. Deleted from the project: Form1.vb, Form1.Designer.vb, Form1.resx (Form1.vb had a hardcoded admin password string), Forms/RecordListView.vb, Forms/frmExpenseDetails.vb and its `Compile Remove` line in DoughTrack.vbproj. Check Solution Explorer and the folder. Developer reported the rebuild still has 0 errors.

## Open gaps
6. Logo image, Admin dropdown/notification icon, and pagination from the Figma design are not built.
7. Figma Inventory Overview cards are not in the approved module list and were not built.
8. Menu item description from Figma has no database column and was not added.
9. Dashboard Fast Moving Items uses the current menu name, not the saved sale name.
10. Deactivating an inventory item does not block sales that use it in a recipe.
11. The SaleRepository insufficient-stock safety check was not triggered directly in testing; the Sales screen blocks over-quantity first.
12. Untested items are listed in TESTING_RESULTS.md ("Not yet run").
13. DATABASE_STRUCTURE.md row counts are from 2026-11-04 morning; recount after testing.

## Security notes
- appsettings.json is gitignored; the repo has only appsettings.example.json. Confirm with `git ls-files`.
- Passwords are PBKDF2 hashed, including when changed in Settings. All SQL uses parameters. The only dynamic SQL is the IN list in SaleRepository.LockStock, which builds parameter NAMES only.
- Do not run the Workbench dump files in OneDrive\Documents\dumps (they contain DROP TABLE).
- Never put the database or admin password in any document.
