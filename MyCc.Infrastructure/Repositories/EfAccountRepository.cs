using Microsoft.EntityFrameworkCore;
using MyCc.Domain.Accounts;
using MyCc.Domain.Accounts.Repositories;
using MyCc.Infrastructure.Data;

namespace MyCc.Infrastructure.Repositories;


public class EfAccountRepository(MyDbContext context) : IAccountRepository
{
    public async Task<Account?> GetByIdAsync(Guid id)
    {
        return await context.Accounts.FindAsync(id);
    }

    public async Task<Account> GetByEmailAsync(string email)
    {
        return await context.Accounts.FirstOrDefaultAsync(a => a.Email == email) ?? throw new Exception("账户不存在"); // 或者如果允许 null 则返回 null

        // return await context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
    }

    public async Task AddAsync(Account account)
    {
        await context.Accounts.AddAsync(account);
        // 使用工作单元模式时，此处可省略 SaveChangesAsync()
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Account account)
    {
        context.Accounts.Update(account);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var account = await GetByIdAsync(id);

        if (account != null)
        {
            context.Accounts.Remove(account);
            await context.SaveChangesAsync();
        }
    }

    public async Task<List<Account>> GetAllAsync()
    {
        return await context.Accounts.ToListAsync();
    }
}