# Outlook Timesheet Add-in

A VSTO (Visual Studio Tools for Office) Outlook Add-in that enables users to manage time entries directly within their Outlook Calendar.

## Overview

This add-in implements a "Shadow Copy" workflow where appointments can be copied to a dedicated "Timesheets" calendar folder. These items are then tracked with additional metadata (Job ID, Task ID, Regular Time, Overtime, etc.) and can be submitted to a backend API (mocked in this version).

## Features

- **Ribbon Interface**: A custom "Timesheets" tab in the Outlook Ribbon.
- **Job & Task Selection**: Dropdowns to select active Jobs and Tasks.
- **Assign & Track**: Convert selected calendar items into timesheet entries.
- **Custom Form**: A custom WinForms dialog replaces the standard Outlook Inspector for tracked items, allowing detailed time entry editing.
- **Smart Resize**: Dragging and resizing items in the "Timesheets" calendar automatically recalculates hours.
- **Sync**: Changes are synchronized with a backend API.

## Architecture

The solution is structured as follows:

- **Forms/**: Custom WinForms dialogs (`TimeEntryForm`) for editing time entries.
- **Models/**: Data models (`TimeEntryModel`) representing the timesheet data.
- **Services/**: `ApiService` handles communication with the backend (currently using mock data).
- **UI/**: Ribbon XML and code-behind (`TimesheetRibbon`) for the Outlook UI integration.
- **ThisAddIn.cs**: The main entry point, handling lifecycle events, Inspector interception, and Item changes.

## Prerequisites

- **Visual Studio 2019 or later** with the ".NET desktop development" and "Office/SharePoint development" workloads installed.
- **Microsoft Outlook** (Desktop version).
- **.NET Framework 4.8**.

## Getting Started

1.  **Clone the repository**.
2.  **Open the solution** (`timesheets.sln`) in Visual Studio.
    *   *Note: If the solution file is missing, you can open the project file `timesheets/timesheets.csproj` directly and save a new solution.*
3.  **Restore NuGet packages** (if any are missing).
4.  **Build the project**.
5.  **Run (F5)**: Visual Studio will launch Outlook with the add-in loaded.

## Usage Guide

### 1. The Timesheets Tab
Locate the "Timesheets" tab in the Outlook Ribbon.
- Use the **Active Job** and **Active Task** dropdowns to select the project you are working on.

### 2. Creating Time Entries
- Select one or more appointments in your main Calendar.
- Click **Assign Selected** in the Timesheets Ribbon.
- The items will be copied to the **Timesheets** calendar folder.
- They will be tagged with the selected Job and Task.

### 3. Editing Entries
- Double-click an item in the **Timesheets** folder.
- Instead of the standard Outlook window, a **Time Entry Form** will appear.
- You can adjust:
    - Regular Time (RT)
    - Overtime (OT)
    - Double Time (DT)
    - Travel Time
    - Notes
- Click **Save** to update the item and sync to the API.

### 4. Smart Resizing
- You can drag or resize items directly in the **Timesheets** calendar view.
- The "Regular Time" (RT) will automatically update based on the new duration (minus any OT/DT/Travel logged).

### 5. Submitting
- Click **Submit Week** (mock functionality) to lock entries for the current week.

## Troubleshooting

- **Add-in not loading?** Check `File > Options > Add-ins` in Outlook. If it's disabled, re-enable it.
- **Developer Mode**: Ensure you are running in Debug configuration to see detailed errors if any occur.

## Contributing

1.  Fork the repository.
2.  Create a feature branch.
3.  Commit your changes.
4.  Push to the branch.
5.  Create a Pull Request.

## License

[License Name Here]
