using FrontOfficeERP.ViewModels;

namespace FrontOfficeERP.Views;

public partial class DutyMasterPage : ContentPage
{
    private readonly DutyMasterViewModel _viewModel;

    public DutyMasterPage(DutyMasterViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDutiesCommand.ExecuteAsync(null);
    }
}
