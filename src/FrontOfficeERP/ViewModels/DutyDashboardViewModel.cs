using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FrontOfficeERP.Models;
using FrontOfficeERP.Services;

namespace FrontOfficeERP.ViewModels;

/// <summary>
/// ViewModel for the Duty Management Dashboard with modern 3D/Fluent-style cards.
/// Follows MVVM pattern using CommunityToolkit.Mvvm.
/// </summary>
public partial class DutyDashboardViewModel : ObservableObject
{
    private readonly DutyService _dutyService;
    private readonly EmployeeService _employeeService;

    [ObservableProperty]
    private ObservableCollection<DutyShiftCard> _dutyShiftCards = new();

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private int _totalGeneral;

    [ObservableProperty]
    private int _totalMorning;

    [ObservableProperty]
    private int _totalEvening;

    [ObservableProperty]
    private int _totalNight;

    [ObservableProperty]
    private int _totalScheduled;

    [ObservableProperty]
    private string _currentPeriod = string.Empty;

    public DutyDashboardViewModel(DutyService dutyService, EmployeeService employeeService)
    {
        _dutyService = dutyService;
        _employeeService = employeeService;
        CurrentPeriod = $"Week of {DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday):dd MMM yyyy}";
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsBusy = true;
        try
        {
            var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            var weekEnd = weekStart.AddDays(6);
            var rosters = await _dutyService.GetRosterAsync(weekStart, weekEnd);

            var cards = new ObservableCollection<DutyShiftCard>();
            foreach (var roster in rosters.OrderBy(r => r.DutyDate).ThenBy(r => r.ShiftType))
            {
                cards.Add(new DutyShiftCard
                {
                    Id = roster.Id,
                    EmployeeName = roster.EmployeeName,
                    DutyName = roster.DutyName,
                    DutyCode = roster.DutyCode,
                    ShiftType = roster.ShiftType,
                    DutyDate = roster.DutyDate,
                    Status = roster.Status,
                    Remarks = roster.Remarks
                });
            }

            DutyShiftCards = cards;

            // Update counters
            TotalGeneral = rosters.Count(r => r.ShiftType.Equals("General", StringComparison.OrdinalIgnoreCase));
            TotalMorning = rosters.Count(r => r.ShiftType.Equals("Morning", StringComparison.OrdinalIgnoreCase));
            TotalEvening = rosters.Count(r => r.ShiftType.Equals("Evening", StringComparison.OrdinalIgnoreCase));
            TotalNight = rosters.Count(r => r.ShiftType.Equals("Night", StringComparison.OrdinalIgnoreCase));
            TotalScheduled = rosters.Count;

            CurrentPeriod = $"Week of {weekStart:dd MMM yyyy}";
            StatusMessage = $"{TotalScheduled} shifts loaded for current week";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading data: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDataAsync();
    }
}

/// <summary>
/// Presentation model for a duty shift card in the dashboard.
/// </summary>
public class DutyShiftCard
{
    public int Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string DutyName { get; set; } = string.Empty;
    public string DutyCode { get; set; } = string.Empty;
    public string ShiftType { get; set; } = "General";
    public DateTime DutyDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;

    public string FormattedDate => DutyDate.ToString("ddd, dd MMM");
    public string ShiftIcon => ShiftType.ToLowerInvariant() switch
    {
        "morning" => "\u2600",   // Sun
        "evening" => "\uD83C\uDF05", // Sunset
        "night" => "\uD83C\uDF19",   // Moon
        _ => "\uD83D\uDCBC"          // Briefcase (General)
    };
}
