# FrontOffice ERP - Publish, Install & Deployment Guide

**copyright @2026 Develop By Chetan**

This document provides step-by-step instructions for building, publishing, installing, and deploying the FrontOffice ERP application built with .NET MAUI (WinUI 3) targeting Windows Desktop.

---

## Table of Contents

1. [Prerequisites](#1-prerequisites)
2. [Project Setup](#2-project-setup)
3. [Building the Application](#3-building-the-application)
4. [Publishing for Windows](#4-publishing-for-windows)
5. [Creating an Installer (MSIX)](#5-creating-an-installer-msix)
6. [Sideload Installation](#6-sideload-installation)
7. [Microsoft Store Distribution](#7-microsoft-store-distribution)
8. [Unpackaged Deployment](#8-unpackaged-deployment)
9. [Configuration & Database](#9-configuration--database)
10. [Troubleshooting](#10-troubleshooting)
11. [Module Overview](#11-module-overview)

---

## 1. Prerequisites

Before building or publishing, ensure the following are installed:

| Requirement | Version | Notes |
|---|---|---|
| Visual Studio 2022/2026 | 17.x+ | Community, Professional, or Enterprise |
| .NET SDK | 9.0+ | Included with Visual Studio |
| .NET MAUI Workload | Latest | Install via Visual Studio Installer |
| Windows App SDK | Latest | Required for WinUI 3 |
| Windows 10/11 | 19041+ | Minimum target SDK version |

### Install .NET MAUI Workload

```bash
dotnet workload install maui
```

### Verify Installation

```bash
dotnet --list-sdks
dotnet workload list
```

---

## 2. Project Setup

### Clone the Repository

```bash
git clone https://github.com/cvyas2504/Chetan.git
cd Chetan
```

### Restore NuGet Packages

```bash
cd src/FrontOfficeERP
dotnet restore
```

### Key NuGet Dependencies

| Package | Purpose |
|---|---|
| CommunityToolkit.Mvvm | MVVM framework (ObservableObject, RelayCommand) |
| CommunityToolkit.Maui | MAUI UI helpers and converters |
| ClosedXML | Excel file reading/writing for Compare Tool |
| QuestPDF | PDF generation for reports |
| sqlite-net-pcl | Local SQLite database |
| SQLitePCLRaw.bundle_e_sqlcipher | SQLite encryption support |

---

## 3. Building the Application

### From Visual Studio

1. Open `FrontOfficeERP.sln` in Visual Studio 2022/2026
2. Set the startup project to `FrontOfficeERP`
3. Select target framework: `net9.0-windows10.0.19041.0`
4. Select configuration: `Debug` or `Release`
5. Select platform: `x64` (recommended) or `x86`
6. Press `F5` (Debug) or `Ctrl+F5` (Run without debug)

### From Command Line

```bash
# Debug build
dotnet build src/FrontOfficeERP/FrontOfficeERP.csproj -c Debug

# Release build
dotnet build src/FrontOfficeERP/FrontOfficeERP.csproj -c Release
```

---

## 4. Publishing for Windows

### Option A: Self-Contained Single File (Recommended for Direct Distribution)

This creates a standalone executable that includes the .NET runtime.

```bash
dotnet publish src/FrontOfficeERP/FrontOfficeERP.csproj \
  -c Release \
  -f net9.0-windows10.0.19041.0 \
  -r win10-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:WindowsPackageType=None \
  -o ./publish/standalone
```

Output: A single `.exe` file in `./publish/standalone/` that can be run directly.

### Option B: Framework-Dependent (Smaller Size, Requires .NET Runtime)

```bash
dotnet publish src/FrontOfficeERP/FrontOfficeERP.csproj \
  -c Release \
  -f net9.0-windows10.0.19041.0 \
  -r win10-x64 \
  --self-contained false \
  -p:WindowsPackageType=None \
  -o ./publish/framework-dependent
```

**Note:** Users must have .NET 9 Runtime installed on their machines.

### Option C: MSIX Package (For Store or Enterprise)

```bash
dotnet publish src/FrontOfficeERP/FrontOfficeERP.csproj \
  -c Release \
  -f net9.0-windows10.0.19041.0 \
  -r win10-x64 \
  -p:WindowsPackageType=MSIX \
  -p:PackageCertificateThumbprint=YOUR_CERT_THUMBPRINT \
  -o ./publish/msix
```

---

## 5. Creating an Installer (MSIX)

### Step 1: Generate a Self-Signed Certificate (Development)

```powershell
New-SelfSignedCertificate `
  -Type Custom `
  -Subject "CN=FrontOfficeERP, O=Chetan, C=IN" `
  -KeyUsage DigitalSignature `
  -FriendlyName "FrontOffice ERP Dev Cert" `
  -CertStoreLocation "Cert:\CurrentUser\My" `
  -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")
```

### Step 2: Export the Certificate

```powershell
$cert = Get-ChildItem -Path "Cert:\CurrentUser\My" | Where-Object { $_.Subject -like "*FrontOfficeERP*" }
Export-PfxCertificate -Cert $cert -FilePath "FrontOfficeERP.pfx" -Password (ConvertTo-SecureString -String "YourPassword" -Force -AsPlainText)
```

### Step 3: Update Package.appxmanifest

Edit `src/FrontOfficeERP/Platforms/Windows/Package.appxmanifest`:
- Set `Identity Name` to your app identifier
- Set `Publisher` to match certificate subject
- Configure display name, description, and logos

### Step 4: Build MSIX from Visual Studio

1. Right-click the project in Solution Explorer
2. Select **Publish** > **Create App Packages**
3. Choose **Sideloading** or **Microsoft Store**
4. Select certificate
5. Configure architecture (x64)
6. Click **Create**

---

## 6. Sideload Installation

For distributing the MSIX package outside the Microsoft Store:

### On the Target Machine

1. **Enable Sideloading** (if not already):
   - Go to **Settings** > **Apps** > **Apps & features**
   - Under **Choose where to get apps**, select **Anywhere**
   - Or: Settings > Update & Security > For Developers > Sideload apps

2. **Install the Certificate**:
   - Double-click the `.cer` file included with the MSIX
   - Install to **Local Machine** > **Trusted Root Certification Authorities**

3. **Install the App**:
   - Double-click the `.msix` or `.msixbundle` file
   - Click **Install** in the App Installer dialog

### PowerShell Installation

```powershell
Add-AppPackage -Path "FrontOfficeERP_1.0.0.0_x64.msix"
```

---

## 7. Microsoft Store Distribution

### Step 1: Register as Microsoft Developer

1. Go to [Microsoft Partner Center](https://partner.microsoft.com/)
2. Register for a developer account (one-time fee)
3. Create a new app submission

### Step 2: Prepare Store Assets

- App icons (various sizes: 44x44, 150x150, 300x300)
- Screenshots (1366x768 minimum)
- App description and privacy policy
- Age rating questionnaire

### Step 3: Submit Package

1. Build MSIX with Store association
2. Upload to Partner Center
3. Complete submission details
4. Submit for certification
5. Wait for review (typically 1-3 business days)

---

## 8. Unpackaged Deployment

The current project is configured for unpackaged deployment (`WindowsPackageType=None`), meaning the output is a simple `.exe` that can be distributed as-is.

### Distribution Steps

1. Build with the publish command from Section 4, Option A
2. Zip the contents of the `publish/standalone` folder
3. Distribute the ZIP file
4. Users extract and run `FrontOfficeERP.exe` directly

### Creating a Setup Installer (Optional)

Use tools like:
- **Inno Setup** (free): Create a traditional Windows installer
- **WiX Toolset** (free): MSI-based installer
- **Advanced Installer** (commercial): Visual installer builder

Example Inno Setup script outline:

```iss
[Setup]
AppName=FrontOffice ERP
AppVersion=1.0.0
DefaultDirName={autopf}\FrontOfficeERP
DefaultGroupName=FrontOffice ERP
OutputBaseFilename=FrontOfficeERP_Setup
Compression=lzma
SolidCompression=yes

[Files]
Source: "publish\standalone\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\FrontOffice ERP"; Filename: "{app}\FrontOfficeERP.exe"
Name: "{autodesktop}\FrontOffice ERP"; Filename: "{app}\FrontOfficeERP.exe"
```

---

## 9. Configuration & Database

### SQLite Database

The app uses SQLite for local data storage. The database file is created automatically on first run at:

```
%LOCALAPPDATA%\FrontOfficeERP\frontoffice.db
```

### Default Login Credentials

| Username | Password |
|---|---|
| admin | admin123 |

### Data Backup

To back up the application data:
1. Navigate to `%LOCALAPPDATA%\FrontOfficeERP\`
2. Copy the `frontoffice.db` file
3. Store in a safe location

### Export Directory

All Excel and PDF exports are saved to:
```
%USERPROFILE%\Documents\FrontOfficeERP\Exports\
```

---

## 10. Troubleshooting

### Common Issues

| Issue | Solution |
|---|---|
| App won't start | Ensure Windows 10 version 19041+ is installed |
| Missing .NET Runtime | Install .NET 9 Desktop Runtime from [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| Excel Compare errors | Ensure .xlsx files are not open in Excel while comparing |
| Database locked | Close other instances of the app |
| MSIX install fails | Check certificate is installed in Trusted Root |
| Build errors | Run `dotnet workload repair` then `dotnet restore` |

### Checking .NET Installation

```bash
dotnet --info
```

### Clearing Build Cache

```bash
dotnet clean src/FrontOfficeERP/FrontOfficeERP.csproj
dotnet nuget locals all --clear
dotnet restore src/FrontOfficeERP/FrontOfficeERP.csproj
```

### Logs

For debug builds, check the Visual Studio Output window. For release builds, exceptions are shown via in-app dialogs.

---

## 11. Module Overview

### Module 1: Advanced Excel Compare Tool

- **Location**: Excel Compare tab in the main navigation
- **Features**:
  - Load two Excel workbooks (.xlsx)
  - Column Mapping mode: Map columns between files and compare by reference key
  - Cell-by-Cell mode: Full workbook diff identifying additions, deletions, and value changes
  - High-performance parallel processing for large workbooks
  - Export comparison results to Excel
- **Key Files**:
  - `Services/ExcelCompareService.cs` - Core comparison logic with ClosedXML
  - `ViewModels/ExcelCompareViewModel.cs` - MVVM ViewModel
  - `Views/ExcelComparePage.xaml` - UI with dual comparison modes
  - `Models/ExcelCompareResult.cs` - Data models including CellDifference and ChangeType

### Module 2: Duty Management Dashboard

- **Location**: Duty Dashboard tab in the main navigation
- **Features**:
  - Glassmorphism main container (semi-transparent background with blur effect)
  - CollectionView with gradient shift cards (Green/Blue/Orange/Purple by shift type)
  - 3D tilt animation on tap and hover using ScaleTo/RotateTo
  - Responsive layout using FlexLayout for Windows Desktop and Mobile
  - Real-time shift statistics with color-coded counters
- **Key Files**:
  - `ViewModels/DutyDashboardViewModel.cs` - MVVM ViewModel with CommunityToolkit.Mvvm
  - `Views/DutyDashboardPage.xaml` - Glassmorphism + gradient card UI
  - `Views/DutyDashboardPage.xaml.cs` - 3D tilt animation code-behind
  - `Converters/ShiftTypeToGradientConverter.cs` - Shift-to-gradient mapping

### Architectural Patterns

- **MVVM**: All pages use CommunityToolkit.Mvvm with ObservableObject and RelayCommand
- **Dependency Injection**: Services registered in MauiProgram.cs
- **Responsive Design**: FlexLayout with Wrap for adaptive layouts
- **Branding**: Mandatory footer on every page: "copyright @2026 Develop By Chetan"

---

*This guide was written for FrontOffice ERP v1.0. For updates, check the repository at https://github.com/cvyas2504/Chetan*
