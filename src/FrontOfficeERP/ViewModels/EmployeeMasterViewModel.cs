using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FrontOfficeERP.Models;
using FrontOfficeERP.Services;

namespace FrontOfficeERP.ViewModels;

public partial class EmployeeMasterViewModel : ObservableObject
{
    private readonly EmployeeService _employeeService;

    [ObservableProperty]
    private ObservableCollection<Employee> _employees = new();

    [ObservableProperty]
    private Employee? _selectedEmployee;

    [ObservableProperty]
    private string _employeeCode = string.Empty;

    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private string _department = string.Empty;

    [ObservableProperty]
    private string _designation = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private DateTime _dateOfJoining = DateTime.Today;

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public EmployeeMasterViewModel(EmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [RelayCommand]
    private async Task LoadEmployeesAsync()
    {
        IsBusy = true;
        try
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            Employees = new ObservableCollection<Employee>(employees);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void NewEmployee()
    {
        SelectedEmployee = null;
        EmployeeCode = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
        Department = string.Empty;
        Designation = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        DateOfJoining = DateTime.Today;
        IsActive = true;
        IsEditing = true;
    }

    [RelayCommand]
    private void EditEmployee(Employee employee)
    {
        SelectedEmployee = employee;
        EmployeeCode = employee.EmployeeCode;
        FirstName = employee.FirstName;
        LastName = employee.LastName;
        Department = employee.Department;
        Designation = employee.Designation;
        Email = employee.Email;
        Phone = employee.Phone;
        DateOfJoining = employee.DateOfJoining ?? DateTime.Today;
        IsActive = employee.IsActive;
        IsEditing = true;
    }

    [RelayCommand]
    private async Task SaveEmployeeAsync()
    {
        if (string.IsNullOrWhiteSpace(EmployeeCode) || string.IsNullOrWhiteSpace(FirstName))
        {
            await Shell.Current.DisplayAlert("Validation", "Employee Code and First Name are required.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            var employee = SelectedEmployee ?? new Employee();
            employee.EmployeeCode = EmployeeCode;
            employee.FirstName = FirstName;
            employee.LastName = LastName;
            employee.Department = Department;
            employee.Designation = Designation;
            employee.Email = Email;
            employee.Phone = Phone;
            employee.DateOfJoining = DateOfJoining;
            employee.IsActive = IsActive;

            var (success, message) = await _employeeService.SaveEmployeeAsync(employee);
            await Shell.Current.DisplayAlert(success ? "Success" : "Error", message, "OK");

            if (success)
            {
                IsEditing = false;
                await LoadEmployeesAsync();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteEmployeeAsync(Employee employee)
    {
        bool confirm = await Shell.Current.DisplayAlert("Confirm", $"Deactivate {employee.FullName}?", "Yes", "No");
        if (!confirm) return;

        await _employeeService.DeleteEmployeeAsync(employee.Id);
        await LoadEmployeesAsync();
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        SelectedEmployee = null;
    }
}
