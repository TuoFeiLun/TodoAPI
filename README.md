# TodoAPI

A RESTful API built with ASP.NET Core 8 Minimal API, featuring JWT authentication, role-based authorization, and user-todo relationships.

## Features

- **JWT Authentication** - Secure token-based authentication
- **Role-Based Authorization** - Admin, Editor, Viewer roles with different permissions
- **Password Hashing** - BCrypt encryption for secure password storage
- **User-Todo Relationship** - Each todo is linked to its creator
- **Swagger/OpenAPI** - Interactive API documentation

## Tech Stack

- .NET 8 Minimal API
- Entity Framework Core with PostgreSQL
- JWT Bearer Authentication
- BCrypt.Net for password hashing
- NSwag for OpenAPI documentation

## Prerequisites

- .NET 8 SDK
- PostgreSQL database server

## Getting Started

### 1. Configure Database Connection

Update `appsettings.json` with your PostgreSQL connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=todoapi;Username=postgres;Password=postgres"
  }
}
```

### 2. Run the Application

```bash
cd MyApi
dotnet restore
dotnet run
```

The application will automatically apply EF Core migrations on startup.

### EF Core Migrations Commands

```bash
# Add a new migration
dotnet ef migrations add MigrationName

# Apply migrations manually
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

The API will be available at:
- http://localhost:3000
- http://localhost:4000

Swagger UI: http://localhost:3000/swagger

## API Endpoints

### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/login` | Login and get JWT token |

### Users (Admin only)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/users` | Get all users |
| GET | `/users/{id}` | Get user by ID |
| POST | `/users` | Create new user |
| PUT | `/users/{id}` | Update user |
| DELETE | `/users/{id}` | Delete user |
| PUT | `/users/role/{id}` | Change user role |

### Users (Authenticated)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/users/me` | Get current user info |
| PUT | `/users/change-password` | Change password |

### Todos (User's own)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/todoitems` | Get user's todos |
| GET | `/todoitems/{id}` | Get todo by ID |
| POST | `/todoitems` | Create todo |
| PUT | `/todoitems/{id}` | Update todo |
| DELETE | `/todoitems/{id}` | Delete todo |

### Todos (Admin)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/admin/todoitems` | Get all todos |
| GET | `/admin/todoitems/{id}` | Get any todo |
| POST | `/admin/todoitems` | Create todo with secret |

## Test Accounts

| Email | Password | Role |
|-------|----------|------|
| admin@test.com | Admin123! | admin |
| user@test.com | User123! | editor |
| test@test.com | Test123! | viewer |

## Authorization Policies

| Policy | Roles | Description |
|--------|-------|-------------|
| `create_and_delete_user` | admin | Manage users |
| `change_user_role` | admin | Change user roles |
| `editor_user` | admin, editor | Edit users |
| `viewer_user` | admin, editor, viewer | View users |
| `viewer_todoitem` | admin, editor, viewer | View todos |
| `crud_todoitem` | editor | Create/Update/Delete todos |

## Project Structure

```
MyApi/
├── Authorization/       # Authorization policies
├── Config/              # JWT configuration
├── Controller/          # Business logic
├── Database/            # AppDbContext for PostgreSQL
├── Middleware/          # Global exception handler
├── Migrations/          # EF Core database migrations
├── Model/               # Entity models and DTOs
├── Services/            # Password hashing service
└── TodoEndpoints/       # API endpoint mappings
```

## Database Schema

### Users Table
| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary Key (auto-increment) |
| Name | varchar(100) | User name |
| Email | varchar(255) | Unique email |
| Password | text | BCrypt hashed password |
| Role | varchar(50) | admin/editor/viewer |
| IsAdmin | boolean | Admin flag |
| CreatedAt | timestamp | UTC creation time |
| UpdatedAt | timestamp | UTC update time |

### Todos Table
| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary Key (auto-increment) |
| Name | varchar(500) | Todo name |
| IsComplete | boolean | Completion status |
| Secret | text | Admin-only secret |
| CreatedByUserId | int | Foreign Key to Users |
| CreatedAt | timestamp | UTC creation time |
| UpdatedAt | timestamp | UTC update time |

## License

MIT License

Copyright (c) 2025

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

