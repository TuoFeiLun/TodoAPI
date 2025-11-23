using MyApi.Controller;
namespace MinAPISeparateFile;

public static class TodoEndpoints
{
    public static void MapTodoEndpoints(WebApplication app)
    {
        var todoItems = app.MapGroup("/todoitems");

        todoItems.MapGet("/", TodoCrud.GetAllTodos);
        todoItems.MapGet("/complete", TodoCrud.GetCompleteTodos);
        todoItems.MapGet("/{id}", TodoCrud.GetTodo);
        todoItems.MapPost("/", TodoCrud.CreateTodo);
        todoItems.MapPut("/{id}", TodoCrud.UpdateTodo);
        todoItems.MapDelete("/{id}", TodoCrud.DeleteTodo);


        var adminItems = app.MapGroup("/admin/todoitems");

        adminItems.MapGet("/", TodoCrud.GetAllTodosAdmin);
        adminItems.MapGet("/{id}", TodoCrud.GetTodoAdmin);
        adminItems.MapPost("/", TodoCrud.CreateTodoAdmin);
    }

}