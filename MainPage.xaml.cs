using System;
using System.Collections.Generic;

namespace StudyMateProject
{
    public partial class MainPage : ContentPage
    {
        private readonly List<string> _calculatorTips;
        private readonly Random _random;
        private DateTime _sessionStartTime;

        public MainPage()
        {
            InitializeComponent();

            _random = new Random();
            _sessionStartTime = DateTime.Now;

            _calculatorTips = new List<string>
            {
                "Используйте скобки для сложных вычислений: (2+3)×4 = 20",
                "Квадратный корень: √16 = 4",
                "Проценты: 15% от 200 = 30",
                "Десятичные числа: 3.14 × 2 = 6.28",
                "Последовательность операций: 2+3×4 = 14 (сначала умножение)",
                "Научный калькулятор: sin(π/2) = 1",
                "Степенные функции: 2³ = 8",
                "Логарифмы: ln(e) = 1",
                "Константа π ≈ 3.14159",
                "Константа e ≈ 2.71828"
            };

            LoadMainPageData();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RefreshStatistics();
            ShowRandomTip();
        }

        private void LoadMainPageData()
        {
            RefreshStatistics();
            ShowRandomTip();
        }

        private void RefreshStatistics()
        {
            // Здесь будет логика получения реальной статистики калькулятора
            CalculationsCountLabel.Text = GetCalculationsCount().ToString();

            // Время сессии
            var sessionTime = DateTime.Now - _sessionStartTime;
            SessionTimeLabel.Text = $"{(int)sessionTime.TotalMinutes} мин";

            // Последнее вычисление
            var lastCalculation = GetLastCalculation();
            LastCalculationLabel.Text = string.IsNullOrEmpty(lastCalculation)
                ? "Пока нет вычислений"
                : lastCalculation;
        }

        private void ShowRandomTip()
        {
            int randomIndex = _random.Next(_calculatorTips.Count);
            TipOfDayLabel.Text = _calculatorTips[randomIndex];
        }

        #region Event Handlers

        private async void OnCalculatorClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("//calculator");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось открыть калькулятор: {ex.Message}", "ОК");
            }
        }

        private async void OnScientificCalculatorClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync("//scientific");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось открыть научный калькулятор: {ex.Message}", "ОК");
            }
        }

        #endregion

        #region Data Methods (заглушки)

        private int GetCalculationsCount()
        {
            // Здесь будет обращение к сервису данных калькулятора
            return _random.Next(0, 50);
        }

        private string GetLastCalculation()
        {
            // Здесь будет обращение к истории вычислений
            var sampleCalculations = new[]
            {
                "",
                "25 + 17 = 42",
                "√144 = 12",
                "(5+3) × 2 = 16",
                "100 - 25% = 75",
                "sin(π/2) = 1",
                "2³ = 8",
                "ln(e) = 1"
            };

            return sampleCalculations[_random.Next(sampleCalculations.Length)];
        }

        #endregion
    }
}