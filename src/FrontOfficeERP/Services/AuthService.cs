using FrontOfficeERP.Data;
using FrontOfficeERP.Models;

namespace FrontOfficeERP.Services;

public class AuthService
{
    private readonly DatabaseService _dbService;
    private User? _currentUser;

    public User? CurrentUser => _currentUser;
    public bool IsLoggedIn => _currentUser is not null;

    public AuthService(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    public async Task<(bool Success, string Message)> LoginAsync(string username, string password)
    {
        var db = await _dbService.GetDatabaseAsync();
        var passwordHash = DatabaseService.HashPassword(password);

        var user = await db.Table<User>()
            .Where(u => u.Username == username && u.PasswordHash == passwordHash && u.IsActive)
            .FirstOrDefaultAsync();

        var loginRecord = new LoginMaster
        {
            UserId = user?.Id ?? 0,
            LoginTime = DateTime.UtcNow,
            MachineName = Environment.MachineName,
            IsSuccess = user is not null,
            Remarks = user is null ? "Invalid credentials" : "Login successful"
        };

        await db.InsertAsync(loginRecord);

        if (user is null)
            return (false, "Invalid username or password.");

        user.LastLoginAt = DateTime.UtcNow;
        await db.UpdateAsync(user);

        _currentUser = user;
        return (true, "Login successful.");
    }

    public async Task LogoutAsync()
    {
        if (_currentUser is not null)
        {
            var db = await _dbService.GetDatabaseAsync();
            var lastLogin = await db.Table<LoginMaster>()
                .Where(l => l.UserId == _currentUser.Id && l.LogoutTime == null)
                .OrderByDescending(l => l.LoginTime)
                .FirstOrDefaultAsync();

            if (lastLogin is not null)
            {
                lastLogin.LogoutTime = DateTime.UtcNow;
                await db.UpdateAsync(lastLogin);
            }
        }
        _currentUser = null;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        var db = await _dbService.GetDatabaseAsync();
        return await db.Table<User>().ToListAsync();
    }

    public async Task<(bool Success, string Message)> CreateUserAsync(User user)
    {
        var db = await _dbService.GetDatabaseAsync();
        var existing = await db.Table<User>().Where(u => u.Username == user.Username).FirstOrDefaultAsync();
        if (existing is not null)
            return (false, "Username already exists.");

        user.PasswordHash = DatabaseService.HashPassword(user.PasswordHash);
        user.CreatedAt = DateTime.UtcNow;
        await db.InsertAsync(user);
        return (true, "User created successfully.");
    }

    public async Task<(bool Success, string Message)> UpdateUserAsync(User user)
    {
        var db = await _dbService.GetDatabaseAsync();
        user.LastLoginAt = user.LastLoginAt;
        await db.UpdateAsync(user);
        return (true, "User updated successfully.");
    }

    public async Task<List<LoginMaster>> GetLoginHistoryAsync(int? userId = null)
    {
        var db = await _dbService.GetDatabaseAsync();
        if (userId.HasValue)
            return await db.Table<LoginMaster>().Where(l => l.UserId == userId.Value).OrderByDescending(l => l.LoginTime).ToListAsync();
        return await db.Table<LoginMaster>().OrderByDescending(l => l.LoginTime).Take(100).ToListAsync();
    }
}
