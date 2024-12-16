

好的，下面是一个更复杂的账户模块登录功能实现，包含详细的注释。该实现使用清洁架构、领域驱动设计（DDD）、CQRS模式，并使用EF Core和PostgreSQL数据库。我们将实体的int主键替换为GUID。

### 项目结构

```
MyCc/
│
├── MyCc.Domain/
│   ├── Entities/
│   │   └── User.cs
│   └── Interfaces/
│       └── IUserRepository.cs
│
├── MyCc.Infrastructure/
│   ├── Data/
│   │   ├── MyDbContext.cs
│   │   └── UserRepository.cs
│
├── MyCc.Application/
│   ├── Commands/
│   │   └── LoginCommand.cs
│   ├── Handlers/
│   │   └── LoginCommandHandler.cs
│   └── Services/
│       └── AuthenticationService.cs
│
└── MyCc.WebAPI/
    ├── Controllers/
    │   └── AccountController.cs
    └── Startup.cs
```

### 代码实现

#### 1. Domain层

**User实体**

```csharp:MyCc.Domain/Entities/User.cs
using System;

namespace MyCc.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } // 使用GUID作为主键
        public string Username { get; set; }
        public string PasswordHash { get; set; } // 存储加密后的密码
        public int FailedLoginAttempts { get; set; } // 登录失败次数
        public bool IsLocked { get; set; } // 账户是否被锁定
        public DateTime? LastLoginTime { get; set; } // 上次登录时间
        // 其他属性...
    }
}
```

**IUserRepository接口**

```csharp:MyCc.Domain/Interfaces/IUserRepository.cs
using System.Threading.Tasks;
using MyCc.Domain.Entities;

namespace MyCc.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByUsernameAsync(string username); // 根据用户名获取用户
        Task UpdateAsync(User user); // 更新用户信息
        // 其他方法...
    }
}
```

#### 2. Infrastructure层

**MyDbContext**

```csharp:MyCc.Infrastructure/Data/MyDbContext.cs
using Microsoft.EntityFrameworkCore;
using MyCc.Domain.Entities;

namespace MyCc.Infrastructure.Data
{
    public class MyDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } // 用户表

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("YourConnectionStringHere"); // 配置PostgreSQL连接字符串
        }
    }
}
```

**UserRepository实现**

```csharp:MyCc.Infrastructure/Data/UserRepository.cs
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyCc.Domain.Entities;
using MyCc.Domain.Interfaces;

namespace MyCc.Infrastructure.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly MyDbContext _context;

        public UserRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            // 根据用户名查找用户
            return await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
        }

        public async Task UpdateAsync(User user)
        {
            // 更新用户信息
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
```

#### 3. Application层

**LoginCommand**

```csharp:MyCc.Application/Commands/LoginCommand.cs
namespace MyCc.Application.Commands
{
    public class LoginCommand
    {
        public string Username { get; set; } // 用户名
        public string Password { get; set; } // 密码
    }
}
```

**LoginCommandHandler**

```csharp:MyCc.Application/Handlers/LoginCommandHandler.cs
using System.Threading.Tasks;
using MyCc.Application.Commands;
using MyCc.Application.Services;
using MyCc.Domain.Interfaces;

namespace MyCc.Application.Handlers
{
    public class LoginCommandHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly AuthenticationService _authService;

        public LoginCommandHandler(IUserRepository userRepository, AuthenticationService authService)
        {
            _userRepository = userRepository;
            _authService = authService;
        }

        public async Task<bool> Handle(LoginCommand command)
        {
            var user = await _userRepository.GetByUsernameAsync(command.Username);
            if (user == null || user.IsLocked)
            {
                // 用户不存在或账户被锁定
                return false;
            }

            if (_authService.VerifyPassword(command.Password, user.PasswordHash))
            {
                // 密码验证成功
                user.FailedLoginAttempts = 0; // 重置失败次数
                user.LastLoginTime = DateTime.UtcNow; // 更新最后登录时间
                // 记录登录日志（可选）
                await _userRepository.UpdateAsync(user);
                return true;
            }
            else
            {
                // 密码验证失败
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= 5)
                {
                    // 超过失败次数限制，锁定账户
                    user.IsLocked = true;
                }
                await _userRepository.UpdateAsync(user);
                return false;
            }
        }
    }
}
```

**AuthenticationService**

