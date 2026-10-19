-- 插入个人中心菜单
INSERT INTO sb_agenthub_menu (ParentId, Name, Path, Component, Redirect, Title, Icon, OrderNo, HideInMenu, IgnoreKeepAlive, IsDeleted)
VALUES (0, 'Profile', '/profile', 'views/_core/profile/index.vue', '', '个人中心', 'lucide:user', 9999, 1, 0, 0);

-- 将个人中心分配给当前所有角色
INSERT INTO sb_agenthub_admin_role_menu (RoleId, MenuId)
SELECT r.Id, m.Id
FROM sb_agenthub_admin_role r, sb_agenthub_menu m
WHERE r.IsDeleted = 0
  AND m.Name = 'Profile'
  AND m.IsDeleted = 0;
