using FrontOfficeERP.ViewModels;

namespace FrontOfficeERP.Views;

public partial class DutyDashboardPage : ContentPage
{
    private readonly DutyDashboardViewModel _viewModel;

    public DutyDashboardPage(DutyDashboardViewModel viewModel)
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

    /// <summary>
    /// Creates a 3D 'Tilt' effect when a card is tapped.
    /// Uses ScaleTo and RotateTo to simulate depth.
    /// </summary>
    private async void OnCardTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not View card)
            return;

        // Quick press-down effect
        await Task.WhenAll(
            card.ScaleTo(0.95, 80, Easing.CubicIn),
            card.RotateYTo(8, 80, Easing.CubicIn),
            card.RotateXTo(-4, 80, Easing.CubicIn)
        );

        // Bounce back with slight overshoot
        await Task.WhenAll(
            card.ScaleTo(1.03, 120, Easing.CubicOut),
            card.RotateYTo(-4, 120, Easing.CubicOut),
            card.RotateXTo(2, 120, Easing.CubicOut)
        );

        // Settle to normal
        await Task.WhenAll(
            card.ScaleTo(1.0, 100, Easing.CubicInOut),
            card.RotateYTo(0, 100, Easing.CubicInOut),
            card.RotateXTo(0, 100, Easing.CubicInOut)
        );
    }

    /// <summary>
    /// Creates a 3D hover-tilt effect when pointer enters a card.
    /// Slightly scales up and rotates for a floating 3D appearance.
    /// </summary>
    private async void OnCardPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is not View card)
            return;

        await Task.WhenAll(
            card.ScaleTo(1.04, 200, Easing.CubicOut),
            card.RotateYTo(3, 200, Easing.CubicOut),
            card.RotateXTo(-2, 200, Easing.CubicOut)
        );
    }

    /// <summary>
    /// Resets the card to its normal state when pointer exits.
    /// </summary>
    private async void OnCardPointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is not View card)
            return;

        await Task.WhenAll(
            card.ScaleTo(1.0, 200, Easing.CubicInOut),
            card.RotateYTo(0, 200, Easing.CubicInOut),
            card.RotateXTo(0, 200, Easing.CubicInOut)
        );
    }
}
