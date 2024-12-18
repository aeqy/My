namespace MyCc.Domain.Accounts.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id);
    Task<Account> GetByEmailAsync(string email);
    Task AddAsync(Account account);
    Task UpdateAsync(Account account);
    Task DeleteAsync(Guid id); // 新增删除方法
    Task<List<Account>> GetAllAsync(); // 新增获取所有账户的方法（根据实际需要）
}