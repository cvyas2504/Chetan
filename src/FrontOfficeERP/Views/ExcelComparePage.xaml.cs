using FrontOfficeERP.ViewModels;

namespace FrontOfficeERP.Views;

public partial class ExcelComparePage : ContentPage
{
    public ExcelComparePage(ExcelCompareViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
