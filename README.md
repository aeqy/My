好的，下面是如何使用`dotnet`命令行工具搭建一个使用EF Core的项目，项目名称为MyCc，并将`Core`替换为`Domain`。我们将使用GUID作为实体的主键。

### 使用`dotnet`命令搭建项目

1. **创建解决方案**：
   ```bash
   dotnet new sln -n MyCc
   ```

2. **创建项目结构**：

    - **Domain层**：
      ```bash
      dotnet new classlib -n MyCc.Domain
      mkdir -p MyCc.Domain/Entities
      mkdir -p MyCc.Domain/Interfaces
      ```

    - **Infrastructure层**：
      ```bash
      dotnet new classlib -n MyCc.Infrastructure
      mkdir -p MyCc.Infrastructure/Data
      ```

    - **Application层**：
      ```bash
      dotnet new classlib -n MyCc.Application
      mkdir -p MyCc.Application/Commands
      mkdir -p MyCc.Application/Queries
      ```

    - **WebAPI层**：
      ```bash
      dotnet new webapi -n MyCc.WebAPI
      ```

3. **将项目添加到解决方案**：
   ```bash
   dotnet sln MyCc.sln add MyCc.Domain/MyCc.Domain.csproj
   dotnet sln MyCc.sln add MyCc.Infrastructure/MyCc.Infrastructure.csproj
   dotnet sln MyCc.sln add MyCc.Application/MyCc.Application.csproj
   dotnet sln MyCc.sln add MyCc.WebAPI/MyCc.WebAPI.csproj
   ```

4. **添加项目引用**：
   ```bash
   dotnet add MyCc.Infrastructure/MyCc.Infrastructure.csproj reference MyCc.Domain/MyCc.Domain.csproj
   dotnet add MyCc.Application/MyCc.Application.csproj reference MyCc.Domain/MyCc.Domain.csproj
   dotnet add MyCc.Application/MyCc.Application.csproj reference MyCc.Infrastructure/MyCc.Infrastructure.csproj
   dotnet add MyCc.WebAPI/MyCc.WebAPI.csproj reference MyCc.Application/MyCc.Application.csproj
   ```

5. **安装EF Core和PostgreSQL包**：
   ```bash
   dotnet add MyCc.Infrastructure/MyCc.Infrastructure.csproj package Microsoft.EntityFrameworkCore
   dotnet add MyCc.Infrastructure/MyCc.Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
   ```

### 项目架构目录

```
MyCc/
│
├── MyCc.sln
│
├── MyCc.Domain/
│   ├── MyCc.Domain.csproj
│   ├── Entities/
│   │   └── MyEntity.cs
│   └── Interfaces/
│       └── IMyEntityRepository.cs
│
├── MyCc.Infrastructure/
│   ├── MyCc.Infrastructure.csproj
│   ├── Data/
│   │   ├── MyDbContext.cs
│   │   └── MyEntityRepository.cs
│
├── MyCc.Application/
│   ├── MyCc.Application.csproj
│   ├── Commands/
│   │   └── CreateMyEntityCommand.cs
│   └── Queries/
│       └── GetMyEntityQuery.cs
│
└── MyCc.WebAPI/
    ├── MyCc.WebAPI.csproj
    ├── Controllers/
    │   └── MyEntityController.cs
    └── Startup.cs
```

### 说明

- **Domain**：包含领域模型和接口。`Entities`文件夹存放领域实体，例如`MyEntity.cs`，使用GUID作为主键。`Interfaces`文件夹存放领域接口。

- **Infrastructure**：包含数据库上下文和存储库实现。`Data`文件夹中的`MyDbContext.cs`配置PostgreSQL数据库连接，`MyEntityRepository.cs`实现领域接口。

- **Application**：包含CQRS命令和查询。`Commands`文件夹存放命令处理程序，例如`CreateMyEntityCommand.cs`。`Queries`文件夹存放查询处理程序，例如`GetMyEntityQuery.cs`。

- **WebAPI**：包含API控制器和CORS配置。`Controllers`文件夹中的`MyEntityController.cs`处理HTTP请求。`Startup.cs`配置CORS策略。

通过这种结构，项目可以实现清晰的分层，便于维护和扩展。每层都有明确的职责，符合清洁架构和领域驱动设计的原则。





在软件开发中，使用Git进行版本控制时，遵循良好的提交代码规范可以提高代码的可读性和可维护性。以下是一些常见的Git提交代码规范：

## Git提交信息规范

1. **提交信息格式**：
    - 提交信息通常由标题、正文和脚注组成。
    - 标题：简洁明了，通常不超过50个字符。
    - 正文：详细描述更改内容，解释为什么需要这些更改。
    - 脚注：用于引用相关的任务、问题或文档。

2. **使用动词开头**：
    - 标题应以动词的祈使句形式开头，例如：`Add`、`Fix`、`Update`、`Remove`等。
    - 示例：`Add user authentication feature`

3. **保持一致性**：
    - 使用一致的格式和风格，确保团队成员都遵循相同的规范。

4. **关联问题或任务**：
    - 在提交信息中引用相关的任务或问题编号，便于追踪。
    - 示例：`Fix login bug (#123)`

5. **避免提交无关更改**：
    - 每次提交应只包含一个逻辑上的更改，避免将多个不相关的更改混合在一起。

6. **使用英文**：
    - 通常建议使用英文撰写提交信息，特别是在国际化团队中。

### 提交信息示例

```plaintext
feat: Add user authentication

- Implemented login and registration functionality
- Added JWT token generation and validation
- Updated user model to include password hashing

Resolves: #123
```

### 提交类型标签

可以在提交信息中使用标签来标识提交的类型，常见的标签包括：

- `feat`：新功能
- `fix`：修复bug
- `docs`：文档更新
- `style`：代码格式（不影响代码运行的变动）
- `refactor`：重构（即不是新增功能，也不是修复bug的代码变动）
- `test`：增加测试
- `chore`：构建过程或辅助工具的变动

通过遵循这些规范，可以提高代码库的可读性和可维护性，帮助团队更有效地协作。
