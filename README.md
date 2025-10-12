# TopicalBirdAPI

**TopicalBirdAPI** is a clean, modular REST API built with **ASP.NET Core** and **Entity Framework Core**, 
providing a backend for a discussion-based social platform (similar to Reddit).  
It supports **user management**, **nest (topic) organization**, and **comment interactions**, with robust 
authentication, authorization, and role-based access control powered by ASP.NET Identity.

---

## Features

### User Management
- Full integration with **ASP.NET Identity** for registration and login.
- Profile management (`DisplayName`, `Icon`).
- Role-based permissions (`IsAdmin`, `IsBanned`).
- Admin tools for banning/unbanning users and promoting/demoting admins.
- Secure, minimal data exposure via DTOs.

### Comment System
- Authenticated users can create, update, and delete comments.
- Supports **soft deletion** (`IsDeleted`) for auditability.
- Author and admin-based permission enforcement.
- Clean response structure with author info and timestamps.

### Nests (Communities)
- Publicly visible topic containers for posts and comments.
- Admin/moderator management and creation.
- Moderator assignment and metadata (title, display name, icon).
- Extendable for future post grouping.

### Security
- JWT or cookie-based authentication (via Identity).
- `[Authorize]` on protected routes.
- Strict ownership checks before edits/deletes.
- Centralized error and success message constants for consistent responses.

---

## Technology Stack

| Layer | Technology |
|-------|-------------|
| **Framework** | ASP.NET Core 8.0 |
| **ORM** | Entity Framework Core |
| **Auth** | ASP.NET Identity (Guid keys) |
| **Database** | PostgreSQL (Any EF Core compatible databse. Configure in `program.cs`) |
| **Language** | C# |
| **Response Format** | JSON |

---

## Configuration

### Database Connection
In `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TopicalBird;User Id=sa;Password=YourStrongPassword;"
  }
}
