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

## Azure Blob Storage

Configure Blob settings in `appsettings.json` or environment variables:

```json
"AzureBlobStorage": {
   "ConnectionString": "<your-blob-connection-string>",
   "ContainerName": "hycoat-files",
   "CreateContainerIfNotExists": true
}
```

Environment variable names:
- `AzureBlobStorage__ConnectionString`
- `AzureBlobStorage__ContainerName`
- `AzureBlobStorage__CreateContainerIfNotExists`

## One-Time Backfill (Local Uploads -> Blob)

Preview without changing DB rows:

```bash
dotnet run -- --backfill-blob --dry-run
```

Execute migration and persist updated URLs:

```bash
dotnet run -- --backfill-blob
```
