using FrontOfficeERP.ViewModels;

namespace FrontOfficeERP.Views;

public partial class EmployeeMasterPage : ContentPage
{
    private readonly EmployeeMasterViewModel _viewModel;

    public EmployeeMasterPage(EmployeeMasterViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadEmployeesCommand.ExecuteAsync(null);
    }
}
