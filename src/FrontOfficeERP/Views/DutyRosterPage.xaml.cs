using FrontOfficeERP.ViewModels;

namespace FrontOfficeERP.Views;

public partial class DutyRosterPage : ContentPage
{
    private readonly DutyRosterViewModel _viewModel;

    public DutyRosterPage(DutyRosterViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDataCommand.ExecuteAsync(null);
    }
}
