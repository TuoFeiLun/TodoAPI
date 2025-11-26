using Microsoft.AspNetCore.Authorization;
using MyApi.Controller;
using MyApi.Database;
using MyApi.Model.TodoItemDTO;
using MyApi.Model.TodoAdminDTO;
using static MyApi.Authorization.AuthorizePolicies;
namespace MinAPISeparateFile;

public static class TodoEndpoints
{
    public static void MapTodoEndpoints(WebApplication app)
    {
        // User todo endpoints - users can only access their own todos
        var todoItems = app.MapGroup("/todoitems");

        todoItems.MapGet("/", [Authorize(ViewerTodoItem)] (HttpContext context, TodoDb db) =>
            TodoCrud.GetAllTodos(context, db));
        todoItems.MapGet("/complete", [Authorize(ViewerTodoItem)] (HttpContext context, TodoDb db) =>
            TodoCrud.GetCompleteTodos(context, db));
        todoItems.MapGet("/{id}", [Authorize(ViewerTodoItem)] (HttpContext context, int id, TodoDb db) =>
            TodoCrud.GetTodo(context, id, db));
        todoItems.MapPost("/", [Authorize(CrudTodoItem)] (HttpContext context, TodoItemDTO todoItemDTO, TodoDb db) =>
            TodoCrud.CreateTodo(context, todoItemDTO, db));
        todoItems.MapPut("/{id}", [Authorize(CrudTodoItem)] (HttpContext context, int id, TodoItemDTO todoItemDTO, TodoDb db) =>
            TodoCrud.UpdateTodo(context, id, todoItemDTO, db));
        todoItems.MapDelete("/{id}", [Authorize(CrudTodoItem)] (HttpContext context, int id, TodoDb db) =>
            TodoCrud.DeleteTodo(context, id, db));

        // Admin-only todo endpoints - can access all todos
        var adminItems = app.MapGroup("/admin/todoitems");

        adminItems.MapGet("/", [Authorize(CreateAndDeleteUser)] (TodoDb db) =>
            TodoCrud.GetAllTodosAdmin(db));
        adminItems.MapGet("/{id}", [Authorize(CreateAndDeleteUser)] (int id, TodoDb db) =>
            TodoCrud.GetTodoAdmin(id, db));
        adminItems.MapPost("/", [Authorize(CreateAndDeleteUser)] (HttpContext context, TodoAdminDTO todoAdminDTO, TodoDb db) =>
            TodoCrud.CreateTodoAdmin(context, todoAdminDTO, db));
    }

}