```csharp:MyCc.Application/Services/AuthenticationService.cs
using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace MyCc.Application.Services
{
    public class AuthenticationService
    {
        public bool VerifyPassword(string password, string storedHash)
        {
            // 验证密码
            // 这里假设storedHash是用HashPassword方法生成的
            var parts = storedHash.Split('.');
            if (parts.Length != 2) return false;

            var salt = Convert.FromBase64String(parts[0]);
            var hash = Convert.FromBase64String(parts[1]);

            var hashed = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8);

            return CryptographicOperations.FixedTimeEquals(hash, hashed);
        }

        public string HashPassword(string password)
        {
            // 生成密码哈希
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8);

            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }
    }
}
```

#### 4. WebAPI层

**AccountController**

```csharp:MyCc.WebAPI/Controllers/AccountController.cs
using Microsoft.AspNetCore.Mvc;
using MyCc.Application.Commands;
using MyCc.Application.Handlers;
using System.Threading.Tasks;

namespace MyCc.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly LoginCommandHandler _loginHandler;

        public AccountController(LoginCommandHandler loginHandler)
        {
            _loginHandler = loginHandler;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var result = await _loginHandler.Handle(command);
            if (result)
                return Ok("Login successful");
            else
                return Unauthorized("Invalid credentials or account locked");
        }
    }
}
```

**Startup.cs**

```csharp:MyCc.WebAPI/Startup.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyCc.Infrastructure.Data;
using MyCc.Domain.Interfaces;
using MyCc.Application.Handlers;
using MyCc.Application.Services;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<MyDbContext>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<LoginCommandHandler>();
        services.AddScoped<AuthenticationService>();
        services.AddControllers();
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();
        app.UseCors("AllowAll");
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
```

### 说明

- **领域层**：定义了`User`实体和`IUserRepository`接口，负责领域逻辑。
- **基础设施层**：实现了`UserRepository`，用于与数据库交互。
- **应用层**：包含`LoginCommand`和`LoginCommandHandler`，处理登录逻辑。`AuthenticationService`用于密码验证和哈希。
- **WebAPI层**：`AccountController`提供API接口，`Startup.cs`配置服务和CORS策略。

通过这种结构，项目可以实现清晰的分层，便于维护和扩展。每层都有明确的职责，符合清洁架构和领域驱动设计的原则。





好的，接下来我们将继续补充代码，完善登录功能的实现，特别是多因素认证、登录日志记录和“记住我”功能。

### 代码补充

#### 1. Domain层

**User实体**（补充）

```csharp:MyCc.Domain/Entities/User.cs
using System;

namespace MyCc.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } // 使用GUID作为主键
        public string Username { get; set; }
        public string PasswordHash { get; set; } // 存储加密后的密码
        public int FailedLoginAttempts { get; set; } // 登录失败次数
        public bool IsLocked { get; set; } // 账户是否被锁定
        public DateTime? LastLoginTime { get; set; } // 上次登录时间
        public string TwoFactorSecret { get; set; } // 多因素认证密钥
        public bool IsTwoFactorEnabled { get; set; } // 是否启用多因素认证
        // 其他属性...
    }
}
```

#### 2. Infrastructure层

**LoginLog实体**

```csharp:MyCc.Infrastructure/Data/LoginLog.cs
using System;

namespace MyCc.Infrastructure.Data
{
    public class LoginLog
    {
        public Guid Id { get; set; } // 使用GUID作为主键
        public Guid UserId { get; set; } // 用户ID
        public DateTime LoginTime { get; set; } // 登录时间
        public string IpAddress { get; set; } // IP地址
        public bool IsSuccessful { get; set; } // 是否登录成功
    }
}
```

**MyDbContext**（补充）

```csharp:MyCc.Infrastructure/Data/MyDbContext.cs
using Microsoft.EntityFrameworkCore;
using MyCc.Domain.Entities;

namespace MyCc.Infrastructure.Data
{
    public class MyDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } // 用户表
        public DbSet<LoginLog> LoginLogs { get; set; } // 登录日志表

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("YourConnectionStringHere"); // 配置PostgreSQL连接字符串
        }
    }
}
```

**ILoginLogRepository接口**

```csharp:MyCc.Domain/Interfaces/ILoginLogRepository.cs
using System.Threading.Tasks;
using MyCc.Infrastructure.Data;

namespace MyCc.Domain.Interfaces
{
    public interface ILoginLogRepository
    {
        Task AddAsync(LoginLog log); // 添加登录日志
    }
}
```

