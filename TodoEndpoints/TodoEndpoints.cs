using Microsoft.AspNetCore.Authorization;
using MyApi.Controller;
using MyApi.Database;
using MyApi.DTOs;
namespace MinAPISeparateFile;

public static class TodoEndpoints
{
    public static void MapTodoEndpoints(WebApplication app)
    {
        var todoItems = app.MapGroup("/todoitems");

        todoItems.MapGet("/", [Authorize("viewer_todoitem")] (TodoDb db) =>
            TodoCrud.GetAllTodos(db));
        todoItems.MapGet("/complete", [Authorize("viewer_todoitem")] (TodoDb db) =>
            TodoCrud.GetCompleteTodos(db));
        todoItems.MapGet("/{id}", [Authorize("viewer_todoitem")] (int id, TodoDb db) =>
            TodoCrud.GetTodo(id, db));
        todoItems.MapPost("/", [Authorize("crud_todoitem")] (TodoItemDTO todoItemDTO, TodoDb db) =>
            TodoCrud.CreateTodo(todoItemDTO, db));
        todoItems.MapPut("/{id}", [Authorize("crud_todoitem")] (int id, TodoItemDTO todoItemDTO, TodoDb db) =>
            TodoCrud.UpdateTodo(id, todoItemDTO, db));
        todoItems.MapDelete("/{id}", [Authorize("crud_todoitem")] (int id, TodoDb db) =>
            TodoCrud.DeleteTodo(id, db));


        var adminItems = app.MapGroup("/admin/todoitems");

        adminItems.MapGet("/", (TodoDb db) =>
            TodoCrud.GetAllTodosAdmin(db));
        adminItems.MapGet("/{id}", (int id, TodoDb db) =>
            TodoCrud.GetTodoAdmin(id, db));
        adminItems.MapPost("/", (TodoAdminDTO todoAdminDTO, TodoDb db) =>
            TodoCrud.CreateTodoAdmin(todoAdminDTO, db));
    }

}