using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MyApi.Database;
using MinAPISeparateFile;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.IncludeFields = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "TodoAPI";
    config.Title = "TodoAPI v1";
    config.Version = "v1";
});
builder.Services.AddAuthorization(o => o.AddPolicy("AdminsOnly",
                                  b => b.RequireClaim("admin", "true")));


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "TodoAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}
app.UseAuthorization();
app.MapGet("/admin", [Authorize("AdminsOnly")] () =>
                             "The /admin endpoint is for admins only.");
app.MapGet("/login", [AllowAnonymous] () => "This endpoint is for all roles.");

app.MapGet("/", () => TypedResults.Ok(new { message = "Hello World" }));
app.MapGet("/hello", () => "Hello named route")
   .WithName("hi");

app.MapGet("/1", () => Results.Redirect("/"));


TodoEndpoints.MapTodoEndpoints(app);

// 管理端点方法
app.Urls.Add("http://localhost:3000");
app.Urls.Add("http://localhost:4000");
app.Run();

