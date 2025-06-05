using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using StudyMateProject.Services;
using StudyMateProject.ViewModels;
using StudyMateProject.Views;

namespace StudyMateProject;

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
            });

        // Регистрируем Services
        builder.Services.AddSingleton<ICalculationService, CalculationService>();
        builder.Services.AddSingleton<IMatrixService, MatrixService>();
        builder.Services.AddSingleton<IAdvancedMathService, AdvancedMathService>();
        builder.Services.AddSingleton<ICalculatorIntegrationService, CalculatorIntegrationService>();

        // Регистрируем ViewModels
        builder.Services.AddTransient<CalculatorViewModel>();
        builder.Services.AddTransient<MiniCalculatorViewModel>();
        builder.Services.AddTransient<MatrixCalculatorViewModel>();
        builder.Services.AddTransient<AdvancedMathViewModel>();
        builder.Services.AddTransient<HistoryViewModel>();

        // Регистрируем Views/Pages
        builder.Services.AddTransient<CalculatorPage>();
        builder.Services.AddTransient<MatrixCalculatorPage>();
        builder.Services.AddTransient<AdvancedMathPage>();

        builder.Logging.AddDebug();

        return builder.Build();
    }
}