# DoughTrack known issues

Updated 2026-11-04.

## Blocking
1. **Migration 002 not applied.** `Database/Migrations/002_sale_cogs_snapshot.sql` must be run as root in Workbench. Until then Complete Sale refuses to save (by design) and Sales History labels items "Item <id>". Darwin cannot ALTER tables.
2. **New files not yet compiled.** Inventory Stocks, Stock-In, Low Stock, Expenses files were written without a compiler available. Expect possible small compile errors; send the Error List text.

## Gaps against the specification / Figma
3. **Settings page** (Figma: username + change password) is not built and is not in the module list. Sidebar has no Settings item. Ask the owner whether it is required.
4. **Sidebar is plain text** (no Doughtown logo image) and does not show the Admin dropdown/notification icon from Figma. The header shows the user's full name only.
5. **Figma Inventory Overview** (Total Items / Low Stock / Out of Stock cards) is not in the spec's three-tab list, so it was not built.
6. **Pagination** (Showing 1-7 of 7) from Figma is not implemented; grids scroll.
7. Menu item **description** field from Figma does not exist in the approved database (no column). Not added.
8. Error text for insufficient stock in `SaleRepository.DeductStock` reads "Not enough <item> to complete this sale." The spec example is "Insufficient stock for <item>." Align wording during Prompt 9.
9. Dashboard "Fast Moving Items" uses the current menu name, not the sale snapshot name.
10. Deactivating an inventory item does not block sales that use it in a recipe (SaleRepository ignores inventory status). Decide if that is acceptable.

## Cleanup
11. `Form1.vb` / `Form1.Designer.vb` / `Form1.resx` are the leftover temporary test form (not the startup form). `Form1.vb` contains a hardcoded admin password string. Delete these three files from the project.
12. `Forms/RecordListView.vb` is no longer used after this change. It can be deleted.
13. `DATABASE_STRUCTURE.md` lists row counts as of 2026-11-04 morning; recount after testing.
14. Earlier chat guidance told the developer to create a separate throwaway project (Steps 1-6 in the chat). The Cursor project is the real one; the chat project can be discarded.

## Security notes
- `appsettings.json` is gitignored; the repo only has `appsettings.example.json`. Confirm with `git ls-files`.
- Passwords are PBKDF2 hashed. All SQL found so far uses parameters. The only dynamic SQL is the IN list in `SaleRepository.LockStock`, which builds `@id0, @id1...` parameter NAMES only (values are parameters).
- Do not run the Workbench dump files in OneDrive\Documents\dumps (they contain DROP TABLE).
