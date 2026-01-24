# Outlook Timesheet Solution

This repository contains a full-stack solution for managing timesheets via Outlook. It consists of a VSTO Outlook Add-in (Client) and an ASP.NET Core Web API (Server), sharing data models via a .NET Standard library.

## Project Structure

*   **Client/**: The VSTO Outlook Add-in (.NET Framework 4.8).
*   **Server/**: The Backend Web API (ASP.NET Core 8.0, Entity Framework Core, SQLite).
*   **Timesheets.Shared/**: Shared data models library (.NET Standard 2.0).

## Features

- **Outlook Integration**: Manage time entries directly within the Outlook Calendar (Shadow Copy workflow).
- **Backend API**: RESTful API to store Jobs and Time Entries using SQLite.
- **Shared Models**: `TimeEntryModel` is shared between Client and Server to ensure consistency.
- **Offline/Sync**: The client synchronizes changes with the backend (via `ApiService`).

## Prerequisites

- **Visual Studio 2022** (required for .NET 8 and VSTO development).
- **Workloads**:
    - ".NET desktop development"
    - "Office/SharePoint development"
    - "ASP.NET and web development"
- **Microsoft Outlook** (Desktop version).
- **.NET 8 SDK**.

## Getting Started

### 1. Setup
1.  Clone the repository.
2.  Open `Timesheets.sln` in Visual Studio.
3.  Restore NuGet packages for the solution.

### 2. Run the Server
1.  Set `Timesheets.API` as the Startup Project (or configure Multiple Startup Projects).
2.  Run the project.
3.  The API will start (default `http://localhost:5000` or `https://localhost:7001`).
4.  On first run, it will create `timesheets.db` (SQLite) and seed it with dummy Jobs.
    - Check Swagger UI at `/swagger/index.html`.

### 3. Run the Client
1.  Set `timesheets` (in the `Client` folder) as the Startup Project.
2.  Ensure the API is running (or running in the background).
3.  Run (F5). Outlook will launch with the add-in.

## Usage

1.  **Ribbon**: Go to the "Timesheets" tab in Outlook.
2.  **Jobs**: The dropdown loads jobs from the API (`GET /api/jobs`).
3.  **Assign**: Select calendar items -> "Assign Selected". Copies them to the "Timesheets" sub-calendar and POSTs to `/api/timesheets`.
4.  **Edit**: Double-click an item in "Timesheets" folder to open the custom form. Saves update the API.

## Notes

- The database is a local SQLite file (`timesheets.db`) created in the API execution directory.
- The VSTO Client is configured to talk to `localhost`. Ensure CORS is enabled on the server (already configured in `Program.cs`).

