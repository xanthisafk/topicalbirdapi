![Logo of TopicalBirdAPI](/TopicalBirdAPI/wwwroot/content/assets/defaults/api_logo.svg "From SVGREPO.org")

# TopicalBird API

A robust, feature-rich ASP.NET Core Web API that powers a modern social media/forum platform.

## Key Features

This API provides all the necessary backend services for a dynamic content and community application, featuring:

  - Full Authentication" User registration, secure login, and password management using ASP.NET Core Identity.
  - User & Admin Management: Endpoints for user profiles, and administrative control to manage and promote users.
  - Posts & Media: Create, read, and manage posts, supporting multi-part form data for file and media uploads.
  - Comment System: A nested system for creating and deleting comments on posts.
  - Dynamic Voting: Real-time upvoting and downvoting mechanism (`VoteController`) with post score calculation.
  - Nest Communities: Dedicated controllers for creating and managing topic-specific communities or sub-forums (`NestController`).
  - Authorization & Logging: Role-based access control and detailed structured logging for all critical operations.

-----

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes.

### Prerequisites

  * .NET SDK 8.0
  * PostgreSQL, or other database supported by Entity Framework Core.

### Installation

1.  Clone the repository"
    ```bash
    git clone https://github.com/YourUsername/TopicalBirdAPI.git
    cd TopicalBirdAPI
    ```
	
2.  Update Database Connection""
    Configure your connection string in `appsettings.json`.
	Configure EF core in `program.cs` if you are using anything other than PostgreSQL.
	
3.  Apply Migrations and Run"
    ```bash
    dotnet ef database update
    dotnet run
    ```

The API should now be running on `http://localhost:7154` (or the configured port).

-----

## Tech Stack

| Technology | Purpose |
| :--- | :--- |
| **C\# / .NET** | Primary language and framework |
| **ASP.NET Core** | Web API structure and hosting |
| **Entity Framework Core** | ORM for database interaction |
| **ASP.NET Core Identity** | Robust authentication and user management |

## License

See `LICENSE.md` for more information.
