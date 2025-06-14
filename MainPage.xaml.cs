using Microsoft.Maui.Controls;

namespace StudyMateProject
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        // Только калькулятор работает
        private async void OnCalculatorClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("//CalculatorPage");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось открыть калькулятор: {ex.Message}", "ОК");
            }
        }

        // Заглушки для остальных кнопок
        private async void OnNotebooksClicked(object sender, EventArgs e)
        {
            await DisplayAlert("— В РАЗРАБОТКЕ —",
                              "Функция 'Мои тетради' будет добавлена в следующих версиях приложения",
                              "ПОНЯТНО");
        }

        private async void OnRemindersClicked(object sender, EventArgs e)
        {
            await DisplayAlert("— В РАЗРАБОТКЕ —",
                              "Функция 'Мои напоминания' будет добавлена в следующих версиях приложения",
                              "ПОНЯТНО");
        }
    }
}