using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FrontOfficeERP.Models;
using FrontOfficeERP.Services;

namespace FrontOfficeERP.ViewModels;

public partial class DutyMasterViewModel : ObservableObject
{
    private readonly DutyService _dutyService;

    [ObservableProperty]
    private ObservableCollection<DutyMaster> _duties = new();

    [ObservableProperty]
    private DutyMaster? _selectedDuty;

    [ObservableProperty]
    private string _dutyCode = string.Empty;

    [ObservableProperty]
    private string _dutyName = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _startTime = string.Empty;

    [ObservableProperty]
    private string _endTime = string.Empty;

    [ObservableProperty]
    private string _shiftType = "General";

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isBusy;

    public List<string> ShiftTypes { get; } = new() { "General", "Morning", "Afternoon", "Night", "Off" };

    public DutyMasterViewModel(DutyService dutyService)
    {
        _dutyService = dutyService;
    }

    [RelayCommand]
    private async Task LoadDutiesAsync()
    {
        IsBusy = true;
        try
        {
            var duties = await _dutyService.GetAllDutiesAsync();
            Duties = new ObservableCollection<DutyMaster>(duties);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void NewDuty()
    {
        SelectedDuty = null;
        DutyCode = string.Empty;
        DutyName = string.Empty;
        Description = string.Empty;
        StartTime = string.Empty;
        EndTime = string.Empty;
        ShiftType = "General";
        IsActive = true;
        IsEditing = true;
    }

    [RelayCommand]
    private void EditDuty(DutyMaster duty)
    {
        SelectedDuty = duty;
        DutyCode = duty.DutyCode;
        DutyName = duty.DutyName;
        Description = duty.Description;
        StartTime = duty.StartTime;
        EndTime = duty.EndTime;
        ShiftType = duty.ShiftType;
        IsActive = duty.IsActive;
        IsEditing = true;
    }

    [RelayCommand]
    private async Task SaveDutyAsync()
    {
        if (string.IsNullOrWhiteSpace(DutyCode) || string.IsNullOrWhiteSpace(DutyName))
        {
            await Shell.Current.DisplayAlert("Validation", "Duty Code and Duty Name are required.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            var duty = SelectedDuty ?? new DutyMaster();
            duty.DutyCode = DutyCode;
            duty.DutyName = DutyName;
            duty.Description = Description;
            duty.StartTime = StartTime;
            duty.EndTime = EndTime;
            duty.ShiftType = ShiftType;
            duty.IsActive = IsActive;

            var (success, message) = await _dutyService.SaveDutyAsync(duty);
            await Shell.Current.DisplayAlert(success ? "Success" : "Error", message, "OK");

            if (success)
            {
                IsEditing = false;
                await LoadDutiesAsync();
            }
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
        SelectedDuty = null;
    }
}