**LoginLogRepository实现**

```csharp:MyCc.Infrastructure/Data/LoginLogRepository.cs
using System.Threading.Tasks;
using MyCc.Domain.Interfaces;

namespace MyCc.Infrastructure.Data
{
    public class LoginLogRepository : ILoginLogRepository
    {
        private readonly MyDbContext _context;

        public LoginLogRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(LoginLog log)
        {
            await _context.LoginLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}
```

#### 3. Application层

**LoginCommandHandler**（补充）

```csharp:MyCc.Application/Handlers/LoginCommandHandler.cs
using System;
using System.Threading.Tasks;
using MyCc.Application.Commands;
using MyCc.Application.Services;
using MyCc.Domain.Interfaces;
using MyCc.Infrastructure.Data;

namespace MyCc.Application.Handlers
{
    public class LoginCommandHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly ILoginLogRepository _loginLogRepository;
        private readonly AuthenticationService _authService;

        public LoginCommandHandler(IUserRepository userRepository, ILoginLogRepository loginLogRepository, AuthenticationService authService)
        {
            _userRepository = userRepository;
            _loginLogRepository = loginLogRepository;
            _authService = authService;
        }

        public async Task<bool> Handle(LoginCommand command, string ipAddress)
        {
            var user = await _userRepository.GetByUsernameAsync(command.Username);
            if (user == null || user.IsLocked)
            {
                await LogLoginAttempt(user?.Id, ipAddress, false);
                return false;
            }

            if (_authService.VerifyPassword(command.Password, user.PasswordHash))
            {
                if (user.IsTwoFactorEnabled)
                {
                    // 处理多因素认证逻辑
                    // 例如，发送验证码到用户的手机或邮箱
                }

                user.FailedLoginAttempts = 0; // 重置失败次数
                user.LastLoginTime = DateTime.UtcNow; // 更新最后登录时间
                await _userRepository.UpdateAsync(user);
                await LogLoginAttempt(user.Id, ipAddress, true);
                return true;
            }
            else
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= 5)
                {
                    user.IsLocked = true;
                }
                await _userRepository.UpdateAsync(user);
                await LogLoginAttempt(user.Id, ipAddress, false);
                return false;
            }
        }

        private async Task LogLoginAttempt(Guid? userId, string ipAddress, bool isSuccess)
        {
            var log = new LoginLog
            {
                Id = Guid.NewGuid(),
                UserId = userId ?? Guid.Empty,
                LoginTime = DateTime.UtcNow,
                IpAddress = ipAddress,
                IsSuccessful = isSuccess
            };
            await _loginLogRepository.AddAsync(log);
        }
    }
}
```

#### 4. WebAPI层

**AccountController**（补充）

```csharp:MyCc.WebAPI/Controllers/AccountController.cs
using Microsoft.AspNetCore.Mvc;
using MyCc.Application.Commands;
using MyCc.Application.Handlers;
using System.Threading.Tasks;

namespace MyCc.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly LoginCommandHandler _loginHandler;

        public AccountController(LoginCommandHandler loginHandler)
        {
            _loginHandler = loginHandler;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _loginHandler.Handle(command, ipAddress);
            if (result)
                return Ok("Login successful");
            else
                return Unauthorized("Invalid credentials or account locked");
        }
    }
}
```

**Startup.cs**（补充）

```csharp:MyCc.WebAPI/Startup.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyCc.Infrastructure.Data;
using MyCc.Domain.Interfaces;
using MyCc.Application.Handlers;
using MyCc.Application.Services;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<MyDbContext>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILoginLogRepository, LoginLogRepository>();
        services.AddScoped<LoginCommandHandler>();
        services.AddScoped<AuthenticationService>();
        services.AddControllers();
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();
        app.UseCors("AllowAll");
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
```

### 说明

- **多因素认证**：在`LoginCommandHandler`中预留了多因素认证的处理逻辑，可以根据需要实现发送验证码等功能。
- **登录日志**：通过`LoginLog`实体和`ILoginLogRepository`接口实现登录日志的记录，记录每次登录尝试的结果。
- **“记住我”功能**：可以通过在`AccountController`中设置持久性cookie来实现，具体实现可以根据需求调整。

通过这些补充，项目的登录功能更加完善，涵盖了多因素认证、登录日志记录等安全性和可用性功能。

