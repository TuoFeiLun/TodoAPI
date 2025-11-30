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
using MyApi.Authorization;
using MyApi.Model.User;

var builder = WebApplication.CreateBuilder(args);

// Configure PostgreSQL database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=todoapi;Username=postgres;Password=postgres";
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add global exception handler
builder.Services.AddExceptionHandler<MyApi.Middleware.GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.IncludeFields = true;
    options.SerializerOptions.AllowTrailingCommas = false;  // Allow trailing commas in JSON
    options.SerializerOptions.ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip;  // Allow comments in JSON
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

// not using fluent api
// MyApi.Authorization.AuthorizePolicies.AddAuthorizationPolicies(builder.Services.AddAuthorizationBuilder());

// Configure Authorization policies - centralized configuration using fluent API
builder.Services.AddAuthorizationBuilder().AddAuthorizationPolicies();


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


// Apply migrations and seed test users for development
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Apply pending migrations
    db.Database.Migrate();

    // Add test users with hashed passwords if they don't exist
    if (!db.Users.Any())
    {
        db.Users.AddRange(
            new MyApi.Model.User.User("Admin", MyApi.Services.PasswordHashService.HashPassword("Admin123!"), "admin@test.com", UserRole.Admin, true),
            new MyApi.Model.User.User("Regular", MyApi.Services.PasswordHashService.HashPassword("User123!"), "user@test.com", UserRole.Editor, false),
            new MyApi.Model.User.User("Regular2", MyApi.Services.PasswordHashService.HashPassword("User123!"), "user2@test.com", UserRole.Editor, false),
            new MyApi.Model.User.User("Test", MyApi.Services.PasswordHashService.HashPassword("Test123!"), "test@test.com", UserRole.Viewer, false)
        );
        db.SaveChanges();
    }
}

// Use global exception handler (must be early in pipeline)
app.UseExceptionHandler();

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
app.MapGet("/whoami", (HttpContext context) =>
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
app.MapGet("/admin", () =>
    "The /admin endpoint is for admins only. You have the 'admin=true' claim!")
    .RequireAuthorization(MyApi.Authorization.AuthorizePolicies.CreateAndDeleteUser);

// Any authenticated user can access
app.MapGet("/protected", [Authorize] (HttpContext context) =>
    $"Hello {context.User.Identity?.Name}, you are authenticated!");

app.MapGet("/", () => TypedResults.Ok(new { message = "Hello World! " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC" }));
app.MapGet("/hello", () => "Hello named route")
   .WithName("hi");

app.MapGet("/1", () => Results.Redirect("/"));


TodoEndpoints.MapTodoEndpoints(app);
UserEndPoints.MapUserEndpoints(app);

// map the endpoints
app.Urls.Add("http://localhost:3000");
app.Urls.Add("http://localhost:4000");
app.Run();

