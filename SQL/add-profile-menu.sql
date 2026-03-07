-- 插入个人中心菜单
INSERT INTO SB_Menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
VALUES (0, 'Profile', '/profile', 'views/_core/profile/index.vue', '', '个人中心', 'lucide:user', 9999, 1, 0, 0);

-- 将个人中心分配给当前所有角色
INSERT INTO SB_AdminRoleMenu (RoleId, MenuId)
SELECT r.Id, m.Id
FROM SB_AdminRole r, SB_Menu m
WHERE r.IsDeleted = 0
  AND m.Name = 'Profile'
  AND m.IsDeleted = 0;
