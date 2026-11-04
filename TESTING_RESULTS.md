# DoughTrack testing results

Last updated: 2026-11-04 (night). Rule: a test is PASS only if the developer ran it and reported seeing the result. The assistant cannot run Visual Studio or MySQL. "Reported" means the developer said it worked, without giving more detail.

## Setup and build
| # | What | Result | Notes |
|---|---|---|---|
| E1 | Rebuild on the Cursor code before new files | PASS | 0 errors (developer) |
| M1 | Migration 002_sale_cogs_snapshot.sql run as root in Workbench | PASS | Verified: sales.cogs_amount, sale_items.menu_item_name, sale_items.cogs_amount, chk_sales_cogs, chk_si_cogs exist. Needed `SET SQL_SAFE_UPDATES = 0` first. |
| B1 | Rebuild with all new files | PASS | 0 errors (developer) |
| B2 | Rebuild after Settings page and cleanup | PASS | 0 errors (developer) |

## Inventory > Stocks
| # | Test | Result | Notes |
|---|---|---|---|
| I1 | Add Coffee Beans, Milk, Sugar, Cup, Straw | PASS | Reported |
| I2 | Duplicate name rejected | PASS | Reported |
| I3 | Blank name rejected | PASS | Reported |
| I4 | Qty below reorder level shows Low | PASS | Reported |
| I5 | Qty 0 shows Out of Stock | NOT EXECUTED | |
| I6 | Unit locked when used by a recipe | NOT EXECUTED | |
| I7 | Deactivate / Reactivate | NOT EXECUTED | |
| I8 | Search and filters | NOT EXECUTED | |

## Inventory > Stock-In
| # | Test | Result | Notes |
|---|---|---|---|
| S1 | +3 L Milk raises Milk by 3,000 ml | PASS | Reported |
| S2 | Check stock_ins / stock_in_items rows in Workbench | NOT EXECUTED | |
| S3 | Quantity 0 rejected | PASS | Reported |
| S4 | Save with no lines rejected | PASS | Reported |
| S5 | Future date blocked | NOT EXECUTED | |
| S6 | Weighted average cost | NOT EXECUTED | |
| S7 | Failure mid-save leaves no partial rows | NOT EXECUTED | |

## Inventory > Low Stock
| # | Test | Result | Notes |
|---|---|---|---|
| L1 | Item at or below reorder level is listed | PASS | Reported |
| L2 | Qty 0 listed as Out of Stock | NOT EXECUTED | |
| L3 | Inactive item not listed | NOT EXECUTED | |
| L4 | Dashboard Low Stocks matches Low Stock list | PASS | Reported (step D) |

## Expenses
| # | Test | Result | Notes |
|---|---|---|---|
| X1 | Add expense (today, Rent, 500) | PASS | Reported |
| X2 | Amount 0 rejected | PASS | Reported |
| X3 | Blank category rejected | PASS | Reported |
| X4 | Edit, View details, Delete with confirmation | NOT EXECUTED | |
| X5 | Search, category and date filters, Clear Filters | NOT EXECUTED | |
| X6 | Dashboard Today's Expense updates (500.00) | PASS | Reported |
| X7 | Yesterday's expense not counted today | NOT EXECUTED | |

## Menu, Sales, integration
| # | Test | Result | Notes |
|---|---|---|---|
| T0 | Create Iced Coffee 120.00 with 5 recipe lines; lines persist on reopen | PASS | Reported |
| T1 | Sale of 2 deducts 40 g beans, 500 ml milk, 20 g sugar, 2 cups, 2 straws | PASS | Reported |
| T2 | Insufficient stock blocked | PASS (Sales screen) | The Sales screen blocked quantity above available stock ("only has stock for 1"). The SaleRepository safety-net check was NOT triggered directly. |
| T3 | Sale rolls back on failure | NOT EXECUTED | |
| T4 | Sales History shows the sale | PASS | Sale appeared (reported). Snapshot item name not specifically reported. |
| T5 | Edit menu price after a sale; old sale unchanged | PASS | Reported |
| T6 | Dashboard sales and expense match the database | PASS | Reported. Gross profit and Fast Moving Items were not individually reported. |
| T7 | Login: correct, wrong, blank; Logout returns to login | PASS | Reported |
| T8 | Settings: change password, validation messages, login with new password | PASS | Reported ("all worked, no errors") |

## Not yet run
I5-I8, S2, S5-S7, L2, L3, X4, X5, X7, T3. Run these before submission if time allows.
