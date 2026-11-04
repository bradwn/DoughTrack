# DoughTrack testing results

Last updated: 2026-11-04 (evening). Rule: a test is PASS only if it was executed and the result was observed. Claude (chat assistant) cannot run Visual Studio or MySQL, so every UI and database test below is **NOT EXECUTED** until the developer runs it and reports the result.

## Tests actually executed so far

| # | What was tested | Expected | Actual | Result | Executed? |
|---|---|---|---|---|---|
| E1 | Rebuild Solution on the Cursor project, BEFORE the Inventory/Stock-In/Expenses files were added | 0 errors | "1 succeeded, 0 failed" (reported by developer) | PASS | Yes (by developer) |
| E2 | Code audit of unzipped source (reading only) | Find what is implemented | Stocks, Stock-In, Low Stock and Expenses were read-only placeholder grids; Sales needs migration 002 | Finding recorded in KNOWN_ISSUES.md | Yes (static reading, not a runtime test) |

## Tests NOT yet executed (run these after copying the new files and building)

Setup: run `002_sale_cogs_snapshot.sql` as root first (see PROGRESS.md). Use the test data in the "Test data" section.

### Build
| # | Test | Expected | Actual | Result | Executed? |
|---|---|---|---|---|---|
| B1 | Rebuild Solution with the new files | 0 errors | - | - | NO |

### Inventory > Stocks
| # | Test | Expected | Actual | Result | Executed? |
|---|---|---|---|---|---|
| I1 | Add item "Milk", Ingredients, ml, qty 5000, reorder 1000, cost 0.10 | Row appears as ING-00n, Normal | - | - | NO |
| I2 | Add item with a duplicate name | Message: item already exists | - | - | NO |
| I3 | Add item with blank name | Message: Item name is required | - | - | NO |
| I4 | Edit quantity to 800 (below reorder 1000) | Status becomes Low | - | - | NO |
| I5 | Edit quantity to 0 | Status Out of Stock | - | - | NO |
| I6 | Change the unit of an item used in a recipe | Unit box locked; repository also refuses | - | - | NO |
| I7 | Deactivate, then Reactivate an item | Status changes; row stays in DB | - | - | NO |
| I8 | Search by name; filter Category; filter Status | Grid narrows correctly | - | - | NO |

### Inventory > Stock-In
| # | Test | Expected | Actual | Result | Executed? |
|---|---|---|---|---|---|
| S1 | Milk 5000 ml on hand. Stock-In +3 L @ 100/L | Line shows +3,000 ml; saved; Milk = 8,000 ml | - | - | NO |
| S2 | Check `stock_ins` and `stock_in_items` in Workbench | 1 header, 1 line, total_cost 300.00 | - | - | NO |
| S3 | Quantity 0 and click Add Item | Message: greater than zero | - | - | NO |
| S4 | Save with no lines | Message: add at least one item | - | - | NO |
| S5 | Future date | Date picker blocks it; repository also refuses | - | - | NO |
| S6 | Unit cost updates as weighted average | 5,000 ml @0.10 + 3,000 ml @0.1 stays 0.1000 | - | - | NO |
| S7 | Force a failure mid-save (e.g. stop MySQL) | Friendly message; no partial rows | - | - | NO |

### Inventory > Low Stock
| # | Test | Expected | Actual | Result | Executed? |
|---|---|---|---|---|---|
| L1 | Item with qty <= reorder | Listed with status Low | - | - | NO |
| L2 | Item with qty 0 | Listed with status Out of Stock | - | - | NO |
| L3 | Inactive item below reorder | NOT listed | - | - | NO |
| L4 | Dashboard Low Stocks card matches this list | Same items | - | - | NO |

### Expenses
| # | Test | Expected | Actual | Result | Executed? |
|---|---|---|---|---|---|
| X1 | Add expense: today, Rent, 500, blank description | Saved | - | - | NO |
| X2 | Amount 0 | Message: Amount must be greater than zero | - | - | NO |
| X3 | Blank category | Message: Category is required | - | - | NO |
| X4 | Edit amount; View details; Delete with confirm | Each works; delete asks first | - | - | NO |
| X5 | Search text, category filter, From/To dates, Clear Filters | Grid and total update | - | - | NO |
| X6 | Dashboard Today's Expense after adding a today expense | Equals sum of today's rows | - | - | NO |
| X7 | Expense dated yesterday | Not counted in Today's Expense | - | - | NO |

### Sales and integration (Prompt 9, not started)
| # | Test | Expected | Actual | Result | Executed? |
|---|---|---|---|---|---|
| T1 | Sale of 2 Iced Coffee deducts 2x each recipe line | Quantities drop correctly | - | - | NO |
| T2 | Sale with insufficient stock | Blocked, nothing saved | - | - | NO |
| T3 | Sale rolls back on failure | No sales row, no deduction | - | - | NO |
| T4 | Sales History shows the sale and its snapshot name | Correct | - | - | NO |
| T5 | Edit menu price/recipe after a sale | Old sale unchanged | - | - | NO |
| T6 | Dashboard sales, gross profit, fast moving match DB | Match | - | - | NO |
| T7 | Login: correct, wrong, blank; Logout returns to login | As expected | - | - | NO |

## Test data (create through the app, not by SQL)
Inventory: Coffee Beans (g), Milk (ml), Sugar (g), Cup (pcs), Straw (pcs). Menu item: Iced Coffee 120.00 with recipe 20 g beans, 250 ml milk, 10 g sugar, 1 cup, 1 straw.
