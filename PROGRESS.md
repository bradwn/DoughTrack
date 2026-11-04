# DoughTrack progress

Updated 2026-11-04 after Expenses.

## Already on origin/main

Commit `50718a3` is on `main` and matches `origin/main`. The earlier push is done. Login, the main shell, menu items, and recipe editing are in that commit.

## This pass

Expenses reads and writes the existing `expenses` table. It is not a Reports module, and it does not add a table.

- The grid columns are Expense ID, Date, Category, Amount, Description, and Actions (View, Edit, Delete).
- Add and Edit require a date and a category. Amount must be greater than zero. Description is optional.
- Search matches the expense id, category, description, and date text.
- From and To date filters are optional. A checked end date includes that whole day. Category filters the stored expense category.
- View Details shows the expense id, date, category, amount, and description from the database.
- Delete removes only that expense after confirmation. Sales, inventory, and other records are not part of the statement.
- The footer shows how many expenses are visible out of the full table, plus the total of the visible amounts.
- Today's Expense on the dashboard is the sum of `expenses.amount` for `expense_date` today. It recalculates when the dashboard opens.

`dotnet build DoughTrack.slnx` succeeded with 0 warnings and 0 errors. A temporary expense dated today was inserted, included in today's sum, updated, and deleted. `expenses` returned to 0 rows. The admin user and the other empty tables were unchanged. The screen was not opened.

## Still in the working tree

Sales History reads completed sales from MySQL.

- The grid columns are Sale ID, Date and Time, Items, Total Amount, and View Details.
- Search matches the sale id, the date, and the item name saved on the sale.
- From and To date filters are optional. A checked end date includes that whole day.
- Category filters sales that contain a menu item currently in that category. The sale does not store a category, so this list can follow a later menu edit. Names and prices on the sale do not.
- View Details shows the transaction id, date and time, each saved item, quantity, unit price, line subtotal, and the sale total.
- Item names come from `sale_items.menu_item_name` when that column exists. Unit price and subtotal always come from `sale_items`. The screen does not display the menu item's current name or selling price. A missing name is shown as `Item` plus the menu item id.

Sales is a point-of-sale screen: menu cards on the left, the current order on the right.

- Search and category filter list active menu items with image, name, category, and price.
- Availability is how many servings the recipe can make from current inventory. No recipe, a unit that cannot be converted, or not enough stock means Unavailable, and Add is disabled.
- The order can add, increase, decrease, remove, and clear lines. The total is the sum of line subtotals.
- Complete Sale checks that the order is not empty, every item is still Active, the price has not changed, and every recipe component has enough stock.
- One transaction locks the menu rows, recipe rows, and inventory rows, inserts `sales` and `sale_items`, stores the item name, selling price, and COGS, then deducts inventory. A failure rolls the whole sale back. The deduction uses `quantity >= needed`, so stock cannot go negative.
- Selling two of an item deducts twice the recipe quantity. Shared ingredients across lines are added together before the deduction.
- There is no payment, cash, change, or card handling.

## Blocked

`Darwin` still cannot `ALTER` tables. `Database/Migrations/002_sale_cogs_snapshot.sql` was not applied. Until an administrator runs it on `doughtrack_db`, Complete Sale stops with that instruction and does not write a partial sale. The script adds `sales.cogs_amount`, `sale_items.menu_item_name`, and `sale_items.cogs_amount`. It does not drop tables or delete rows.

Until that script runs, Sales History can still list a sale and its saved prices, but the item label is `Item` plus the menu item id.

Dashboard profit uses the saved COGS after that column exists. Before the script, it still estimates from the current unit cost. Fast-moving items on the dashboard still use the current menu name.

`doughtrack_dev` is still not created. The app uses `doughtrack_db`.

## Next

1. Run `002_sale_cogs_snapshot.sql` in MySQL Workbench as an administrator.
2. Inventory and stock-in, so the register has ingredients to sell.
