using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Http.HttpResults;
using MyApi.Model.TodoItem;
using MyApi.Model.TodoItemDTO;
using MyApi.Model.TodoAdminDTO;
using MyApi.Database;
using System.Security.Claims;
using System.Text.Json;

namespace MyApi.Controller;

public class TodoCrud
{
    public static async Task<IResult> GetAllTodosAdmin(AppDbContext db)
    {
        return TypedResults.Ok(await db.Todos.Select(x => new TodoAdminDTO(x)).ToArrayAsync());
    }

    public static async Task<IResult> GetTodoAdmin(int id, AppDbContext db)
    {
        return await db.Todos.FindAsync(id)
            is Todo todo
                ? TypedResults.Ok(new TodoAdminDTO(todo))
                : TypedResults.NotFound();
    }

    public static async Task<IResult> CreateTodoAdmin(HttpContext context, TodoAdminDTO todoAdminDTO, AppDbContext db)
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
            Secret = todoAdminDTO.Secret,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Todos.Add(todoItem);
        await db.SaveChangesAsync();

        todoAdminDTO = new TodoAdminDTO(todoItem);
        return TypedResults.Created($"/admin/todoitems/{todoItem.Id}", todoAdminDTO);
    }

    public static async Task<IResult> GetAllTodos(HttpContext context, AppDbContext db)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return TypedResults.Unauthorized();
        }

        return TypedResults.Ok(await db.Todos
            .Where(t => t.CreatedByUserId == userId)
            .Select(x => new TodoItemDTO(x))
            .ToArrayAsync());
    }

    public static async Task<IResult> GetCompleteTodos(HttpContext context, AppDbContext db)
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

    public static async Task<IResult> GetTodo(HttpContext context, int id, AppDbContext db)
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

    public static async Task<IResult> CreateTodo(HttpContext context, TodoItemDTO todoItemDTO, AppDbContext db)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return TypedResults.Unauthorized();
        }

        var todoItem = new Todo
        {
            Name = todoItemDTO.Name,
            IsComplete = todoItemDTO.IsComplete,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Todos.Add(todoItem);
        await db.SaveChangesAsync();

        return TypedResults.Created($"/todoitems/{todoItem.Id}", new TodoItemDTO(todoItem));
    }

    public static async Task<IResult> UpdateTodo(HttpContext context, int id, JsonElement patchData, AppDbContext db)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return TypedResults.Unauthorized();
        }

        var todo = await db.Todos.FindAsync(id);

        // using reflection to get all properties of Todo class
        var todoProperties = typeof(Todo)
            .GetProperties()
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // check if the keys in the request are valid properties of the Todo class
        foreach (var property in patchData.EnumerateObject())
        {
            if (!todoProperties.Contains(property.Name))
            {
                return TypedResults.BadRequest(new { message = $"Invalid property: {property.Name}" });
            }
        }


        if (todo is null || todo.CreatedByUserId != userId)
        {
            return TypedResults.NotFound();
        }

        // Only update fields that are present in the request
        if (patchData.TryGetProperty("name", out JsonElement nameElement))
        {
            todo.Name = nameElement.GetString();
            todo.UpdatedAt = DateTime.UtcNow;
        }
        if (patchData.TryGetProperty("isComplete", out JsonElement isCompleteElement))
        {
            todo.IsComplete = isCompleteElement.GetBoolean();
            todo.UpdatedAt = DateTime.UtcNow;
        }

        // if put DataTime.UtcNow in here, it will update even if the request is empty and or properties are not present in the request.

        await db.SaveChangesAsync();
        return TypedResults.Ok(new TodoItemDTO(todo));
    }

    public static async Task<IResult> DeleteTodo(HttpContext context, int id, AppDbContext db)
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

