# DoughTrack design reference

The Figma screens in `Design/references/` are the visual reference for every remaining screen. Match layout, spacing, colors, type, buttons, sidebar, tables, and cards as closely as Windows Forms allows. Keep the same shell on every module. Do not turn this into a web app. Do not drop a working behavior just to copy a pixel.

Reviewed 2026-11-04. No screen was rebuilt in that review.

| File | Screens |
| --- | --- |
| `Design/references/01-login-dashboard.png` | Login, Dashboard |
| `Design/references/02-sales-history.png` | Sales, Sales History |
| `Design/references/03-menu-items.png` | Menu list, add, edit, details |
| `Design/references/04-inventory.png` | Inventory overview, stocks, stock-in, low stock |
| `Design/references/05-expenses.png` | Expenses list, add expense |
| `Design/references/06-settings.png` | Account settings |

The login design is included, so a later restyle of `frmLogin` is allowed. It was not changed during this review.

A separate logo file was not provided. Use the mark shown in these screens. Until a real image file is added, a circular white badge with the Doughtown name is the stand-in.

## Colors

Sampled from the screenshots. Use these instead of the default Windows theme.

| Role | Color | Approx. RGB |
| --- | --- | --- |
| App background | near black | 18, 18, 18 |
| Sidebar | charcoal | 44, 44, 44 |
| Sidebar selected item | lighter gray | 90, 90, 90 |
| Header bar | medium gray | 128, 128, 128 |
| Page surface | light gray | 232, 232, 232 |
| Card / input fill | off-white | 245, 245, 245 |
| Card header | medium gray | 128, 128, 128 |
| Table header | darker gray | 70, 70, 70 |
| Table row | light gray | 230, 230, 230 |
| Primary button | near black | 30, 30, 30 |
| Button text | white | 255, 255, 255 |
| Body text | near black | 20, 20, 20 |
| Muted text | gray | 120, 120, 120 |
| In stock | green | 40, 160, 70 |
| Low / unavailable | red | 200, 40, 40 |

Flat controls. No system blue. No gradients.

## Type and controls

- UI font: Segoe UI. Titles about 16–18 pt. Body and table text about 9–10 pt.
- Buttons: flat, dark fill, white text, slight padding. Disabled actions stay visible and gray.
- Inputs: light fill, thin gray border, small inner padding.
- Tables: gray header, light rows, no heavy grid. Search on the left, filters on the right.
- Cards: gray title strip, light body, even gap between cards.
- Status text: green for available / good stock, red for unavailable / low stock.
- Money uses the peso sign shown in the designs.

Windows Forms cannot match every radius, shadow, or custom dropdown. Use flat `Button` controls, `Panel` sections, and `DataGridView` or a drawn list. Square corners are acceptable.

## Shell used by every module after login

Left sidebar, full height:

1. Circular Doughtown mark at the top.
2. Dashboard
3. Sales
4. Sales History
5. Menu Items
6. Inventory
7. Expenses
8. A gap, then Settings
9. Logout

The selected item is a lighter gray bar. Sidebar text is white.

Top bar, gray:

- Screen title on the left.
- Bell icon, then a dark **Admin** label on the right.

There is one admin account. The Admin control shows the logged-in name. It does not switch users. The bell has no table behind it. Draw it, and leave it without a fake notification feed.

Content sits on the light gray page with a margin inside the header and sidebar.

Logout clears `Session` and returns to `frmLogin`.

## Screens

### Login

White page. Left column: title **DoughTrack**, Username, Password, **Login**, and **Forgot Password?**. Right side: Doughtown mark, “est. 2020”, and “coffee • snacks • dessert”.

Forgot Password cannot send email. This app has no online service. The practical behavior is a short message that the password is changed in Settings after login. Do not add a bypass that sets a new password with no proof of the current one.

### Dashboard

Five cards:

- Today’s Sales
- Fast Moving Items
- Est. Gross Profit
- Today’s Expense (wide)
- Low Stocks (wide)

These read existing sales, sale items, expenses, and inventory. They are the home screen, not a separate Reports module. Gross profit is an estimate from today’s sales, recipe cost, and today’s expenses. Say “Est.” because recipe cost uses current `unit_cost`, not a stored historical cost.

### Sales

Search, category filter, then a row for each active menu item: image, name, price, stock (Available or Unavailable), quantity stepper, **Add to order**. Unavailable items cannot be added.

Current Order lists the lines, a total, **Clear**, and **Complete Sale**. The order stays in memory until Complete Sale writes `sales`, `sale_items`, and inventory deductions in one transaction.

Available means the recipe can be made from current ingredient quantities. A menu item with no recipe is not sold as available.

### Sales History

Columns: ID, Date & Time, Items. Search and category filter sit above the grid. Items is a summary of that sale’s line names, not a database column. The search box in the mock says “Search menu items”; search the sale id, date, and item names instead.

### Menu Items

List columns: checkbox, image, name, category, selling price, status, actions (view, edit, delete). Search, category filter, and **Add Menu Item**.

Add and edit: image file, name, category, price, status, description, ingredients. Buttons: Cancel and Save. Edit also has Delete.

Details: image, name, category, price, status, description, ingredient list, and a 30-day sales count.

Status on screen is Available / Unavailable. The database values stay `Active` and `Inactive`. Show Active as Available and Inactive as Unavailable.

`menu_items` has no description column. Do not invent one in the UI and then drop the text. When the Menu screen is built, add a nullable `description` column with a new incremental script. Do not rebuild the table.

Images: file picker, copy the file under the app’s image folder, store the path in `image_path`.

Ingredients on the add/edit form are `menu_item_ingredients`.

### Inventory

Same shell, with these views:

- Overview: total items, low-stock count, total stock value, a low-stock preview, and an entry to the full list.
- Stocks: ingredients and supplies. Columns include name, category, quantity, unit, reorder point, status, and actions. Green when quantity is above the reorder point, red when it is at or below it.
- Stock-in: date, lines (item, quantity, unit cost, subtotal), total, Cancel, Save. Saving writes `stock_ins`, `stock_in_items`, and the inventory quantity update in one transaction.
- Low stock: items at or below `reorder_point`, with current stock in red.

### Expenses

List columns: Date, Category, Amount. Search, category filter, date filter, and **Add Expenses**. The footer shows the visible range.

Add: date (required), category, amount, Cancel, Save Expense. The table also has an optional `description`. The mock does not show it. Keep the column and include it on the form only if it fits without crowding the design.

### Settings

Account Settings: username `admin` (read-only), masked password, **change password**. Changing the password checks the current password with `PasswordHelper`, stores a new hash, and updates the existing admin row. Do not create a second account type.

## Build order

1. Shared shell: sidebar, header, colors, navigation, logout.
2. Login restyle, because the Figma includes it.
3. Dashboard.
4. Menu, Inventory, Recipes, Stock-in, Sales, Sales History, Expenses, Settings.

Each new screen uses this file and the matching reference image.
