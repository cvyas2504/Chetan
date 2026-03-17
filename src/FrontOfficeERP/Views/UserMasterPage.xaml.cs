using FrontOfficeERP.ViewModels;

namespace FrontOfficeERP.Views;

public partial class UserMasterPage : ContentPage
{
    private readonly UserMasterViewModel _viewModel;

    public UserMasterPage(UserMasterViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadUsersCommand.ExecuteAsync(null);
    }
}
