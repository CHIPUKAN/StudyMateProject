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
            Routing.RegisterRoute("scientific", typeof(ScientificCalculatorPage));
        }
    }
}