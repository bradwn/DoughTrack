# DoughTrack database structure

Checked against live MySQL on 2026-11-04. `PROJECT_REQUIREMENTS.md` is not in this repository or under the user profile. This document describes the database that already exists.

## Server

- Product: MySQL 9.6.0
- Host name reported by the server: Darwin
- Application port: **3306** (`@@port`, and `port=3306` in `C:\ProgramData\MySQL\MySQL Server 9.6\my.ini`)
- Port 33060 is the X Protocol port. The VB app does not use it. A classic MySQL client gets a protocol mismatch there.
- Charset: `utf8mb4`. Table collation: `utf8mb4_unicode_ci`.
- Engine: InnoDB

## Connection

`DoughTrack/Data/DatabaseHelper.vb` reads `DoughTrack/appsettings.json`:

| Setting | Current value |
| --- | --- |
| Server | localhost |
| Port | 3306 |
| Name | doughtrack_db |
| User | Darwin |

The password stays in `appsettings.json` only. That file is gitignored. `appsettings.example.json` uses the placeholder `your-password`.

`Darwin`@`localhost` can `SELECT`, `INSERT`, `UPDATE`, and `DELETE` on `doughtrack_db` only. It cannot create a database. A local `root` login without a password was denied. `doughtrack_dev` was therefore not created in this session.

## Databases

| Database | Status |
| --- | --- |
| doughtrack_db | Original database. Do not drop, recreate, reset, or delete its rows. |
| doughtrack_dev | Not created yet. Script: `Database/Migrations/001_create_doughtrack_dev.sql`. Run it in MySQL Workbench with an account that can create databases. |

The script creates `doughtrack_dev` only if it is missing, creates each table only if it is missing, grants Darwin the same four DML rights, and copies `users` with `INSERT ... SELECT` when the username is not already there. It does not `DROP`, `ALTER`, `DELETE`, or `UPDATE` `doughtrack_db`.

