using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FrontOfficeERP.Services;

namespace FrontOfficeERP.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly DutyService _dutyService;
    private readonly EmployeeService _employeeService;

    [ObservableProperty]
    private string _welcomeMessage = string.Empty;

    [ObservableProperty]
    private int _totalEmployees;

    [ObservableProperty]
    private int _totalDuties;

    [ObservableProperty]
    private int _todayRosterCount;

    [ObservableProperty]
    private int _weekRosterCount;

    [ObservableProperty]
    private bool _isBusy;

    public DashboardViewModel(AuthService authService, DutyService dutyService, EmployeeService employeeService)
    {
        _authService = authService;
        _dutyService = dutyService;
        _employeeService = employeeService;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsBusy = true;
        try
        {
            WelcomeMessage = $"Welcome, {_authService.CurrentUser?.FullName ?? "User"}!";

            var employees = await _employeeService.GetActiveEmployeesAsync();
            TotalEmployees = employees.Count;

            var duties = await _dutyService.GetActiveDutiesAsync();
            TotalDuties = duties.Count;

            var today = DateTime.Today;
            var todayRoster = await _dutyService.GetRosterAsync(today, today);
            TodayRosterCount = todayRoster.Count;

            var weekStart = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            var weekRoster = await _dutyService.GetWeeklyRosterAsync(weekStart);
            WeekRosterCount = weekRoster.Count;
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
    private async Task NavigateToAsync(string route)
    {
        await Shell.Current.GoToAsync(route);
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _authService.LogoutAsync();
        await Shell.Current.GoToAsync("//login");
    }
}
