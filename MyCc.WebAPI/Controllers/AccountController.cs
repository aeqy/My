using Microsoft.AspNetCore.Mvc;
using MyCc.Application.Services;

namespace MyCc.WebAPI.Controllers;

[ApiController]
    [Route("api/[controller]")] // 设置路由为 /api/Account
    public class AccountController(AccountService accountService) : ControllerBase
    {
        // 构造函数注入 AccountService

        /// <summary>
        /// 用户注册接口。
        /// </summary>
        /// <param name="email">用户邮箱。</param>
        /// <param name="password">用户密码。</param>
        /// <returns>注册结果。</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return BadRequest(new { message = "邮箱和密码不能为空" });
            }

            var result = await accountService.RegisterAsync(email, password);

            if (result.Succeeded)
            {
                return Ok(new { message = "注册成功" });
            }
            else
            {
                // 处理注册失败的情况，返回错误信息
                return BadRequest(new { message = "注册失败", errors = result.Errors });
            }
        }


        /// <summary>
        /// 用户登录接口。
        /// </summary>
        /// <param name="email">用户邮箱。</param>
        /// <param name="password">用户密码。</param>
        /// <returns>登录结果。</returns>
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
                // 登录成功，可以生成 JWT Token 等
                return Ok(new { message = "登录成功" });
            }
            else
            {
                return Unauthorized(new { message = "用户名或密码错误" });
            }
        }
    }