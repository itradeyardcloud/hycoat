# dotnet-api

ASP.NET Core 10 Web API with Entity Framework Core + SQL Server.

## Requirements
- .NET 10 SDK
- SQL Server (local or remote)

## Setup

1. Update connection string in `appsettings.json`
2. Run migrations:
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```
3. Run the API:
   ```bash
   dotnet run
   ```

Swagger UI: https://localhost:7xxx/swagger
