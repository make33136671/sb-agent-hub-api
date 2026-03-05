-- Migration: Add menu entries for Phase 3 features
-- Run AFTER migration_phase3_features.sql

-- ========================================
-- 1. 访客信息收集 (top-level)
-- ========================================
INSERT INTO SB_Menu (ParentId, Name, Path, Component, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
VALUES (0, 'VisitorFormMgmt', '/visitor-form-mgmt', 'BasicLayout', '访客收集', 'lucide:clipboard-list', 30, 0, 0, 0);
SET @visitorFormMenuId = LAST_INSERT_ID();

INSERT INTO SB_Menu (ParentId, Name, Path, Component, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
VALUES (@visitorFormMenuId, 'VisitorFormList', '/visitor-form-mgmt/list', '/visitor-form/List', '表单管理', '', 1, 0, 0, 0);

-- ========================================
-- 2. 自动回复 (top-level)
-- ========================================
INSERT INTO SB_Menu (ParentId, Name, Path, Component, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
VALUES (0, 'AutoReplyMgmt', '/auto-reply-mgmt', 'BasicLayout', '自动回复', 'lucide:message-square-reply', 31, 0, 0, 0);
SET @autoReplyMenuId = LAST_INSERT_ID();

INSERT INTO SB_Menu (ParentId, Name, Path, Component, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
VALUES (@autoReplyMenuId, 'AutoReplyList', '/auto-reply-mgmt/list', '/auto-reply/List', '规则管理', '', 1, 0, 0, 0);

-- ========================================
-- 3. 满意度评价 (top-level)
-- ========================================
INSERT INTO SB_Menu (ParentId, Name, Path, Component, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
VALUES (0, 'SatisfactionMgmt', '/satisfaction-mgmt', 'BasicLayout', '满意度评价', 'lucide:star', 32, 0, 0, 0);
SET @satisfactionMenuId = LAST_INSERT_ID();

INSERT INTO SB_Menu (ParentId, Name, Path, Component, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
VALUES (@satisfactionMenuId, 'SatisfactionList', '/satisfaction-mgmt/list', '/satisfaction/List', '评价列表', '', 1, 0, 0, 0);

-- ========================================
-- 4. 数据导出 (top-level)
-- ========================================
INSERT INTO SB_Menu (ParentId, Name, Path, Component, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
VALUES (0, 'DataExportMgmt', '/data-export-mgmt', 'BasicLayout', '数据导出', 'lucide:download', 33, 0, 0, 0);
SET @dataExportMenuId = LAST_INSERT_ID();

INSERT INTO SB_Menu (ParentId, Name, Path, Component, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
VALUES (@dataExportMenuId, 'DataExportList', '/data-export-mgmt/list', '/data-export/List', '导出管理', '', 1, 0, 0, 0);

-- ========================================
-- 5. 套餐模板 (under QuotaMgmt)
-- ========================================
SET @quotaMenuId = (SELECT Id FROM SB_Menu WHERE Name = 'QuotaMgmt' LIMIT 1);

INSERT INTO SB_Menu (ParentId, Name, Path, Component, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
VALUES (@quotaMenuId, 'QuotaPackageList', '/quota-mgmt/packages', '/quota-package/List', '套餐模板', '', 3, 0, 0, 0);

-- ========================================
-- 6. Associate with super admin role
-- ========================================
SET @superRoleId = (SELECT Id FROM SB_AdminRole WHERE RoleKey = 'super' LIMIT 1);

INSERT INTO SB_AdminRoleMenu (RoleId, MenuId)
SELECT @superRoleId, Id FROM SB_Menu
WHERE Name IN (
    'VisitorFormMgmt', 'VisitorFormList',
    'AutoReplyMgmt', 'AutoReplyList',
    'SatisfactionMgmt', 'SatisfactionList',
    'DataExportMgmt', 'DataExportList',
    'QuotaPackageList'
)
AND Id NOT IN (SELECT MenuId FROM SB_AdminRoleMenu WHERE RoleId = @superRoleId);
