namespace MyCc.Domain.Accounts;

/// <summary>
/// 表示一个用户账户的聚合根。
/// </summary>
public class Account
{
    /// <summary>
    /// 账户 ID。
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// 账户邮箱地址。
    /// </summary>
    public string Email { get; private set; }

    /// <summary>
    /// 账户密码哈希值。
    /// </summary>
    public string PasswordHash { get; private set; }

    /// <summary>
    /// 指示账户是否被锁定。
    /// </summary>
    public bool IsLockedOut { get; private set; }

    /// <summary>
    /// 账户创建时间。
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// 账户最后一次登录时间。
    /// </summary>
    public DateTime? LastLoginAt { get; private set; }

    /// <summary>
    /// 锁定结束时间。
    /// </summary>
    public DateTime? LockoutEnd { get; private set; }

    /// <summary>
    /// 尝试登录失败的次数
    /// </summary>
    public int AccessFailedCount { get; private set; }


    /// <summary>
    /// EF Core 需要的无参数构造函数。
    /// </summary>
    private Account()
    {
    }

    /// <summary>
    /// 创建一个新的账户实例。
    /// </summary>
    /// <param name="email">账户邮箱地址。</param>
    /// <param name="passwordHash">账户密码哈希值。</param>
    public Account(string email, string passwordHash)
    {
        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        IsLockedOut = false;
        CreatedAt = DateTime.UtcNow;
        AccessFailedCount = 0;
    }

    /// <summary>
    /// 锁定账户。
    /// </summary>
    /// <param name="lockoutDuration">锁定持续时间。</param>
    public void LockOut(TimeSpan lockoutDuration)
    {
        IsLockedOut = true;
        LockoutEnd = DateTime.UtcNow.Add(lockoutDuration);
    }

    /// <summary>
    /// 解锁账户。
    /// </summary>
    public void Unlock()
    {
        IsLockedOut = false;
        LockoutEnd = null;
        AccessFailedCount = 0;
    }

    /// <summary>
    /// 更新最后一次登录时间。
    /// </summary>
    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        AccessFailedCount = 0;
    }

    /// <summary>
    /// 增加登录失败计数
    /// </summary>
    public void IncreaseAccessFailedCount()
    {
        AccessFailedCount++;
    }
}