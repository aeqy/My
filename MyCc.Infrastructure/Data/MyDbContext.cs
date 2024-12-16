using Microsoft.EntityFrameworkCore;

namespace MyCc.Infrastructure.Data;

public class MyDbContext(DbContextOptions<MyDbContext> options) : DbContext(options)
{
    
}