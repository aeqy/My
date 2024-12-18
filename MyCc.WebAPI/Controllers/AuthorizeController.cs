using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MyCc.Application.Services;

namespace MyCc.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorizeController(AccountService accountService, IConfiguration configuration)
    : ControllerBase
{
    /// <summary>
    /// 用户登录并获取 JWT Token。
    /// </summary>
    /// <param name="email">用户邮箱。</param>
    /// <param name="password">用户密码。</param>
    /// <returns>包含 JWT Token 的响应。</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            return BadRequest(new { message = "邮箱和密码不能为空" });
        }

        var isValid = await accountService.VerifyPasswordAsync(email, password);

        if (isValid)
        {
            // 生成 JWT Token
            var token = GenerateJwtToken(email);
            return Ok(new { token }); // 返回 Token
        }
        else
        {
            return Unauthorized(new { message = "用户名或密码错误" });
        }
    }

    /// <summary>
    /// 生成 JWT Token 的方法。
    /// </summary>
    /// <param name="email">用户的邮箱（作为 Claim）。</param>
    /// <returns>生成的 JWT Token 字符串。</returns>
    private string GenerateJwtToken(string email)
    {
        // 从配置中读取密钥
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? string.Empty));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // 创建 Claims，可以添加用户的角色、ID 等信息
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            // 可以添加更多 Claims，例如：
            // new Claim(ClaimTypes.Role, "User"),
            // new Claim("UserId", "123")
        };

        // 创建 Token
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1), // 设置过期时间
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// 受保护的接口，需要 JWT 认证。
    /// </summary>
    /// <returns>受保护的资源。</returns>
    [HttpGet("protected"), Authorize] // 添加 Authorize 特性
    public IActionResult ProtectedResource()
    {
        return Ok(new { message = "这是受保护的资源" });
    }
}