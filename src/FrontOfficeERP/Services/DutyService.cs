using FrontOfficeERP.Data;
using FrontOfficeERP.Models;

namespace FrontOfficeERP.Services;

public class DutyService
{
    private readonly DatabaseService _dbService;

    public DutyService(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    // Duty Master operations
    public async Task<List<DutyMaster>> GetAllDutiesAsync()
    {
        var db = await _dbService.GetDatabaseAsync();
        return await db.Table<DutyMaster>().OrderBy(d => d.DutyCode).ToListAsync();
    }

    public async Task<List<DutyMaster>> GetActiveDutiesAsync()
    {
        var db = await _dbService.GetDatabaseAsync();
        return await db.Table<DutyMaster>().Where(d => d.IsActive).OrderBy(d => d.DutyCode).ToListAsync();
    }

    public async Task<(bool Success, string Message)> SaveDutyAsync(DutyMaster duty)
    {
        var db = await _dbService.GetDatabaseAsync();

        if (duty.Id == 0)
        {
            var existing = await db.Table<DutyMaster>()
                .Where(d => d.DutyCode == duty.DutyCode)
                .FirstOrDefaultAsync();
            if (existing is not null)
                return (false, "Duty code already exists.");

            duty.CreatedAt = DateTime.UtcNow;
            await db.InsertAsync(duty);
            return (true, "Duty created successfully.");
        }

        await db.UpdateAsync(duty);
        return (true, "Duty updated successfully.");
    }

    // Duty Roster operations
    public async Task<List<DutyRoster>> GetRosterAsync(DateTime startDate, DateTime endDate)
    {
        var db = await _dbService.GetDatabaseAsync();
        var rosters = await db.Table<DutyRoster>()
            .Where(r => r.DutyDate >= startDate && r.DutyDate <= endDate)
            .OrderBy(r => r.DutyDate)
            .ToListAsync();

        var employees = await db.Table<Employee>().ToListAsync();
        var duties = await db.Table<DutyMaster>().ToListAsync();

        foreach (var roster in rosters)
        {
            var emp = employees.FirstOrDefault(e => e.Id == roster.EmployeeId);
            var duty = duties.FirstOrDefault(d => d.Id == roster.DutyId);
            roster.EmployeeName = emp?.FullName ?? "Unknown";
            roster.DutyName = duty?.DutyName ?? "Unknown";
            roster.DutyCode = duty?.DutyCode ?? "";
            roster.ShiftType = duty?.ShiftType ?? "";
        }

        return rosters;
    }

    public async Task<List<DutyRoster>> GetWeeklyRosterAsync(DateTime weekStart)
    {
        var weekEnd = weekStart.AddDays(6);
        return await GetRosterAsync(weekStart, weekEnd);
    }

    public async Task<List<DutyRoster>> GetMonthlyRosterAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        return await GetRosterAsync(startDate, endDate);
    }

    public async Task<(bool Success, string Message)> SaveRosterEntryAsync(DutyRoster roster)
    {
        var db = await _dbService.GetDatabaseAsync();

        // Check for duplicate
        var existing = await db.Table<DutyRoster>()
            .Where(r => r.EmployeeId == roster.EmployeeId && r.DutyDate == roster.DutyDate)
            .FirstOrDefaultAsync();

        if (existing is not null && roster.Id == 0)
        {
            existing.DutyId = roster.DutyId;
            existing.Status = roster.Status;
            existing.Remarks = roster.Remarks;
            existing.UpdatedAt = DateTime.UtcNow;
            await db.UpdateAsync(existing);
            return (true, "Roster entry updated.");
        }

        if (roster.Id == 0)
        {
            roster.CreatedAt = DateTime.UtcNow;
            roster.UpdatedAt = DateTime.UtcNow;
            await db.InsertAsync(roster);
            return (true, "Roster entry created.");
        }

        roster.UpdatedAt = DateTime.UtcNow;
        await db.UpdateAsync(roster);
        return (true, "Roster entry updated.");
    }

    public async Task<(bool Success, string Message)> BulkSaveRosterAsync(List<DutyRoster> entries)
    {
        var db = await _dbService.GetDatabaseAsync();
        int saved = 0;

        foreach (var entry in entries)
        {
            var existing = await db.Table<DutyRoster>()
                .Where(r => r.EmployeeId == entry.EmployeeId && r.DutyDate == entry.DutyDate)
                .FirstOrDefaultAsync();

            if (existing is not null)
            {
                existing.DutyId = entry.DutyId;
                existing.Status = entry.Status;
                existing.Remarks = entry.Remarks;
                existing.UpdatedAt = DateTime.UtcNow;
                await db.UpdateAsync(existing);
            }
            else
            {
                entry.CreatedAt = DateTime.UtcNow;
                entry.UpdatedAt = DateTime.UtcNow;
                await db.InsertAsync(entry);
            }
            saved++;
        }

        return (true, $"{saved} roster entries saved.");
    }

    public async Task<bool> DeleteRosterEntryAsync(int id)
    {
        var db = await _dbService.GetDatabaseAsync();
        return await db.DeleteAsync<DutyRoster>(id) > 0;
    }
}
