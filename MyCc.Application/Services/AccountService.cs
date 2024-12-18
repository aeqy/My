using Microsoft.AspNetCore.Identity;

namespace MyCc.Application.Services;

public class AccountService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IPasswordHasher<IdentityUser> _passwordHasher;

    public AccountService(UserManager<IdentityUser> userManager, IPasswordHasher<IdentityUser> passwordHasher)
    {
        _userManager = userManager;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// 用户注册。
    /// </summary>
    public async Task<IdentityResult> RegisterAsync(string email, string password)
    {
        var user = new IdentityUser { UserName = email, Email = email };
        return await _userManager.CreateAsync(user, password);
    }

    /// <summary>
    /// 验证密码。
    /// </summary>
    public async Task<bool> VerifyPasswordAsync(string email, string providedPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return false;
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, providedPassword);

        switch (result)
        {
            case PasswordVerificationResult.Failed:
                return false;
            case PasswordVerificationResult.Success:
                return true;
            case PasswordVerificationResult.SuccessRehashNeeded:
                var newHash = _passwordHasher.HashPassword(user, providedPassword);
                user.PasswordHash = newHash;
                await _userManager.UpdateAsync(user);
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// 根据邮箱查找用户。
    /// </summary>
    public async Task<IdentityUser> FindByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    /// <summary>
    /// 更新用户信息（例如邮箱）。
    /// </summary>
    public async Task<IdentityResult> UpdateUserAsync(IdentityUser user)
    {
        return await _userManager.UpdateAsync(user);
    }

    /// <summary>
    /// 更改用户密码。
    /// </summary>
    public async Task<IdentityResult> ChangePasswordAsync(string email, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "用户不存在" });
        }

        return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="user">用户对象</param>
    /// <returns></returns>
    public async Task<IdentityResult> DeleteUserAsync(IdentityUser user)
    {
        return await _userManager.DeleteAsync(user);
    }
}