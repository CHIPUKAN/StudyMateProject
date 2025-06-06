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

            // Сервисы для заметок (будущие)
            services.AddSingleton<INotesService, NotesService>();
            services.AddSingleton<IDrawingService, DrawingService>();

            // Сервисы для напоминаний (будущие)
            services.AddSingleton<IRemindersService, RemindersService>();

            // Сервисы синхронизации (будущие)
            services.AddSingleton<ISyncService, SyncService>();

            // Сервисы настроек
            services.AddSingleton<ISettingsService, SettingsService>();

            // Сервисы данных
            services.AddSingleton<IDataService, DataService>();
        }

        private static void RegisterViewModels(IServiceCollection services)
        {
            // ViewModels для основных страниц
            services.AddTransient<MainPageViewModel>();
            services.AddTransient<CalculatorViewModel>();

            // ViewModels для заметок (будущие)
            services.AddTransient<NotesViewModel>();
            services.AddTransient<DrawingViewModel>();
            services.AddTransient<NoteDetailViewModel>();

            // ViewModels для напоминаний (будущие)
            services.AddTransient<RemindersViewModel>();
            services.AddTransient<ReminderDetailViewModel>();

            // ViewModels для настроек
            services.AddTransient<SettingsViewModel>();
        }

        private static void RegisterViews(IServiceCollection services)
        {
            // Основные страницы
            services.AddTransient<MainPage>();
            services.AddTransient<CalculatorPage>();

            // Страницы заметок (будущие)
            services.AddTransient<NotesPage>();
            services.AddTransient<DrawingPage>();
            services.AddTransient<NoteDetailPage>();

            // Страницы напоминаний (будущие)
            services.AddTransient<RemindersPage>();
            services.AddTransient<ReminderDetailPage>();

            // Страницы настроек
            services.AddTransient<SettingsPage>();
            services.AddTransient<AboutPage>();
            services.AddTransient<SyncPage>();
        }
    }

    // Заглушки для будущих сервисов
    public interface INotesService { }
    public class NotesService : INotesService { }

    public interface IDrawingService { }
    public class DrawingService : IDrawingService { }

    public interface IRemindersService { }
    public class RemindersService : IRemindersService { }

    public interface ISyncService { }
    public class SyncService : ISyncService { }

    public interface ISettingsService { }
    public class SettingsService : ISettingsService { }

    public interface IDataService { }
    public class DataService : IDataService { }

    // Заглушки для будущих ViewModels
    public class MainPageViewModel { }
    public class NotesViewModel { }
    public class DrawingViewModel { }
    public class NoteDetailViewModel { }
    public class RemindersViewModel { }
    public class ReminderDetailViewModel { }
    public class SettingsViewModel { }

    // Заглушки для будущих Views
    public class NotesPage : ContentPage { }
    public class DrawingPage : ContentPage { }
    public class NoteDetailPage : ContentPage { }
    public class RemindersPage : ContentPage { }
    public class ReminderDetailPage : ContentPage { }
    public class SettingsPage : ContentPage { }
    public class AboutPage : ContentPage { }
    public class SyncPage : ContentPage { }
}