Do not run the Workbench dumps in `OneDrive\Documents\dumps\Dump20261104\`. Those files contain `DROP TABLE`.

## Row counts in doughtrack_db

Exact `COUNT(*)` on 2026-11-04, before this preparation session:

| Table | Rows |
| --- | ---: |
| users | 1 |
| menu_items | 0 |
| inventory_items | 0 |
| menu_item_ingredients | 0 |
| sales | 0 |
| sale_items | 0 |
| stock_ins | 0 |
| stock_in_items | 0 |
| expenses | 0 |

The user row is `user_id` 1, username `admin`, full name `Doughtown Administrator`. The password is stored as a PBKDF2 hash (`iterations.salt.hash`). There is no role column. The application has one account type: Admin.

## Missing tables and columns

No table or column is missing for the modules still to be built:

| Module | Tables |
| --- | --- |
| Login | users |
| Menu | menu_items |
| Inventory | inventory_items |
| Recipes | menu_item_ingredients |
| Sales | sales, sale_items |
| Stock-in | stock_ins, stock_in_items |
| Expenses | expenses |

These are intentionally absent: Suppliers, Reports, Staff, Customers, and a user-role column.

Saving a sale or a stock-in must update `inventory_items.quantity` in the same transaction as the header and lines. That is application behavior. It does not need another table. `quantity` cannot go below 0.

## Tables

### users

| Column | Type | Notes |
| --- | --- | --- |
| user_id | int | PK, auto-increment |
| username | varchar(50) | NOT NULL, unique |
| password_hash | varchar(255) | NOT NULL |
| full_name | varchar(100) | NOT NULL |
| created_at | datetime | NOT NULL, default CURRENT_TIMESTAMP |

### menu_items

| Column | Type | Notes |
| --- | --- | --- |
| menu_item_id | int | PK, auto-increment |
| menu_item_name | varchar(100) | NOT NULL, unique |
| category | varchar(50) | NOT NULL |
| selling_price | decimal(10,2) | NOT NULL, check >= 0 |
| image_path | varchar(255) | NULL |
| status | enum('Active','Inactive') | NOT NULL, default Active |

### inventory_items

| Column | Type | Notes |
| --- | --- | --- |
| inventory_item_id | int | PK, auto-increment |
| item_name | varchar(100) | NOT NULL, unique |
| category | enum('Ingredients','Supplies') | NOT NULL |
| quantity | decimal(12,3) | NOT NULL, default 0, check >= 0 |
| unit | varchar(10) | NOT NULL |
| reorder_point | decimal(12,3) | NOT NULL, default 0, check >= 0 |
| unit_cost | decimal(12,4) | NOT NULL, default 0, check >= 0 |
| status | enum('Active','Inactive') | NOT NULL, default Active |

### menu_item_ingredients

| Column | Type | Notes |
| --- | --- | --- |
| menu_item_ingredient_id | int | PK, auto-increment |
| menu_item_id | int | NOT NULL, FK menu_items |
| inventory_item_id | int | NOT NULL, FK inventory_items |
| quantity_required | decimal(12,3) | NOT NULL, check > 0 |
| unit | varchar(10) | NOT NULL |

Unique (`menu_item_id`, `inventory_item_id`).

### sales

| Column | Type | Notes |
| --- | --- | --- |
| sale_id | int | PK, auto-increment |
| sale_date_time | datetime | NOT NULL, default CURRENT_TIMESTAMP, indexed |
| total_amount | decimal(12,2) | NOT NULL, check >= 0 |
| user_id | int | NOT NULL, FK users |

### sale_items

| Column | Type | Notes |
| --- | --- | --- |
| sale_item_id | int | PK, auto-increment |
| sale_id | int | NOT NULL, FK sales |
| menu_item_id | int | NOT NULL, FK menu_items |
| quantity | int | NOT NULL, check > 0 |
| unit_price | decimal(10,2) | NOT NULL, check >= 0 |
| subtotal | decimal(12,2) | NOT NULL |

### stock_ins

| Column | Type | Notes |
| --- | --- | --- |
| stock_in_id | int | PK, auto-increment |
| stock_in_date | date | NOT NULL |
| total_cost | decimal(12,2) | NOT NULL, check >= 0 |
| user_id | int | NOT NULL, FK users |

### stock_in_items

| Column | Type | Notes |
| --- | --- | --- |
| stock_in_item_id | int | PK, auto-increment |
| stock_in_id | int | NOT NULL, FK stock_ins |
| inventory_item_id | int | NOT NULL, FK inventory_items |
| quantity | decimal(12,3) | NOT NULL, check > 0 |
| unit_cost | decimal(12,4) | NOT NULL, check >= 0 |
| subtotal | decimal(12,2) | NOT NULL |

### expenses

| Column | Type | Notes |
| --- | --- | --- |
| expense_id | int | PK, auto-increment |
| expense_date | date | NOT NULL |
| category | varchar(50) | NOT NULL |
| amount | decimal(10,2) | NOT NULL, check > 0 |
| description | varchar(255) | NULL |

## Foreign keys

| Name | From | To |
| --- | --- | --- |
| fk_mii_menu_item | menu_item_ingredients.menu_item_id | menu_items.menu_item_id |
| fk_mii_inventory_item | menu_item_ingredients.inventory_item_id | inventory_items.inventory_item_id |
| fk_sales_user | sales.user_id | users.user_id |
| fk_si_sale | sale_items.sale_id | sales.sale_id |
| fk_si_menu_item | sale_items.menu_item_id | menu_items.menu_item_id |
| fk_stockin_user | stock_ins.user_id | users.user_id |
| fk_sii_stock_in | stock_in_items.stock_in_id | stock_ins.stock_in_id |
| fk_sii_inventory_item | stock_in_items.inventory_item_id | inventory_items.inventory_item_id |
