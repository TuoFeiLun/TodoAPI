using Microsoft.EntityFrameworkCore;
using MyApi.Model.TodoItem;

namespace MyApi.Database;

public class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options)
        : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();
}
