namespace FrontOfficeERP.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        this.InitializeComponent();
    }

    protected override MauiApp CreateMauiApp() => FrontOfficeERP.MauiProgram.CreateMauiApp();
}
