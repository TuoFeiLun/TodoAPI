using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Http.HttpResults;
using MyApi.Model.TodoItem;
using MyApi.Model.TodoItemDTO;
using MyApi.Model.TodoAdminDTO;
using MyApi.Database;
using System.Security.Claims;
namespace MyApi.Controller;


public class TodoCrud
{

    public static async Task<IResult> GetAllTodosAdmin(TodoDb db)
    {
        return TypedResults.Ok(await db.Todos.Select(x => new TodoAdminDTO(x)).ToArrayAsync());
    }

    public static async Task<IResult> GetTodoAdmin(int id, TodoDb db)
    {
        return await db.Todos.FindAsync(id)
            is Todo todo
                ? TypedResults.Ok(new TodoAdminDTO(todo))
                : TypedResults.NotFound();
    }

    public static async Task<IResult> CreateTodoAdmin(HttpContext context, TodoAdminDTO todoAdminDTO, TodoDb db)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return TypedResults.Unauthorized();
        }
        var todoItem = new Todo
        {
            IsComplete = todoAdminDTO.IsComplete,
            Name = todoAdminDTO.Name,
            Secret = todoAdminDTO.Secret,  // allow to set Secret
            CreatedByUserId = userId,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        db.Todos.Add(todoItem);
        await db.SaveChangesAsync();

        todoAdminDTO = new TodoAdminDTO(todoItem);
        return TypedResults.Created($"/admin/todoitems/{todoItem.Id}", todoAdminDTO);
    }
    public static async Task<IResult> GetAllTodos(HttpContext context, TodoDb db)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return TypedResults.Unauthorized();
        }

        return TypedResults.Ok(await db.Todos.Where(t => t.CreatedByUserId == userId).Select(x => new TodoItemDTO(x)).ToArrayAsync());
    }

    // Get completed todos for current user only
    public static async Task<IResult> GetCompleteTodos(HttpContext context, TodoDb db)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return TypedResults.Unauthorized();
        }

        return TypedResults.Ok(await db.Todos
            .Where(t => t.IsComplete && t.CreatedByUserId == userId)
            .Select(x => new TodoItemDTO(x))
            .ToListAsync());
    }

    // Get single todo - only if created by current user
    public static async Task<IResult> GetTodo(HttpContext context, int id, TodoDb db)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return TypedResults.Unauthorized();
        }

        var todo = await db.Todos.FindAsync(id);
        if (todo is null || todo.CreatedByUserId != userId)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new TodoItemDTO(todo));
    }

    // Create todo with current user as creator
    public static async Task<IResult> CreateTodo(HttpContext context, TodoItemDTO todoItemDTO, TodoDb db)
    {
        // Get current user id from JWT claims
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return TypedResults.Unauthorized();
        }

        // Only set foreign key - EF Core handles navigation property automatically
        var todoItem = new Todo
        {
            Name = todoItemDTO.Name,
            IsComplete = todoItemDTO.IsComplete,
            CreatedByUserId = userId,  // Only need to set foreign key, not navigation property
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        db.Todos.Add(todoItem);
        await db.SaveChangesAsync();

        return TypedResults.Created($"/todoitems/{todoItem.Id}", new TodoItemDTO(todoItem));
    }

    // Update todo - only if created by current user
    public static async Task<IResult> UpdateTodo(HttpContext context, int id, TodoItemDTO todoItemDTO, TodoDb db)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // this id is user id.
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return TypedResults.Unauthorized();
        }

        var todo = await db.Todos.FindAsync(id); // this id is todoitem id.
        if (todo is null || todo.CreatedByUserId != userId) // check if the todoitem is created by the current user.
        {
            return TypedResults.NotFound();
        }

        todo.Name = todoItemDTO.Name;
        todo.IsComplete = todoItemDTO.IsComplete;
        todo.UpdatedAt = DateTime.Now;

        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    // Delete todo - only if created by current user
    public static async Task<IResult> DeleteTodo(HttpContext context, int id, TodoDb db)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return TypedResults.Unauthorized();
        }

        var todo = await db.Todos.FindAsync(id);
        if (todo is null || todo.CreatedByUserId != userId)
        {
            return TypedResults.NotFound();
        }

        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }
}

