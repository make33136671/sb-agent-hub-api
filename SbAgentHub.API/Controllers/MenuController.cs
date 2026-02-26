using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SbAgentHub.Services;
using SbAgentHub.Models;

namespace SbAgentHub.API.Controllers;

/// <summary>
/// 菜单管理
/// </summary>
[ApiController,
    Authorize(AuthenticationSchemes = "JwtAdmin"),
    Route("[controller]/[action]")]
public class MenuController : Controller
{
    private readonly MenuService _menuService;
    private readonly AuthService _authService;

    public MenuController(MenuService menuService, AuthService authService)
    {
        _menuService = menuService;
        _authService = authService;
    }

    #region 身份认证
    /// <summary>
    /// 获取当前登录用户的权限菜单（Admin / Member）
    /// </summary>
    [HttpGet(Name = "All")]
    public ResultData All() => _authService.GetMenus();

    #endregion


    #region CURD
    /// <summary>
    /// 更新菜单属性
    /// </summary>
    [HttpPost]
    public ResultData UpdateMenu([FromForm] int id, [FromForm] string field, [FromForm] string value) => _menuService.UpdateMenu(id, field, value);

    /// <summary>
    /// 权限菜单列表
    /// </summary>
    [HttpGet]
    public ResultData TreeList(string name = "", string title = "", string field = "", string order = "", int page = 1, int pageSize = 10)
    {
        return _menuService.GetTreeList(name, title, field, order, page, pageSize);
    }

    /// <summary>
    /// 权限菜单列表
    /// </summary>
    [HttpGet]
    public ResultData List(string name = "", string title = "", string field = "", string order = "", int page = 1, int pageSize = 10)
    {
        return _menuService.GetList(name, title, field, order, page, pageSize);
    }

    /// <summary>
    /// 查询权限菜单
    /// </summary>
    [HttpGet]
    public ResultData QueryMenu(string key) => _menuService.QueryMenu(key);

    /// <summary>
    /// 添加权限菜单
    /// </summary>
    [HttpPost]
    public ResultData Add([FromForm] Menu menu)
    {
        return _menuService.Create(menu);
    }

    /// <summary>
    /// 更新权限菜单
    /// </summary>
    [HttpPost]
    public ResultData Edit([FromForm] Menu menu)
    {
        return _menuService.Edit(menu);
    }

    /// <summary>
    /// 删除权限菜单
    /// </summary>
    [HttpPost]
    public ResultData Del([FromForm] int id)
    {
        return _menuService.Del(id);
    }

    /// <summary>
    /// 权限菜单详细信息
    /// </summary>
    [HttpGet]
    public ResultData Detail(int id)
    {
        return _menuService.Detail(id);
    }
    #endregion


}

