using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FrontOfficeERP.Models;
using FrontOfficeERP.Services;

namespace FrontOfficeERP.ViewModels;

public partial class DutyRosterViewModel : ObservableObject
{
    private readonly DutyService _dutyService;
    private readonly EmployeeService _employeeService;
    private readonly ExportService _exportService;
    private readonly AuthService _authService;

    [ObservableProperty]
    private ObservableCollection<DutyRoster> _rosterEntries = new();

    [ObservableProperty]
    private ObservableCollection<Employee> _employees = new();

    [ObservableProperty]
    private ObservableCollection<DutyMaster> _duties = new();

    [ObservableProperty]
    private Employee? _selectedEmployee;

    [ObservableProperty]
    private DutyMaster? _selectedDuty;

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today;

    [ObservableProperty]
    private string _remarks = string.Empty;

    [ObservableProperty]
    private DateTime _viewStartDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);

    [ObservableProperty]
    private DateTime _viewEndDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday + 6);

    [ObservableProperty]
    private string _viewMode = "Weekly";

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _reportTitle = string.Empty;

    public List<string> ViewModes { get; } = new() { "Weekly", "Monthly" };

    public DutyRosterViewModel(DutyService dutyService, EmployeeService employeeService, ExportService exportService, AuthService authService)
    {
        _dutyService = dutyService;
        _employeeService = employeeService;
        _exportService = exportService;
        _authService = authService;
        UpdateReportTitle();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsBusy = true;
        try
        {
            var employees = await _employeeService.GetActiveEmployeesAsync();
            Employees = new ObservableCollection<Employee>(employees);

            var duties = await _dutyService.GetActiveDutiesAsync();
            Duties = new ObservableCollection<DutyMaster>(duties);

            await LoadRosterAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadRosterAsync()
    {
        IsBusy = true;
        try
        {
            var rosters = await _dutyService.GetRosterAsync(ViewStartDate, ViewEndDate);
            RosterEntries = new ObservableCollection<DutyRoster>(rosters);
            UpdateReportTitle();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void SwitchToWeekly()
    {
        ViewMode = "Weekly";
        var today = DateTime.Today;
        ViewStartDate = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        ViewEndDate = ViewStartDate.AddDays(6);
        UpdateReportTitle();
    }

    [RelayCommand]
    private void SwitchToMonthly()
    {
        ViewMode = "Monthly";
        var today = DateTime.Today;
        ViewStartDate = new DateTime(today.Year, today.Month, 1);
        ViewEndDate = ViewStartDate.AddMonths(1).AddDays(-1);
        UpdateReportTitle();
    }

    [RelayCommand]
    private void PreviousPeriod()
    {
        if (ViewMode == "Weekly")
        {
            ViewStartDate = ViewStartDate.AddDays(-7);
            ViewEndDate = ViewEndDate.AddDays(-7);
        }
        else
        {
            ViewStartDate = ViewStartDate.AddMonths(-1);
            ViewEndDate = ViewStartDate.AddMonths(1).AddDays(-1);
        }
        UpdateReportTitle();
    }

    [RelayCommand]
    private void NextPeriod()
    {
        if (ViewMode == "Weekly")
        {
            ViewStartDate = ViewStartDate.AddDays(7);
            ViewEndDate = ViewEndDate.AddDays(7);
        }
        else
        {
            ViewStartDate = ViewStartDate.AddMonths(1);
            ViewEndDate = ViewStartDate.AddMonths(1).AddDays(-1);
        }
        UpdateReportTitle();
    }

    [RelayCommand]
    private void NewRosterEntry()
    {
        SelectedEmployee = null;
        SelectedDuty = null;
        SelectedDate = DateTime.Today;
        Remarks = string.Empty;
        IsEditing = true;
    }

    [RelayCommand]
    private async Task SaveRosterEntryAsync()
    {
        if (SelectedEmployee is null || SelectedDuty is null)
        {
            await Shell.Current.DisplayAlert("Validation", "Please select an employee and duty.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            var entry = new DutyRoster
            {
                EmployeeId = SelectedEmployee.Id,
                DutyId = SelectedDuty.Id,
                DutyDate = SelectedDate,
                Remarks = Remarks,
                Status = "Scheduled",
                CreatedBy = _authService.CurrentUser?.Id ?? 0
            };

            var (success, message) = await _dutyService.SaveRosterEntryAsync(entry);
            if (success)
            {
                IsEditing = false;
                await LoadRosterAsync();
            }
            await Shell.Current.DisplayAlert(success ? "Success" : "Error", message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteRosterEntryAsync(DutyRoster entry)
    {
        bool confirm = await Shell.Current.DisplayAlert("Confirm", "Delete this roster entry?", "Yes", "No");
        if (!confirm) return;

        await _dutyService.DeleteRosterEntryAsync(entry.Id);
        await LoadRosterAsync();
    }

    [RelayCommand]
    private async Task ExportToExcelAsync()
    {
        if (RosterEntries.Count == 0)
        {
            await Shell.Current.DisplayAlert("Info", "No data to export.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            var filePath = await _exportService.ExportDutyRosterToExcelAsync(
                RosterEntries.ToList(), ReportTitle, "DutyRoster");
            PrintService.OpenFile(filePath);
            await Shell.Current.DisplayAlert("Success", $"Exported to:\n{filePath}", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExportToPdfAsync()
    {
        if (RosterEntries.Count == 0)
        {
            await Shell.Current.DisplayAlert("Info", "No data to export.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            var filePath = await _exportService.ExportDutyRosterToPdfAsync(
                RosterEntries.ToList(), ReportTitle, "DutyRoster");
            PrintService.OpenFile(filePath);
            await Shell.Current.DisplayAlert("Success", $"Exported to:\n{filePath}", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task PrintRosterAsync()
    {
        if (RosterEntries.Count == 0)
        {
            await Shell.Current.DisplayAlert("Info", "No data to print.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            var filePath = await _exportService.ExportDutyRosterToPdfAsync(
                RosterEntries.ToList(), ReportTitle, "DutyRoster_Print");
            PrintService.PrintFile(filePath);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
    }

    private void UpdateReportTitle()
    {
        ReportTitle = ViewMode == "Weekly"
            ? $"Duty Roster: {ViewStartDate:dd MMM} - {ViewEndDate:dd MMM yyyy}"
            : $"Duty Roster: {ViewStartDate:MMMM yyyy}";
    }
}
