using StudyMateProject.Views;

namespace StudyMateProject
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Регистрируем маршруты для навигации
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            // Основные страницы
            Routing.RegisterRoute("main", typeof(MainPage));
            Routing.RegisterRoute("calculator", typeof(CalculatorPage));

            //// Заметки
            //Routing.RegisterRoute("notes", typeof(NotesPage));
            //Routing.RegisterRoute("drawing", typeof(DrawingPage));
            //Routing.RegisterRoute("notedetail", typeof(NoteDetailPage));

            //// Напоминания
            //Routing.RegisterRoute("reminders", typeof(RemindersPage));
            //Routing.RegisterRoute("reminderdetail", typeof(ReminderDetailPage));

            //// Настройки
            //Routing.RegisterRoute("settings", typeof(SettingsPage));

            //// Дополнительные страницы
            //Routing.RegisterRoute("about", typeof(AboutPage));
            //Routing.RegisterRoute("sync", typeof(SyncPage));
        }
    }
}