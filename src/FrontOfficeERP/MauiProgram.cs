using CommunityToolkit.Maui;
using FrontOfficeERP.Data;
using FrontOfficeERP.Services;
using FrontOfficeERP.ViewModels;
using FrontOfficeERP.Views;
using Microsoft.Extensions.Logging;

namespace FrontOfficeERP;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // Data
        builder.Services.AddSingleton<DatabaseService>();

        // Services
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<EmployeeService>();
        builder.Services.AddSingleton<DutyService>();
        builder.Services.AddSingleton<ExportService>();
        builder.Services.AddSingleton<ExcelCompareService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<EmployeeMasterViewModel>();
        builder.Services.AddTransient<DutyMasterViewModel>();
        builder.Services.AddTransient<DutyRosterViewModel>();
        builder.Services.AddTransient<DutyDashboardViewModel>();
        builder.Services.AddTransient<UserMasterViewModel>();
        builder.Services.AddTransient<ExcelCompareViewModel>();

        // Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<EmployeeMasterPage>();
        builder.Services.AddTransient<DutyMasterPage>();
        builder.Services.AddTransient<DutyRosterPage>();
        builder.Services.AddTransient<DutyDashboardPage>();
        builder.Services.AddTransient<UserMasterPage>();
        builder.Services.AddTransient<LoginHistoryPage>();
        builder.Services.AddTransient<ExcelComparePage>();

        return builder.Build();
    }
}
