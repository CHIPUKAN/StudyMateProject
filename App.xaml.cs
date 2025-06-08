namespace StudyMateProject
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Устанавливаем главную оболочку приложения
            MainPage = new AppShell();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);

            // Настройки окна приложения
            window.Title = "StudyMate Calculator";

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
        }

        protected override void OnResume()
        {
            // Код, который выполняется когда приложение возвращается из фонового режима
            base.OnResume();
        }
    }
}