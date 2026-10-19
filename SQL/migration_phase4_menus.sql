-- Migration: Phase 4 - 订单管理菜单
-- Run AFTER migration_phase4_order.sql

-- ========================================
-- 1. 订单管理 (top-level)
-- ========================================
INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
VALUES (0, 'OrderMgmt', '/order-mgmt', 'BasicLayout', '订单管理', 'lucide:receipt', 7, 0, 0, 0);
SET @orderMenuId = LAST_INSERT_ID();

INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
VALUES (@orderMenuId, 'OrderList', '/order-mgmt/list', '/order/List', '订单列表', '', 1, 0, 0, 0);

-- ========================================
-- 2. Associate with super admin role
-- ========================================
SET @superRoleId = (SELECT Id FROM sb_agenthub_admin_role WHERE RoleKey = 'super' LIMIT 1);

INSERT INTO sb_agenthub_admin_role_menu (RoleId, MenuId)
SELECT @superRoleId, Id FROM sb_agenthub_menu
WHERE Name IN ('OrderMgmt', 'OrderList')
AND Id NOT IN (SELECT MenuId FROM sb_agenthub_admin_role_menu WHERE RoleId = @superRoleId);
