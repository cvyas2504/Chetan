using FrontOfficeERP.Data;
using FrontOfficeERP.Models;

namespace FrontOfficeERP.Services;

public class EmployeeService
{
    private readonly DatabaseService _dbService;

    public EmployeeService(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    public async Task<List<Employee>> GetAllEmployeesAsync()
    {
        var db = await _dbService.GetDatabaseAsync();
        return await db.Table<Employee>().OrderBy(e => e.FirstName).ToListAsync();
    }

    public async Task<List<Employee>> GetActiveEmployeesAsync()
    {
        var db = await _dbService.GetDatabaseAsync();
        return await db.Table<Employee>().Where(e => e.IsActive).OrderBy(e => e.FirstName).ToListAsync();
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
        var db = await _dbService.GetDatabaseAsync();
        return await db.Table<Employee>().Where(e => e.Id == id).FirstOrDefaultAsync();
    }

    public async Task<(bool Success, string Message)> SaveEmployeeAsync(Employee employee)
    {
        var db = await _dbService.GetDatabaseAsync();

        if (employee.Id == 0)
        {
            var existing = await db.Table<Employee>()
                .Where(e => e.EmployeeCode == employee.EmployeeCode)
                .FirstOrDefaultAsync();
            if (existing is not null)
                return (false, "Employee code already exists.");

            employee.CreatedAt = DateTime.UtcNow;
            employee.UpdatedAt = DateTime.UtcNow;
            await db.InsertAsync(employee);
            return (true, "Employee created successfully.");
        }

        employee.UpdatedAt = DateTime.UtcNow;
        await db.UpdateAsync(employee);
        return (true, "Employee updated successfully.");
    }

    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        var db = await _dbService.GetDatabaseAsync();
        var employee = await db.Table<Employee>().Where(e => e.Id == id).FirstOrDefaultAsync();
        if (employee is null) return false;

        employee.IsActive = false;
        employee.UpdatedAt = DateTime.UtcNow;
        await db.UpdateAsync(employee);
        return true;
    }
}
