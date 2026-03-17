using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FrontOfficeERP.Models;
using FrontOfficeERP.Services;

namespace FrontOfficeERP.ViewModels;

public partial class UserMasterViewModel : ObservableObject
{
    private readonly AuthService _authService;

    [ObservableProperty]
    private ObservableCollection<User> _users = new();

    [ObservableProperty]
    private User? _selectedUser;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _role = "User";

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _isBusy;

    public List<string> Roles { get; } = new() { "Admin", "Manager", "User" };

    public UserMasterViewModel(AuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task LoadUsersAsync()
    {
        IsBusy = true;
        try
        {
            var users = await _authService.GetAllUsersAsync();
            Users = new ObservableCollection<User>(users);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void NewUser()
    {
        SelectedUser = null;
        Username = string.Empty;
        Password = string.Empty;
        FullName = string.Empty;
        Email = string.Empty;
        Role = "User";
        IsActive = true;
        IsEditing = true;
    }

    [RelayCommand]
    private void EditUser(User user)
    {
        SelectedUser = user;
        Username = user.Username;
        Password = string.Empty;
        FullName = user.FullName;
        Email = user.Email;
        Role = user.Role;
        IsActive = user.IsActive;
        IsEditing = true;
    }

    [RelayCommand]
    private async Task SaveUserAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(FullName))
        {
            await Shell.Current.DisplayAlert("Validation", "Username and Full Name are required.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            if (SelectedUser is null)
            {
                if (string.IsNullOrWhiteSpace(Password))
                {
                    await Shell.Current.DisplayAlert("Validation", "Password is required for new users.", "OK");
                    return;
                }

                var user = new User
                {
                    Username = Username,
                    PasswordHash = Password,
                    FullName = FullName,
                    Email = Email,
                    Role = Role,
                    IsActive = IsActive
                };

                var (success, message) = await _authService.CreateUserAsync(user);
                await Shell.Current.DisplayAlert(success ? "Success" : "Error", message, "OK");
                if (success)
                {
                    IsEditing = false;
                    await LoadUsersAsync();
                }
            }
            else
            {
                SelectedUser.Username = Username;
                SelectedUser.FullName = FullName;
                SelectedUser.Email = Email;
                SelectedUser.Role = Role;
                SelectedUser.IsActive = IsActive;

                var (success, message) = await _authService.UpdateUserAsync(SelectedUser);
                await Shell.Current.DisplayAlert(success ? "Success" : "Error", message, "OK");
                if (success)
                {
                    IsEditing = false;
                    await LoadUsersAsync();
                }
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
        SelectedUser = null;
    }
}
