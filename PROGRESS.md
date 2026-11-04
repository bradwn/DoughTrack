# DoughTrack progress

Updated 2026-11-04 (evening), deadline 2026-11-07.

## Verified by reading the source (not by running it)
| Module | Status | Evidence |
|---|---|---|
| Login | Implemented | frmLogin.vb, PBKDF2 hashing, UserRepository |
| Main shell / navigation | Implemented | frmMain.vb (sidebar, content host, logout) |
| Dashboard | Implemented | DashboardView.vb, DashboardRepository.vb. Today's Expense already reads `expenses` |
| Menu Items + recipes | Implemented | MenuItemsView, frmMenuItemEdit, RecipeEditor, repositories |
| Sales | Implemented but BLOCKED | Needs migration 002 |
| Sales History | Implemented | SalesHistoryView, frmSaleDetails |
| Inventory Stocks | WAS A PLACEHOLDER. Now written (this pass), uncompiled | StocksView, frmInventoryItemEdit, InventoryItemRepository |
| Stock-In | WAS A PLACEHOLDER. Now written, uncompiled | StockInView, StockInRepository (transaction) |
| Low Stock | Now written, uncompiled | LowStockView |
| Expenses (Prompt 8) | Written, uncompiled | ExpensesView, frmExpenseEdit, ExpenseRepository |

Developer reported Rebuild succeeded (0 errors) on the Cursor code before these files were added.

## Files added/replaced this pass (copy into the project, same relative paths under DoughTrack/DoughTrack/)
- Helpers/StockFormat.vb (new)
- Data/InventoryItemRepository.vb (replaced; `GetStocks` removed, `GetLowStock` keeps its columns and adds `Status`)
- Data/StockInRepository.vb (replaced; `GetStockIns` removed)
- Data/ExpenseRepository.vb (replaced; `GetExpenses` removed)
- Forms/StocksView.vb, frmInventoryItemEdit.vb, StockInView.vb, LowStockView.vb, ExpensesView.vb, frmExpenseEdit.vb (new)
- Forms/frmMain.vb (replaced; only the four Show... methods changed, LoadList removed)

## Design decisions
- Inventory items are never deleted; Deactivate/Reactivate.
- Unit of an item is locked once a recipe or stock-in uses it.
- Stock-In converts the entered unit to the item's unit (kg<->g, L<->ml), updates quantity, and sets unit_cost to a weighted average, all in one transaction.
- Stock status: Out of Stock (qty<=0), Low (qty<=reorder), Normal, Inactive.
- Expense delete is a real delete (nothing references expenses).
- Display codes ING-001 / SUP-001 are computed, not stored.

## Remaining
1. Run migration 002 (root, Workbench).
2. Build; fix compile errors if any.
3. Run tests in TESTING_RESULTS.md and report results.
4. Prompt 9 integration pass (sales deduction, rollback, dashboard match, history snapshot).
5. Cleanup list in KNOWN_ISSUES.md; decide on Settings page.
