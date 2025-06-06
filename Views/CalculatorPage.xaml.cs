using Microsoft.Maui.Controls;
using System;
using StudyMateProject.Models;
using StudyMateProject.Services;

namespace StudyMateProject.Views
{
    public partial class CalculatorPage : ContentPage
    {
        private readonly ICalculatorService _calculatorService;
        private readonly CalculatorModel _calculatorModel;

        public CalculatorPage()
        {
            InitializeComponent();
            _calculatorService = new CalculatorService();
            _calculatorModel = new CalculatorModel();

            UpdateDisplay();
        }

        private void OnNumberClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string number = button.Text;

            if (_calculatorModel.IsNewCalculation)
            {
                _calculatorModel.DisplayValue = number == "0" ? "0" : number;
                _calculatorModel.CurrentExpression = number;
                _calculatorModel.IsNewCalculation = false;
            }
            else
            {
                if (_calculatorModel.DisplayValue == "0")
                {
                    _calculatorModel.DisplayValue = number;
                    _calculatorModel.CurrentExpression = number;
                }
                else
                {
                    _calculatorModel.DisplayValue += number;
                    _calculatorModel.CurrentExpression += number;
                }
            }

            UpdateDisplay();
        }

        private void OnOperatorClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string operatorSymbol = button.Text;

            // Конвертируем отображаемые символы в математические
            string mathOperator = operatorSymbol switch
            {
                "÷" => "/",
                "×" => "*",
                "−" => "-",
                "+" => "+",
                "%" => "%",
                _ => operatorSymbol
            };

            if (!_calculatorModel.IsNewCalculation)
            {
                // Если уже есть выражение, добавляем оператор
                _calculatorModel.CurrentExpression += $" {mathOperator} ";
                _calculatorModel.LastOperation = mathOperator;
                _calculatorModel.WaitingForOperand = true;
                _calculatorModel.HasDecimalPoint = false;
            }
            else if (_calculatorModel.LastResult != 0)
            {
                // Если начинаем новое вычисление с предыдущим результатом
                _calculatorModel.CurrentExpression = $"{_calculatorModel.LastResult} {mathOperator} ";
                _calculatorModel.IsNewCalculation = false;
                _calculatorModel.LastOperation = mathOperator;
                _calculatorModel.WaitingForOperand = true;
            }

            UpdateDisplay();
        }

        private void OnDecimalClicked(object sender, EventArgs e)
        {
            if (_calculatorModel.HasDecimalPoint)
                return;

            if (_calculatorModel.IsNewCalculation)
            {
                _calculatorModel.DisplayValue = "0.";
                _calculatorModel.CurrentExpression = "0.";
                _calculatorModel.IsNewCalculation = false;
            }
            else
            {
                _calculatorModel.DisplayValue += ".";
                _calculatorModel.CurrentExpression += ".";
            }

            _calculatorModel.HasDecimalPoint = true;
            UpdateDisplay();
        }

        private void OnEqualsClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_calculatorModel.CurrentExpression) ||
                    _calculatorModel.CurrentExpression == "0")
                    return;

                // Проверяем корректность выражения
                if (!_calculatorService.IsValidExpression(_calculatorModel.CurrentExpression))
                {
                    ShowError("Некорректное выражение");
                    return;
                }

                // Вычисляем результат
                double result = _calculatorService.Calculate(_calculatorModel.CurrentExpression);
                string formattedResult = _calculatorService.FormatResult(result);

                // Добавляем в историю
                string historyEntry = $"{_calculatorModel.CurrentExpression} = {formattedResult}";
                _calculatorModel.AddToHistory(historyEntry);

                // Обновляем состояние
                _calculatorModel.LastResult = result;
                _calculatorModel.DisplayValue = formattedResult;
                _calculatorModel.IsNewCalculation = true;
                _calculatorModel.HasDecimalPoint = formattedResult.Contains(".");
                _calculatorModel.WaitingForOperand = false;

                UpdateDisplay();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}");
            }
        }

        private void OnSquareRootClicked(object sender, EventArgs e)
        {
            try
            {
                // Если есть текущее значение для извлечения корня
                if (double.TryParse(_calculatorModel.DisplayValue, out double value))
                {
                    double result = _calculatorService.CalculateSquareRoot(value);
                    string formattedResult = _calculatorService.FormatResult(result);

                    // Добавляем в историю
                    string historyEntry = $"√{value} = {formattedResult}";
                    _calculatorModel.AddToHistory(historyEntry);

                    // Обновляем состояние
                    _calculatorModel.LastResult = result;
                    _calculatorModel.DisplayValue = formattedResult;
                    _calculatorModel.CurrentExpression = formattedResult;
                    _calculatorModel.IsNewCalculation = true;
                    _calculatorModel.HasDecimalPoint = formattedResult.Contains(".");

                    UpdateDisplay();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}");
            }
        }

        private void OnClearClicked(object sender, EventArgs e)
        {
            // Полная очистка (C)
            _calculatorModel.Reset();
            UpdateDisplay();
        }

        private void OnClearEntryClicked(object sender, EventArgs e)
        {
            // Очистка последнего ввода (CE)
            _calculatorModel.DisplayValue = "0";
            _calculatorModel.HasDecimalPoint = false;

            // Если мы в середине выражения, удаляем последний операнд
            if (!_calculatorModel.IsNewCalculation && _calculatorModel.WaitingForOperand)
            {
                // Находим последний оператор и обрезаем выражение
                int lastOperatorIndex = Math.Max(
                    Math.Max(_calculatorModel.CurrentExpression.LastIndexOf('+'),
                            _calculatorModel.CurrentExpression.LastIndexOf('-')),
                    Math.Max(_calculatorModel.CurrentExpression.LastIndexOf('*'),
                            _calculatorModel.CurrentExpression.LastIndexOf('/'))
                );

                if (lastOperatorIndex > 0)
                {
                    _calculatorModel.CurrentExpression = _calculatorModel.CurrentExpression.Substring(0, lastOperatorIndex + 2);
                }
            }

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            DisplayLabel.Text = _calculatorModel.DisplayValue;
            ExpressionLabel.Text = _calculatorModel.CurrentExpression;
        }

        private void ShowError(string message)
        {
            DisplayLabel.Text = "Ошибка";
            ExpressionLabel.Text = message;

            // Сброс состояния после ошибки
            _calculatorModel.Reset();
        }
    }
}