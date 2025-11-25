using Microsoft.AspNetCore.Authorization;
using MyApi.Controller;
using MyApi.Database;
using MyApi.DTOs;
using static MyApi.Authorization.AuthorizePolicies;
namespace MinAPISeparateFile;

public static class TodoEndpoints
{
    public static void MapTodoEndpoints(WebApplication app)
    {
        // Regular todo endpoints - all authenticated users can view
        var todoItems = app.MapGroup("/todoitems");

        todoItems.MapGet("/", [Authorize(ViewerTodoItem)] (TodoDb db) =>
            TodoCrud.GetAllTodos(db));
        todoItems.MapGet("/complete", [Authorize(ViewerTodoItem)] (TodoDb db) =>
            TodoCrud.GetCompleteTodos(db));
        todoItems.MapGet("/{id}", [Authorize(ViewerTodoItem)] (int id, TodoDb db) =>
            TodoCrud.GetTodo(id, db));
        todoItems.MapPost("/", [Authorize(CrudTodoItem)] (TodoItemDTO todoItemDTO, TodoDb db) =>
            TodoCrud.CreateTodo(todoItemDTO, db));
        todoItems.MapPut("/{id}", [Authorize(CrudTodoItem)] (int id, TodoItemDTO todoItemDTO, TodoDb db) =>
            TodoCrud.UpdateTodo(id, todoItemDTO, db));
        todoItems.MapDelete("/{id}", [Authorize(CrudTodoItem)] (int id, TodoDb db) =>
            TodoCrud.DeleteTodo(id, db));

        // Admin-only todo endpoints - includes secret field
        var adminItems = app.MapGroup("/admin/todoitems");

        adminItems.MapGet("/", (TodoDb db) =>
            TodoCrud.GetAllTodosAdmin(db));
        adminItems.MapGet("/{id}", (int id, TodoDb db) =>
            TodoCrud.GetTodoAdmin(id, db));
        adminItems.MapPost("/", (TodoAdminDTO todoAdminDTO, TodoDb db) =>
            TodoCrud.CreateTodoAdmin(todoAdminDTO, db));
    }

}