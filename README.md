# Outlook Timesheet Solution

This repository contains a full-stack solution for managing timesheets via Outlook. It consists of a VSTO Outlook Add-in (Client) and an ASP.NET Core Web API (Server), sharing data models via a .NET Standard library.

## Project Structure

*   **Client/**: The VSTO Outlook Add-in (.NET Framework 4.8).
*   **Server/**: The Backend Web API (ASP.NET Core 8.0, Entity Framework Core, SQLite).
*   **Timesheets.Shared/**: Shared data models library (.NET Standard 2.0).
*   **Timesheets.Tests/**: Unit tests for the shared models, server middleware, and offline queue.

## Features

- **Outlook Integration**: Manage time entries directly within the Outlook Calendar. Uses a 'Shadow Copy' workflow where appointments are copied to and managed in a dedicated 'Timesheets' calendar sub-folder, leaving the main calendar unmodified.
- **Backend API**: RESTful API to store Jobs and Time Entries using SQLite.
- **Shared Models**: `TimeEntryModel` and `Job` are shared between Client and Server to ensure consistency.
- **Offline/Sync**: The client synchronizes changes with the backend using an Offline Queue.
- **Security**: Basic API Key authentication.

## Prerequisites

- **Visual Studio 2022** (required for .NET 8 and VSTO development).
- **Workloads**:
    - ".NET desktop development"
    - "Office/SharePoint development"
    - "ASP.NET and web development"
- **Microsoft Outlook** (Desktop version).
- **.NET 8 SDK**.
- **EF Core tools**: Run `dotnet tool install --global dotnet-ef` to install.

## Getting Started

### 1. Setup
1.  Clone the repository.
2.  Open `Timesheets.sln` in Visual Studio.
3.  Restore NuGet packages for the solution.

### 2. Database Setup
1.  Navigate to `/Server/Timesheets.API`.
2.  Run `dotnet ef migrations add InitialCreate`.
3.  Run `dotnet ef database update`.

### 3. Run the Server
1.  Navigate to `/Server/Timesheets.API` (or set as Startup Project in VS).
2.  Run `dotnet run` or press F5 in Visual Studio.
3.  The API will start (check output for ports, e.g., `https://localhost:7053`).
4.  On first run, it will create `timesheets.db` (SQLite) and seed it with dummy Jobs.
    - Check Swagger UI at `/swagger/index.html`.

### 4. Run the Client
1.  Set `timesheets` (in the `Client` folder) as the Startup Project.
2.  Ensure the API is running (or running in the background).
3.  Run (F5). Outlook will launch with the add-in.
4.  **Configuration**: The Client uses user-scoped settings for the API URL and Key.
    - `ApiBaseUrl`: Defaults to `https://localhost:7053/`.
    - `ApiKey`: Defaults to `YOUR_SECURE_KEY`. Update this in `Properties.Settings` or the app's configuration if you change the server key.

### Ngrok Setup (Optional for Remote/HTTPS testing)
1.  Install Ngrok.
2.  Run `ngrok http [PORT]`.
3.  Copy the forwarding URL.
4.  Update the Client's `ApiBaseUrl` setting.

## Testing

The repository contains a unit test project, `Timesheets.Tests`, which covers:
- Shared models (`TimeEntryModel`) validation logic.
- Server middleware (`ApiKeyMiddleware`) security checks.
- Offline queue concurrency and processing logic.

To run the tests:
1. Navigate to the root directory or the `Timesheets.Tests` directory.
2. Run `dotnet test`.

*Note: The VSTO Client project (`Client/timesheets.csproj`) relies on .NET Framework 4.8 and Windows-specific libraries. Therefore, it cannot be built or unit-tested in Linux environments.*

## Architecture & Notes

- **Database**: Local SQLite file (`timesheets.db`).
- **Resilience**: The Client uses an offline queue with a "Rename & Lock" strategy. Offline data is persisted to `timesheets_offline_queue.json` in the user's `AppData` folder. A temporary `.processing` file is used during sync, with automatic crash recovery to append stale processing content back to the main queue to ensure data integrity.
- **Performance**: The Server uses efficient batch processing (bulk ID lookup) to minimize database round-trips.
- **Security**: The Server requires an `X-Api-Key` header for all requests.
