# FrontOffice ERP - Duty Roster Management System

A .NET MAUI desktop application for managing employee duty rosters, with Excel comparison capabilities. Built for Windows desktop deployment.

## Features

### User Management
- **User Login** with secure password hashing (SHA-256)
- **User Master** for managing user accounts (Admin, Manager, User roles)
- **Login Master** audit trail tracking all login/logout activity

### Dashboard
- Overview of key metrics (employees, duties, today's roster, weekly count)
- Quick navigation to all modules

### Duty Roster Module
- **Duty Management** with weekly and monthly views
- **Period Navigation** to browse through weeks/months
- **Duty Master** for defining duty types, shift patterns, and timings
- **Employee Master** for managing employee records
- **Weekly Reports** and **Monthly Reports** views
- **Export to Excel** (.xlsx) with formatted headers and data
- **Export to PDF** with professional layout
- **Print** option that sends directly to default printer

### Excel Compare Module
- Load master and compare Excel files
- **Reference Popup Column** mapping for selecting which headers to match
- Define column mappings between files
- Set reference (key) column for row matching
- Run comparison with Match / Mismatch / Missing detection
- View detailed comparison results with row-level status
- **Merge master file** with all rows and columns from both files
- Export comparison results and merged data to Excel

### Database
- **SQLite** database for local desktop storage
- Auto-creates tables on first run
- Seeds default admin user and duty types

## Project Structure

```
src/FrontOfficeERP/
  Models/           - Data models (User, Employee, DutyMaster, DutyRoster, etc.)
  Data/             - DatabaseService with SQLite integration
  Services/         - Business logic (Auth, Duty, Employee, Export, ExcelCompare, Print)
  ViewModels/       - MVVM ViewModels with CommunityToolkit.Mvvm
  Views/            - XAML pages (Login, Dashboard, DutyRoster, ExcelCompare, etc.)
  Converters/       - Value converters for XAML bindings
  Platforms/Windows/ - Windows-specific platform code
```

## Tech Stack

- **.NET 9** with **.NET MAUI** (Multi-platform App UI)
- **SQLite** via sqlite-net-pcl for local data storage
- **CommunityToolkit.Mvvm** for MVVM pattern (ObservableObject, RelayCommand)
- **CommunityToolkit.Maui** for UI enhancements
- **ClosedXML** for Excel file reading/writing
- **QuestPDF** for PDF generation
- **Windows Desktop** target (unpackaged exe)

## Prerequisites

- Visual Studio 2022/2026 with .NET MAUI workload
- .NET 9 SDK
- Windows 10 (build 19041) or later

## Build and Run

```bash
# Restore packages
dotnet restore FrontOfficeERP.sln

# Build
dotnet build src/FrontOfficeERP/FrontOfficeERP.csproj -f net9.0-windows10.0.19041.0

# Publish as self-contained exe
dotnet publish src/FrontOfficeERP/FrontOfficeERP.csproj -f net9.0-windows10.0.19041.0 -c Release -r win10-x64 --self-contained
```

The published output in `bin/Release/net9.0-windows10.0.19041.0/win10-x64/publish/` contains the standalone `.exe` installer.

## Default Login

- **Username:** admin
- **Password:** admin123

## Database Location

The SQLite database is stored at:
```
%LOCALAPPDATA%/FrontOfficeERP/frontoffice.db
```

## Export Location

Exported files (PDF, Excel) are saved to:
```
%USERPROFILE%/Documents/FrontOfficeERP/Exports/
```
