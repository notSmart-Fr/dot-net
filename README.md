# TaskApi

TaskApi is a minimal ASP.NET Core task management API backed by SQLite and Entity Framework Core.

## Why SQLite?

SQLite was chosen because it is lightweight, embedded, and requires no separate database server for local development. The database is stored as a single file, which keeps the project easy to run and inspect while still providing relational data storage through Entity Framework Core.

## Database location

The connection string in `appsettings.json` is:

```text
Data Source=tasks.db
```

Because the path is relative, the database file is created at:

```text
TaskApi/tasks.db
```

The application creates the database and its schema automatically on startup with `EnsureCreatedAsync()`. The initial task data is seeded by `AppDbContext`.

## How to Start the Project

### Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/download) or later installed.

### Steps to Run

1. Navigate to the project root directory:

   ```bash
   cd TaskApi
   ```

2. Start the API:

   ```bash
   dotnet run
   ```

   The database file will automatically be created and seeded with 3 example tasks on the first run.

3. Open Swagger UI in your browser.

   Check the terminal console output for the local URL, typically `http://localhost:5131/swagger`.

## Database viewer screenshot

Add a screenshot of the SQLite database viewer below. Replace the placeholder path with the image file you add to the repository.

<!-- Replace the path below with the database viewer screenshot. -->
![SQLite database viewer screenshot](docs/database-viewer-screenshot.png)

## Swagger UI screenshot

Add a screenshot of the Swagger UI below. Replace the placeholder path with the image file you add to the repository.

<!-- Replace the path below with the Swagger UI screenshot. -->
![Swagger UI screenshot](docs/swagger-ui-screenshot.png)

## Example Entity Framework Core query

The following SQL query was executed against `tasks.db` to list incomplete tasks, newest first:

```sql
SELECT "Id", "Title", "Done"
FROM "tasks"
WHERE "Done" = 0
ORDER BY "Id" DESC;
```

The equivalent Entity Framework Core query used by the API is:

```csharp
var tasks = await db.Tasks
    .AsNoTracking()
    .Where(task => !task.Done)
    .OrderByDescending(task => task.Id)
    .ToListAsync(cancellationToken);
```

## Run with Docker

### Prerequisites

* [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running.

### Steps to Run

1. Create the Docker environment file from the provided template:

   ```bash
   copy .env.example .env
   ```

   On macOS or Linux, use `cp .env.example .env` instead. Update the PostgreSQL password values in `.env` before starting the containers.

2. Build the image and start the API and database containers:

   ```bash
   docker compose up --build
   ```

3. Open Swagger UI at <http://localhost:5131/swagger>.

4. Stop the containers with `Ctrl+C`, or run the following command from another terminal:

   ```bash
   docker compose down
   ```

To view the application logs while the containers are running, use:

```bash
docker compose logs -f app
```
