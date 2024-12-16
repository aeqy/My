using MyCc.WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 加载配置文件
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// 配置数据库服务
builder.Services.ConfigureServicesDatabase(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddSwaggerDocumentation(); // 启用Swagger
builder.Logging.ClearProviders(); // 清除默认的日志提供程序
builder.Logging.AddConsole(); // 添加控制台日志提供程序

// 注册应用服务和仓储
builder.Services.AddApiServices(builder.Configuration);

// 添加CORS服务
builder.Services.AddCors(options =>
{
    // ReSharper disable once VariableHidesOuterVariable
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()    // 允许所有来源
            .AllowAnyMethod()    // 允许所有HTTP方法
            .AllowAnyHeader();   // 允许所有请求头
    });
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();// 启用Swagger
    app.UseSwaggerDocumentation();  // 启用Swagger
}

app.UseDefaultFiles();// 使用默认文件中间件
app.UseStaticFiles();// 使用静态文件中间件

app.UseRouting();   // 启用路由
app.UseCors("AllowAll"); // 启用跨域
app.UseHttpsRedirection();  // 启用HTTPS重定向
app.Run();  // 启动应用程序