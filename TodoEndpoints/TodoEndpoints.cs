using Microsoft.AspNetCore.Authorization;
using MyApi.Controller;
using MyApi.Database;
using MyApi.Model.TodoItemDTO;
using MyApi.Model.TodoAdminDTO;
using static MyApi.Authorization.AuthorizePolicies;
using System.Text.Json;

namespace MinAPISeparateFile;

public static class TodoEndpoints
{
    public static void MapTodoEndpoints(WebApplication app)
    {
        // User todo endpoints - users can only access their own todos
        var todoItems = app.MapGroup("/todoitems");

        todoItems.MapGet("/", [Authorize(ViewerTodoItem)] (HttpContext context, AppDbContext db) =>
            TodoCrud.GetAllTodos(context, db));
        todoItems.MapGet("/complete", [Authorize(ViewerTodoItem)] (HttpContext context, AppDbContext db) =>
            TodoCrud.GetCompleteTodos(context, db));
        todoItems.MapGet("/{id}", [Authorize(ViewerTodoItem)] (HttpContext context, int id, AppDbContext db) =>
            TodoCrud.GetTodo(context, id, db));
        todoItems.MapPost("/", [Authorize(CrudTodoItem)] (HttpContext context, TodoItemDTO todoItemDTO, AppDbContext db) =>
            TodoCrud.CreateTodo(context, todoItemDTO, db));
        todoItems.MapPatch("/{id}", [Authorize(CrudTodoItem)] (HttpContext context, int id, JsonElement patchData, AppDbContext db) =>
            TodoCrud.UpdateTodo(context, id, patchData, db));
        todoItems.MapDelete("/{id}", [Authorize(CrudTodoItem)] (HttpContext context, int id, AppDbContext db) =>
            TodoCrud.DeleteTodo(context, id, db));

        // Admin-only todo endpoints - can access all todos
        var adminItems = app.MapGroup("/admin/todoitems");

        adminItems.MapGet("/", [Authorize(CreateAndDeleteUser)] (AppDbContext db) =>
            TodoCrud.GetAllTodosAdmin(db));
        adminItems.MapGet("/{id}", [Authorize(CreateAndDeleteUser)] (int id, AppDbContext db) =>
            TodoCrud.GetTodoAdmin(id, db));
        adminItems.MapPost("/", [Authorize(CreateAndDeleteUser)] (HttpContext context, TodoAdminDTO todoAdminDTO, AppDbContext db) =>
            TodoCrud.CreateTodoAdmin(context, todoAdminDTO, db));
    }
}