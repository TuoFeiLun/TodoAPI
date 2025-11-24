using Microsoft.AspNetCore.Authorization;
using MyApi.Controller;
namespace MinAPISeparateFile;

public static class TodoEndpoints
{
    public static void MapTodoEndpoints(WebApplication app)
    {
        var todoItems = app.MapGroup("/todoitems");

        todoItems.MapGet("/", [Authorize("admin_and_editor")] () => TodoCrud.GetAllTodos);
        todoItems.MapGet("/complete", [Authorize("editor_user")] () => TodoCrud.GetCompleteTodos);
        todoItems.MapGet("/{id}", [Authorize("editor_user")] () => TodoCrud.GetTodo);
        todoItems.MapPost("/", [Authorize("create_and_delete_user")] () => TodoCrud.CreateTodo);
        todoItems.MapPut("/{id}", [Authorize("editor_user")] () => TodoCrud.UpdateTodo);
        todoItems.MapDelete("/{id}", [Authorize("create_and_delete_user")] () => TodoCrud.DeleteTodo);


        var adminItems = app.MapGroup("/admin/todoitems");

        adminItems.MapGet("/", TodoCrud.GetAllTodosAdmin);
        adminItems.MapGet("/{id}", TodoCrud.GetTodoAdmin);
        adminItems.MapPost("/", TodoCrud.CreateTodoAdmin);
    }

}