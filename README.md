# Outlook Timesheet Solution

This repository contains a full-stack solution for managing timesheets via Outlook. It consists of a VSTO Outlook Add-in (Client) and an ASP.NET Core Web API (Server), sharing data models via a .NET Standard library.

## Project Structure

*   **Client/**: The VSTO Outlook Add-in (.NET Framework 4.8).
*   **Server/**: The Backend Web API (ASP.NET Core 8.0, Entity Framework Core, SQLite).
*   **Timesheets.Shared/**: Shared data models library (.NET Standard 2.0).

## Features

- **Outlook Integration**: Manage time entries directly within the Outlook Calendar (Shadow Copy workflow).
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
1.  Navigate to `/Server`.
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

## Architecture & Notes

- **Database**: Local SQLite file (`timesheets.db`).
- **Resilience**: The Client uses an offline queue with a "Rename & Lock" strategy to ensure data integrity during sync.
- **Performance**: The Server uses efficient batch processing (bulk ID lookup) to minimize database round-trips.
- **Security**: The Server requires an `X-Api-Key` header for all requests.
