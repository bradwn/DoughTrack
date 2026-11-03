-- Adds sale snapshot columns. Safe to run more than once.
-- Does not drop tables, delete rows, or change existing values except blank item names.
-- Run in MySQL Workbench as an administrator, with doughtrack_db selected.
-- The application user Darwin cannot ALTER tables.

SET @schema_name := DATABASE();

SET @sales_cogs_exists := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = @schema_name
      AND TABLE_NAME = 'sales'
      AND COLUMN_NAME = 'cogs_amount'
);
SET @sales_cogs_sql := IF(
    @sales_cogs_exists = 0,
    'ALTER TABLE sales ADD COLUMN cogs_amount DECIMAL(12,2) NOT NULL DEFAULT 0 AFTER total_amount',
    'SELECT 1'
);
PREPARE sales_cogs_stmt FROM @sales_cogs_sql;
EXECUTE sales_cogs_stmt;
DEALLOCATE PREPARE sales_cogs_stmt;

SET @item_name_exists := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = @schema_name
      AND TABLE_NAME = 'sale_items'
      AND COLUMN_NAME = 'menu_item_name'
);
SET @item_name_sql := IF(
    @item_name_exists = 0,
    'ALTER TABLE sale_items ADD COLUMN menu_item_name VARCHAR(100) NOT NULL DEFAULT '''' AFTER menu_item_id',
    'SELECT 1'
);
PREPARE item_name_stmt FROM @item_name_sql;
EXECUTE item_name_stmt;
DEALLOCATE PREPARE item_name_stmt;

SET @item_cogs_exists := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = @schema_name
      AND TABLE_NAME = 'sale_items'
      AND COLUMN_NAME = 'cogs_amount'
);
SET @item_cogs_sql := IF(
    @item_cogs_exists = 0,
    'ALTER TABLE sale_items ADD COLUMN cogs_amount DECIMAL(12,2) NOT NULL DEFAULT 0 AFTER subtotal',
    'SELECT 1'
);
PREPARE item_cogs_stmt FROM @item_cogs_sql;
EXECUTE item_cogs_stmt;
DEALLOCATE PREPARE item_cogs_stmt;

UPDATE sale_items AS si
INNER JOIN menu_items AS mi ON mi.menu_item_id = si.menu_item_id
SET si.menu_item_name = mi.menu_item_name
WHERE si.menu_item_name = '';

SET @sales_check_exists := (
    SELECT COUNT(*)
    FROM information_schema.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = @schema_name
      AND TABLE_NAME = 'sales'
      AND CONSTRAINT_NAME = 'chk_sales_cogs'
);
SET @sales_check_sql := IF(
    @sales_check_exists = 0,
    'ALTER TABLE sales ADD CONSTRAINT chk_sales_cogs CHECK (cogs_amount >= 0)',
    'SELECT 1'
);
PREPARE sales_check_stmt FROM @sales_check_sql;
EXECUTE sales_check_stmt;
DEALLOCATE PREPARE sales_check_stmt;

SET @item_check_exists := (
    SELECT COUNT(*)
    FROM information_schema.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = @schema_name
      AND TABLE_NAME = 'sale_items'
      AND CONSTRAINT_NAME = 'chk_si_cogs'
);
SET @item_check_sql := IF(
    @item_check_exists = 0,
    'ALTER TABLE sale_items ADD CONSTRAINT chk_si_cogs CHECK (cogs_amount >= 0)',
    'SELECT 1'
);
PREPARE item_check_stmt FROM @item_check_sql;
EXECUTE item_check_stmt;
DEALLOCATE PREPARE item_check_stmt;
