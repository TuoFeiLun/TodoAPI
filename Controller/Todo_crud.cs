using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Http.HttpResults;
using MyApi.Model;      // 导入 Todo 模型
using MyApi.DTOs;        // 导入 DTO 类
using MyApi.Database;    // 导入 TodoDb

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

    public static async Task<IResult> CreateTodoAdmin(TodoAdminDTO todoAdminDTO, TodoDb db)
    {
        var todoItem = new Todo
        {
            IsComplete = todoAdminDTO.IsComplete,
            Name = todoAdminDTO.Name,
            Secret = todoAdminDTO.Secret  // 允许设置 Secret
        };

        db.Todos.Add(todoItem);
        await db.SaveChangesAsync();

        todoAdminDTO = new TodoAdminDTO(todoItem);
        return TypedResults.Created($"/admin/todoitems/{todoItem.Id}", todoAdminDTO);
    }
    public static async Task<IResult> GetAllTodos(TodoDb db)
    {
        return TypedResults.Ok(await db.Todos.Select(x => new TodoItemDTO(x)).ToArrayAsync());
    }

    public static async Task<IResult> GetCompleteTodos(TodoDb db)
    {
        return TypedResults.Ok(await db.Todos.Where(t => t.IsComplete).Select(x => new TodoItemDTO(x)).ToListAsync());
    }

    public static async Task<IResult> GetTodo(int id, TodoDb db)
    {
        return await db.Todos.FindAsync(id)
            is Todo todo
                ? TypedResults.Ok(new TodoItemDTO(todo))
                : TypedResults.NotFound();
    }

    public static async Task<IResult> CreateTodo(TodoItemDTO todoItemDTO, TodoDb db)
    {
        var todoItem = new Todo
        {
            IsComplete = todoItemDTO.IsComplete,
            Name = todoItemDTO.Name
        };

        db.Todos.Add(todoItem);
        await db.SaveChangesAsync();

        todoItemDTO = new TodoItemDTO(todoItem);

        return TypedResults.Created($"/todoitems/{todoItem.Id}", todoItemDTO);
    }

    public static async Task<IResult> UpdateTodo(int id, TodoItemDTO todoItemDTO, TodoDb db)
    {
        var todo = await db.Todos.FindAsync(id);

        if (todo is null) return TypedResults.NotFound();

        todo.Name = todoItemDTO.Name;
        todo.IsComplete = todoItemDTO.IsComplete;

        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    public static async Task<IResult> DeleteTodo(int id, TodoDb db)
    {
        if (await db.Todos.FindAsync(id) is Todo todo)
        {
            db.Todos.Remove(todo);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }

        return TypedResults.NotFound();
    }
}

