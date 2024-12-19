using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;

namespace MyCc.WebAPI.Controllers;

public class AuthorizeController : Controller
{
    [HttpGet("~/connect/authorize")]
    public IActionResult Authorize()
    {
        // 处理授权请求
        var request = HttpContext.GetOpenIddictServerRequest();
        // 验证用户是否已登录
        if (User.Identity is { IsAuthenticated: false })
        {
            // 如果用户未登录，重定向到登录页面
            return Challenge();
        }

        // 创建授权响应
        var response = new OpenIddictResponse
        {
            Code = "authorization_code", // 生成授权码
            State = request?.State
        };
        return Ok(response);
    }

    [HttpPost("~/connect/token")]
    public Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest();

        if (request != null && request.IsAuthorizationCodeGrantType())
        {
            // 处理授权码流
            // 验证授权码并生成访问令牌
        }
        else if (request != null && request.IsRefreshTokenGrantType())
        {
            // 处理刷新令牌流
            // 验证刷新令牌并生成新的访问令牌
        }

        return Task.FromResult<IActionResult>(BadRequest(new OpenIddictResponse
        {
            Error = OpenIddictConstants.Errors.UnsupportedGrantType,
            ErrorDescription = "The specified grant type is not supported."
        }));
    }
}