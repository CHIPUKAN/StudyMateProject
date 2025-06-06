using StudyMateProject.Services;

namespace StudyMateProject
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Инициализируем основные сервисы
            InitializeServices();

            // Устанавливаем главную оболочку приложения
            MainPage = new AppShell();
        }

        private void InitializeServices()
        {
            // Здесь можно инициализировать сервисы для DI
            // Например, регистрация сервисов в контейнере зависимостей
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            // Настройки окна приложения
            window.Title = "StudyMate - Помощник студента";

            // Минимальный размер окна для Windows
#if WINDOWS
            window.MinimumHeight = 600;
            window.MinimumWidth = 400;
            window.Width = 400;
            window.Height = 700;
#endif

            return window;
        }

        protected override void OnStart()
        {
            // Код, который выполняется при запуске приложения
            base.OnStart();
        }

        protected override void OnSleep()
        {
            // Код, который выполняется когда приложение переходит в фоновый режим
            base.OnSleep();

            // Сохраняем состояние приложения
            SaveApplicationState();
        }

        protected override void OnResume()
        {
            // Код, который выполняется когда приложение возвращается из фонового режима
            base.OnResume();

            // Восстанавливаем состояние приложения
            RestoreApplicationState();
        }

        private void SaveApplicationState()
        {
            // Сохранение состояния приложения при переходе в фон
            // Например, сохранение несохраненных заметок
        }

        private void RestoreApplicationState()
        {
            // Восстановление состояния приложения при возвращении
            // Например, восстановление временных данных
        }
    }
}