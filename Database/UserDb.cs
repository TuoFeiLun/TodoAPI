using Microsoft.EntityFrameworkCore;
using MyApi.Model.User;

namespace MyApi.Database;
public class UserDb : DbContext
{
    public UserDb(DbContextOptions<UserDb> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
}
