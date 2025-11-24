using Microsoft.AspNetCore.Authorization;
using MyApi.Controller;
namespace MinAPISeparateFile;

public static class TodoEndpoints
{
    public static void MapTodoEndpoints(WebApplication app)
    {
        var todoItems = app.MapGroup("/todoitems");

        todoItems.MapGet("/", [Authorize("viewer_todoitem")] () => TodoCrud.GetAllTodos);
        todoItems.MapGet("/complete", [Authorize("viewer_todoitem")] () => TodoCrud.GetCompleteTodos);
        todoItems.MapGet("/{id}", [Authorize("viewer_todoitem")] () => TodoCrud.GetTodo);
        todoItems.MapPost("/", [Authorize("crud_todoitem")] () => TodoCrud.CreateTodo);
        todoItems.MapPut("/{id}", [Authorize("crud_todoitem")] () => TodoCrud.UpdateTodo);
        todoItems.MapDelete("/{id}", [Authorize("crud_todoitem")] () => TodoCrud.DeleteTodo);


        var adminItems = app.MapGroup("/admin/todoitems");

        adminItems.MapGet("/", TodoCrud.GetAllTodosAdmin);
        adminItems.MapGet("/{id}", TodoCrud.GetTodoAdmin);
        adminItems.MapPost("/", TodoCrud.CreateTodoAdmin);
    }

}