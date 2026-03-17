using FrontOfficeERP.Services;

namespace FrontOfficeERP.Views;

public partial class LoginHistoryPage : ContentPage
{
    private readonly AuthService _authService;

    public LoginHistoryPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        RefreshButton.Clicked += async (s, e) => await LoadHistory();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadHistory();
    }

    private async Task LoadHistory()
    {
        var history = await _authService.GetLoginHistoryAsync();
        HistoryList.ItemsSource = history;
    }
}
