using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using StudyMateProject.Services;
using StudyMateProject.ViewModels;
using StudyMateProject.Views;

namespace StudyMateProject
{
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

            // Регистрация сервисов
            RegisterServices(builder.Services);

            // Регистрация ViewModels
            RegisterViewModels(builder.Services);

            // Регистрация Views (страниц)
            RegisterViews(builder.Services);

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static void RegisterServices(IServiceCollection services)
        {
            // Сервисы калькулятора
            services.AddSingleton<ICalculatorService, CalculatorService>();
        }

        private static void RegisterViewModels(IServiceCollection services)
        {
            // ViewModels для основных страниц
            services.AddTransient<CalculatorViewModel>();
        }

        private static void RegisterViews(IServiceCollection services)
        {
            // Основные страницы
            services.AddTransient<MainPage>();
            services.AddTransient<CalculatorPage>();
            services.AddTransient<ScientificCalculatorPage>();
        }
    }
}