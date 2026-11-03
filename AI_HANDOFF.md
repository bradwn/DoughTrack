# DoughTrack AI Handoff

Updated 2026-11-04 after Sales History. Read `DESIGN_REFERENCE.md`, `DATABASE_STRUCTURE.md`, and `PROGRESS.md` with this file.

Branch `main` tracks `origin/main`. Framework remains .NET 10 Windows Forms. Startup form is `frmLogin`. Do not add Suppliers, Staff, Customers, Reports, or a payment flow. There is one admin account.

## Database

- MySQL 9.6, localhost, port 3306, database `doughtrack_db`.
- User `Darwin` can SELECT, INSERT, UPDATE, and DELETE. It cannot CREATE or ALTER.
- Do not drop, recreate, or delete existing rows.
- `Database/Migrations/001_create_doughtrack_dev.sql` has not been run.
- `Database/Migrations/002_sale_cogs_snapshot.sql` has not been run. Sales completion still requires it. On 2026-11-04, `sales` and `sale_items` were empty, and `sales.cogs_amount`, `sale_items.menu_item_name`, and `sale_items.cogs_amount` were still absent.

Original sale columns remain. The script adds:

| Table | Column | Purpose |
| --- | --- | --- |
| sales | cogs_amount decimal(12,2) | Cost snapshot for the whole sale |
| sale_items | menu_item_name varchar(100) | Name at the time of sale |
| sale_items | cogs_amount decimal(12,2) | Cost snapshot for the line |

`unit_price` and `subtotal` already store the selling price. Recipe quantity is not copied onto the sale line. The cost uses `unit_cost` at sale time. No new table was added for history.

## Sales behavior

`Forms/SalesView.vb` is the Sales screen. The order stays in memory until Complete Sale.

`SaleRepository.CompleteSale` does all writes through `DatabaseHelper.ExecuteInTransaction`:

1. Reject an empty order.
2. Lock each active menu row and reject a price change.
3. Lock recipe rows. An item with no recipe cannot be sold.
4. Lock inventory rows in id order.
5. Convert recipe units onto the inventory unit with `UnitConverter.ConvertQuantity`. Identical unknown units are compared as written. kg/g, L/ml, and pc/pcs convert. A mismatch fails the sale.
6. Deduct with `UPDATE ... SET quantity = quantity - @needed WHERE quantity >= @needed`.
7. Insert the sale and lines, including name, price, subtotal, and COGS.
8. Any exception rolls back every change in that transaction.

Inactive menu items are omitted from the register and rejected if they are no longer active when the sale is saved. Past sales keep the saved name and price.

Two servings deduct twice each recipe component. Components shared by different lines are summed first.

## Sales History

`Forms/SalesHistoryView.vb` replaces the old read-only grid. `frmSaleDetails` opens from View Details or a double-click on the row.

`SaleRepository.SearchHistory` reads `sales` and `sale_items`. The grid shows sale id, date and time, an item summary, total amount, and View Details.

- Search matches the sale id, the date text, and the saved item name.
- From and To use `sale_date_time`. Each date box stays unchecked until the user checks it. The end date includes that whole day.
- Category lists the current `menu_items.category` of items that appear on a sale. Sale lines do not store a category, so this filter follows a later category edit. It does not change the stored name or price. A sale matches when any of its lines is in the selected category.

`SaleRepository.GetSale` loads one sale for the details window. The detail grid uses `sale_items.quantity`, `unit_price`, and `subtotal`. The item name is `sale_items.menu_item_name` when that column exists and is not blank. History does not read `menu_items.menu_item_name` or `menu_items.selling_price`. If the name snapshot is missing or blank, the line is labeled `Item` plus the menu item id.

The history query was run against `doughtrack_db`. It returned no rows because no sales are saved yet. The screen was not opened.

## Also in the working tree

- Menu list, add, edit, details, and recipe editing.
- Sales completion from the previous pass, still blocked until the COGS migration is applied.
- `UnitConverter` and `StockInDraftLine` are present for stock-in. `UnitConverter.Convert` was renamed to `ConvertQuantity` so it does not hide `System.Convert` inside the Helpers namespace.
- `PasswordHelper` now calls `System.Convert` for Base64.

## Next

Run the COGS migration as an administrator, then build inventory and stock-in so recipes have stock to sell. After a sale is saved, Sales History can show its snapshot name. Until then, a sale line would show `Item` and the menu item id, with the saved unit price and subtotal.
