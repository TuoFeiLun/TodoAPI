using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MyApi.Database;
using MinAPISeparateFile;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MyApi.Config;
using MyApi.Controller;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDbContext<UserDb>(opt => opt.UseInMemoryDatabase("UserList"));
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




// Configure Authentication using JWT Bearer
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = JwtSettings.Issuer,
            ValidAudience = JwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(JwtSettings.SecretKey))
        };
    });

// Configure Authorization policy
builder.Services.AddAuthorization(o => o.AddPolicy("AdminsOnly",
                                  b => b.RequireClaim("admin", "true")));

// Configure CORS for frontend (important for SPA)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:5173", "http://localhost:3001") // Add your frontend URLs
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});


var app = builder.Build();

// RULE TEST OK
// Seed test users for development
using (var scope = app.Services.CreateScope())
{
    var userDb = scope.ServiceProvider.GetRequiredService<UserDb>();

    // Ensure database is created
    userDb.Database.EnsureCreated();

    // Add test users if they don't exist
    if (!userDb.Users.Any())
    {
        userDb.Users.AddRange(
            new MyApi.Model.User.User("Admin", "Admin123!", "admin@test.com", true),
            new MyApi.Model.User.User("Regular", "User123!", "user@test.com", false),
            new MyApi.Model.User.User("Test", "Test123!", "test@test.com", false)
        );
        userDb.SaveChanges();
    }
}


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

// Enable CORS
app.UseCors();

// IMPORTANT: Authentication must be before Authorization
app.UseAuthentication();
app.UseAuthorization();

// Login endpoint - validates credentials and returns JWT token
app.MapPost("/login", LoginController.Login);


// Check who is logged in
app.MapGet("/whoami", [Authorize] (HttpContext context) =>
{
    var user = context.User;
    var claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList();
    return Results.Ok(new
    {
        username = user.Identity?.Name,
        isAuthenticated = user.Identity?.IsAuthenticated,
        claims
    });
});

// Admin-only endpoint
app.MapGet("/admin", [Authorize("AdminsOnly")] () =>
    "The /admin endpoint is for admins only. You have the 'admin=true' claim!");

// Any authenticated user can access
app.MapGet("/protected", [Authorize] (HttpContext context) =>
    $"Hello {context.User.Identity?.Name}, you are authenticated!");

app.MapGet("/", () => TypedResults.Ok(new { message = "Hello World !" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }));
app.MapGet("/hello", () => "Hello named route")
   .WithName("hi");

app.MapGet("/1", () => Results.Redirect("/"));


TodoEndpoints.MapTodoEndpoints(app);
UserEndPoints.MapUserEndpoints(app);

// 管理端点方法
app.Urls.Add("http://localhost:3000");
app.Urls.Add("http://localhost:4000");
app.Run();

