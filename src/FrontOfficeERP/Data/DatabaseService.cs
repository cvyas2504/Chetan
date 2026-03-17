using SQLite;
using FrontOfficeERP.Models;

namespace FrontOfficeERP.Data;

public class DatabaseService
{
    private SQLiteAsyncConnection? _database;
    private readonly string _dbPath;

    public DatabaseService()
    {
        _dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "FrontOfficeERP",
            "frontoffice.db");
    }

    public async Task<SQLiteAsyncConnection> GetDatabaseAsync()
    {
        if (_database is not null)
            return _database;

        var directory = Path.GetDirectoryName(_dbPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        _database = new SQLiteAsyncConnection(_dbPath);

        await _database.CreateTableAsync<User>();
        await _database.CreateTableAsync<LoginMaster>();
        await _database.CreateTableAsync<Employee>();
        await _database.CreateTableAsync<DutyMaster>();
        await _database.CreateTableAsync<DutyRoster>();

        await SeedDefaultDataAsync(_database);

        return _database;
    }

    private async Task SeedDefaultDataAsync(SQLiteAsyncConnection db)
    {
        var userCount = await db.Table<User>().CountAsync();
        if (userCount == 0)
        {
            var admin = new User
            {
                Username = "admin",
                PasswordHash = HashPassword("admin123"),
                FullName = "System Administrator",
                Email = "admin@frontoffice.local",
                Role = "Admin",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await db.InsertAsync(admin);
        }

        var dutyCount = await db.Table<DutyMaster>().CountAsync();
        if (dutyCount == 0)
        {
            var duties = new List<DutyMaster>
            {
                new() { DutyCode = "MRN", DutyName = "Morning Shift", StartTime = "06:00", EndTime = "14:00", ShiftType = "Morning" },
                new() { DutyCode = "AFT", DutyName = "Afternoon Shift", StartTime = "14:00", EndTime = "22:00", ShiftType = "Afternoon" },
                new() { DutyCode = "NGT", DutyName = "Night Shift", StartTime = "22:00", EndTime = "06:00", ShiftType = "Night" },
                new() { DutyCode = "GEN", DutyName = "General Duty", StartTime = "09:00", EndTime = "17:00", ShiftType = "General" },
                new() { DutyCode = "OFF", DutyName = "Day Off", StartTime = "--", EndTime = "--", ShiftType = "Off" },
            };
            await db.InsertAllAsync(duties);
        }
    }

    public static string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
