-- DoughTrack development database.
-- Run this in MySQL Workbench as an account that can CREATE DATABASE.
-- The application login Darwin@localhost cannot (verified 2026-11-04).
--
-- This script does not DROP, ALTER, DELETE, or UPDATE doughtrack_db.
-- CREATE TABLE IF NOT EXISTS is a no-op when a table already exists.
-- The only read of doughtrack_db is the admin-user copy into doughtrack_dev.

CREATE DATABASE IF NOT EXISTS doughtrack_dev
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

GRANT SELECT, INSERT, UPDATE, DELETE ON doughtrack_dev.* TO 'Darwin'@'localhost';

USE doughtrack_dev;

CREATE TABLE IF NOT EXISTS users (
  user_id int NOT NULL AUTO_INCREMENT,
  username varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  password_hash varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  full_name varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  created_at datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (user_id),
  UNIQUE KEY username (username)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS menu_items (
  menu_item_id int NOT NULL AUTO_INCREMENT,
  menu_item_name varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  category varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  selling_price decimal(10,2) NOT NULL,
  image_path varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  status enum('Active','Inactive') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Active',
  PRIMARY KEY (menu_item_id),
  UNIQUE KEY menu_item_name (menu_item_name),
  CONSTRAINT chk_menu_price CHECK (selling_price >= 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS inventory_items (
  inventory_item_id int NOT NULL AUTO_INCREMENT,
  item_name varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  category enum('Ingredients','Supplies') COLLATE utf8mb4_unicode_ci NOT NULL,
  quantity decimal(12,3) NOT NULL DEFAULT '0.000',
  unit varchar(10) COLLATE utf8mb4_unicode_ci NOT NULL,
  reorder_point decimal(12,3) NOT NULL DEFAULT '0.000',
  unit_cost decimal(12,4) NOT NULL DEFAULT '0.0000',
  status enum('Active','Inactive') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Active',
  PRIMARY KEY (inventory_item_id),
  UNIQUE KEY item_name (item_name),
  CONSTRAINT chk_inv_cost CHECK (unit_cost >= 0),
  CONSTRAINT chk_inv_quantity CHECK (quantity >= 0),
  CONSTRAINT chk_inv_reorder CHECK (reorder_point >= 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS menu_item_ingredients (
  menu_item_ingredient_id int NOT NULL AUTO_INCREMENT,
  menu_item_id int NOT NULL,
  inventory_item_id int NOT NULL,
  quantity_required decimal(12,3) NOT NULL,
  unit varchar(10) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (menu_item_ingredient_id),
  UNIQUE KEY uq_mii_menu_inventory (menu_item_id, inventory_item_id),
  KEY fk_mii_inventory_item (inventory_item_id),
  CONSTRAINT fk_mii_inventory_item FOREIGN KEY (inventory_item_id) REFERENCES inventory_items (inventory_item_id),
  CONSTRAINT fk_mii_menu_item FOREIGN KEY (menu_item_id) REFERENCES menu_items (menu_item_id),
  CONSTRAINT chk_mii_quantity CHECK (quantity_required > 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS sales (
  sale_id int NOT NULL AUTO_INCREMENT,
  sale_date_time datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  total_amount decimal(12,2) NOT NULL,
  user_id int NOT NULL,
  PRIMARY KEY (sale_id),
  KEY fk_sales_user (user_id),
  KEY idx_sales_date_time (sale_date_time),
  CONSTRAINT fk_sales_user FOREIGN KEY (user_id) REFERENCES users (user_id),
  CONSTRAINT chk_sales_total CHECK (total_amount >= 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS sale_items (
  sale_item_id int NOT NULL AUTO_INCREMENT,
  sale_id int NOT NULL,
  menu_item_id int NOT NULL,
  quantity int NOT NULL,
  unit_price decimal(10,2) NOT NULL,
  subtotal decimal(12,2) NOT NULL,
  PRIMARY KEY (sale_item_id),
  KEY fk_si_sale (sale_id),
  KEY fk_si_menu_item (menu_item_id),
  CONSTRAINT fk_si_menu_item FOREIGN KEY (menu_item_id) REFERENCES menu_items (menu_item_id),
  CONSTRAINT fk_si_sale FOREIGN KEY (sale_id) REFERENCES sales (sale_id),
  CONSTRAINT chk_si_price CHECK (unit_price >= 0),
  CONSTRAINT chk_si_quantity CHECK (quantity > 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS stock_ins (
  stock_in_id int NOT NULL AUTO_INCREMENT,
  stock_in_date date NOT NULL,
  total_cost decimal(12,2) NOT NULL,
  user_id int NOT NULL,
  PRIMARY KEY (stock_in_id),
  KEY fk_stockin_user (user_id),
  CONSTRAINT fk_stockin_user FOREIGN KEY (user_id) REFERENCES users (user_id),
  CONSTRAINT chk_stockin_total CHECK (total_cost >= 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS stock_in_items (
  stock_in_item_id int NOT NULL AUTO_INCREMENT,
  stock_in_id int NOT NULL,
  inventory_item_id int NOT NULL,
  quantity decimal(12,3) NOT NULL,
  unit_cost decimal(12,4) NOT NULL,
  subtotal decimal(12,2) NOT NULL,
  PRIMARY KEY (stock_in_item_id),
  KEY fk_sii_stock_in (stock_in_id),
  KEY fk_sii_inventory_item (inventory_item_id),
  CONSTRAINT fk_sii_inventory_item FOREIGN KEY (inventory_item_id) REFERENCES inventory_items (inventory_item_id),
  CONSTRAINT fk_sii_stock_in FOREIGN KEY (stock_in_id) REFERENCES stock_ins (stock_in_id),
  CONSTRAINT chk_sii_cost CHECK (unit_cost >= 0),
  CONSTRAINT chk_sii_quantity CHECK (quantity > 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS expenses (
  expense_id int NOT NULL AUTO_INCREMENT,
  expense_date date NOT NULL,
  category varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  amount decimal(10,2) NOT NULL,
  description varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (expense_id),
  CONSTRAINT chk_expense_amount CHECK (amount > 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO doughtrack_dev.users (user_id, username, password_hash, full_name, created_at)
SELECT src.user_id, src.username, src.password_hash, src.full_name, src.created_at
FROM doughtrack_db.users AS src
WHERE NOT EXISTS (
  SELECT 1
  FROM doughtrack_dev.users AS dst
  WHERE dst.username = src.username
);

SELECT 'doughtrack_db' AS database_name, COUNT(*) AS user_rows FROM doughtrack_db.users
UNION ALL
SELECT 'doughtrack_dev', COUNT(*) FROM doughtrack_dev.users;
