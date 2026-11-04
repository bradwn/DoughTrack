# DoughTrack project requirements (source of truth)

Project DoughTrack for Doughtown Cafe. Local VB.NET Windows Forms desktop app, .NET 10, MySQL (MySql.Data), ADO.NET with parameterized SQL. No web, mobile, cloud, external APIs, payments, online ordering.

Modules: Login, Dashboard, Sales, Sales History, Menu Items (with recipe/ingredients), Inventory (Stocks, Stock-In, Low Stock), Expenses. One Admin user type only. NEVER add: Suppliers, Reports, Staff, Customers.

Database (9 tables, no Dashboard or Low Stock table): users, menu_items, menu_item_ingredients, sales, sale_items, inventory_items, stock_ins, stock_in_items, expenses. (Migration 002 adds snapshot columns cogs_amount / menu_item_name to sales and sale_items.)

Rules: sale and stock-in use DB transactions; sales blocked on insufficient stock; no negative inventory; menu and inventory items are deactivated, not deleted, when referenced; unit conversion kg/g and L/ml; Low Stock = quantity <= reorder point, Out of Stock = 0; Estimated Gross Profit = revenue - COGS (expenses not subtracted); Fast Moving = rank by quantity sold; Dashboard has no table; no secrets in documentation.

UI: dark sidebar, gray/white content, clean tables, Doughtown branding. Deadline 2026-11-07.
