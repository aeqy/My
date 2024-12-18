using Microsoft.AspNetCore.Identity;

namespace MyCc.Application.Identity;

public class UserAppService(UserManager<IdentityUser> userManager)
{
    public async Task<IdentityResult> RegisterAsync(string userName, Guid accountId)
    {
        if (string.IsNullOrEmpty(userName))
        {
            throw new ApplicationException("用户名不能为空");
        }
        var user = new IdentityUser { UserName = userName, Email = $"user{accountId}@temp.com" }; // 临时邮箱
        var result = await userManager.CreateAsync(user);
        if(result.Succeeded)
        {
            // 可以添加用户和账户的关联，例如创建一个 UserAccount 表
            // 或者在 IdentityUser 中添加 AccountId 属性
            // 这里为了演示，暂时省略
        }
        return result;
    }
}