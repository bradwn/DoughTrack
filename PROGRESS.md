# DoughTrack progress

Updated 2026-11-04 (night). Deadline 2026-11-07.

## Module status
| Module | Status | Evidence |
|---|---|---|
| Login / Logout | Working | Developer tested (T7) |
| Main shell and sidebar | Working | Used throughout testing |
| Dashboard | Working | Sales and Expense figures matched; Low Stocks card matched |
| Menu Items + recipes | Working | Iced Coffee created, recipe persisted |
| Sales | Working | Sale of 2 deducted stock correctly; over-quantity blocked on screen |
| Sales History | Working | Sale appears; old sale unchanged after price edit |
| Inventory > Stocks | Working (core) | Add, duplicate, blank, Low status tested; see TESTING_RESULTS.md for untested items |
| Inventory > Stock-In | Working (core) | +3 L Milk = +3,000 ml; validation tested |
| Inventory > Low Stock | Working (core) | Listing and Dashboard match tested |
| Expenses | Working (core) | Add and validation tested; Dashboard updates |
| Settings (change password) | Working | Built this session; developer reported all steps worked |

## Done this session
- Migration 002 applied and verified.
- Stocks, Stock-In, Low Stock, Expenses written, built (0 errors), and core-tested.
- Settings page added: SettingsView.vb, UserRepository.UpdatePasswordHash, Settings button in frmMain.
- Insufficient-stock wording change in SaleRepository.DeductStock was given to the developer ("Insufficient stock for X."). Developer has not explicitly confirmed it was applied.

## Design decisions
- Inventory items are never deleted; Deactivate/Reactivate.
- Unit of an item is locked once a recipe or stock-in uses it.
- Stock-In converts kg/g and L/ml, updates quantity, and sets unit_cost to a weighted average in one transaction.
- Stock status: Out of Stock (qty<=0), Low (qty<=reorder), Normal, Inactive.
- Expense delete is a real delete.
- Settings: change password only (min 8 characters, current password required).

## Remaining
1. Confirm cleanup (see KNOWN_ISSUES.md).
2. Run the untested items in TESTING_RESULTS.md if time allows.
3. Recount rows in DATABASE_STRUCTURE.md after testing.
4. Final build, then submit by 2026-11-07.
