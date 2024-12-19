using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyCc.Application.DTOs;
using MyCc.Application.Services;

namespace MyCc.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")] // 设置路由为 /api/Account
public class AccountController(UserManager<IdentityUser<Guid>> userManager, SignInManager<IdentityUser<Guid>> signInManager) : ControllerBase
{
    // 构造函数注入 AccountService

    /// <summary>
    /// 用户注册接口。
    /// </summary>
    /// <param name="email">用户邮箱。</param>
    /// <param name="password">用户密码。</param>
    /// <returns>注册结果。</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        var user = new IdentityUser<Guid> { UserName = model.Email, Email = model.Email };
        var result = await userManager.CreateAsync(user, model.Password);
        
        if (result.Succeeded)
        {
            return Ok(new { message = "注册成功" });
        }
        
        // 返回详细的错误信息
        var errors = result.Errors.Select(e => e.Description).ToList();
        return BadRequest(new { Errors = errors });
    }


    /// <summary>
    /// 用户登录接口。
    /// </summary>
    /// <param name="model">包含用户邮箱和密码的登录数据传输对象。</param>
    /// <returns>登录结果。</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, model.Password))
        {
            return Unauthorized("登录失败，用户名或密码错误。");
        }

        // 创建 ClaimsPrincipal 表示用户
        var principal = await signInManager.CreateUserPrincipalAsync(user);

        // 签发身份验证 Cookie
        await HttpContext.SignInAsync(
            IdentityConstants.ApplicationScheme,
            principal
            // new AuthenticationProperties { IsPersistent = model.RememberMe } // 如果有 RememberMe 字段的话
        );

        // 返回成功的响应
        return Ok(new { Message = "登录成功" });
    }
    
    /// <summary>
    /// 用户登出接口。
    /// </summary>
    /// <returns>登出结果。</returns>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        return Ok(new { Message = "登出成功" });
    }
}