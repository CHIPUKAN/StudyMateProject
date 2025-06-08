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
        private string _currentExpression = "";
        private bool _justCalculated = false;

        public CalculatorPage()
        {
            InitializeComponent();
            _calculatorService = new CalculatorService();
            _calculatorModel = new CalculatorModel();
            UpdateDisplay();
        }

        private void OnNumberClicked(object sender, EventArgs e)
        {
            if (sender is not Button button) return;
            string number = button.Text;

            if (_justCalculated)
            {
                // После расчета начинаем новое выражение
                _currentExpression = number;
                _justCalculated = false;
            }
            else
            {
                _currentExpression += number;
            }

            UpdateDisplay();
        }

        private void OnOperatorClicked(object sender, EventArgs e)
        {
            if (sender is not Button button) return;
            string operatorSymbol = button.Text;

            // Конвертируем символы для отображения
            string mathOperator = operatorSymbol switch
            {
                "÷" => " ÷ ",
                "×" => " × ",
                "−" => " − ",
                "+" => " + ",
                _ => " " + operatorSymbol + " "
            };

            if (_justCalculated)
            {
                // После расчета используем результат
                _currentExpression = _calculatorService.FormatResult(_calculatorModel.LastResult) + mathOperator;
                _justCalculated = false;
            }
            else if (!string.IsNullOrEmpty(_currentExpression))
            {
                // Проверяем, не заканчивается ли выражение уже оператором
                if (_currentExpression.TrimEnd().EndsWith(" "))
                {
                    // Заменяем последний оператор
                    var trimmed = _currentExpression.TrimEnd();
                    var lastSpaceIndex = trimmed.LastIndexOf(' ');
                    if (lastSpaceIndex > 0)
                    {
                        _currentExpression = trimmed.Substring(0, lastSpaceIndex) + mathOperator;
                    }
                }
                else
                {
                    // Добавляем новый оператор
                    _currentExpression += mathOperator;
                }
            }
            else if (_calculatorModel.LastResult != 0)
            {
                // Начинаем с предыдущего результата
                _currentExpression = _calculatorService.FormatResult(_calculatorModel.LastResult) + mathOperator;
            }

            UpdateDisplay();
        }

        private void OnDecimalClicked(object sender, EventArgs e)
        {
            if (_justCalculated)
            {
                // После расчета начинаем новое число
                _currentExpression = "0.";
                _justCalculated = false;
            }
            else
            {
                // Находим последнее число в выражении
                var parts = _currentExpression.Split(new[] { " + ", " − ", " × ", " ÷ " }, StringSplitOptions.None);
                string lastPart = parts[^1].Trim();

                // Проверяем, есть ли уже точка в последнем числе
                if (!lastPart.Contains("."))
                {
                    if (string.IsNullOrEmpty(lastPart) || lastPart.EndsWith("(") || lastPart.EndsWith("√"))
                    {
                        _currentExpression += "0.";
                    }
                    else
                    {
                        _currentExpression += ".";
                    }
                }
            }

            UpdateDisplay();
        }

        private void OnPercentClicked(object sender, EventArgs e)
        {
            try
            {
                if (_justCalculated)
                {
                    // Применяем процент к результату
                    double percentValue = _calculatorModel.LastResult / 100;
                    _currentExpression = _calculatorService.FormatResult(percentValue);
                    _justCalculated = false;
                }
                else if (!string.IsNullOrEmpty(_currentExpression))
                {
                    // Находим последнее число и заменяем его на процент
                    var parts = _currentExpression.Split(new[] { " + ", " − ", " × ", " ÷ " }, StringSplitOptions.None);
                    string lastPart = parts[^1].Trim();

                    if (double.TryParse(lastPart, out double number))
                    {
                        double percentValue = number / 100;
                        string formattedPercent = _calculatorService.FormatResult(percentValue);

                        // Заменяем последнее число
                        if (parts.Length > 1)
                        {
                            parts[^1] = formattedPercent;
                            _currentExpression = string.Join(" ", parts).Replace("  ", " ÷ ").Replace("  ", " × ").Replace("  ", " − ").Replace("  ", " + ");
                        }
                        else
                        {
                            _currentExpression = formattedPercent;
                        }
                    }
                }

                UpdateDisplay();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка процентов: {ex.Message}");
            }
        }

        private void OnEqualsClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_currentExpression))
                    return;

                // Подготавливаем выражение для вычисления
                string expression = _currentExpression.Trim();

                // Убираем оператор в конце, если есть
                while (expression.EndsWith(" + ") || expression.EndsWith(" − ") ||
                       expression.EndsWith(" × ") || expression.EndsWith(" ÷ "))
                {
                    var lastSpaceIndex = expression.TrimEnd().LastIndexOf(' ');
                    if (lastSpaceIndex > 0)
                    {
                        expression = expression.Substring(0, lastSpaceIndex);
                    }
                    else
                    {
                        break;
                    }
                }

                if (!_calculatorService.IsValidExpression(expression))
                {
                    ShowError("Некорректное выражение");
                    return;
                }

                double result = _calculatorService.Calculate(expression);
                string formattedResult = _calculatorService.FormatResult(result);

                // Добавляем в историю
                string historyEntry = $"{_currentExpression.Trim()} = {formattedResult}";
                _calculatorModel.AddToHistory(historyEntry);

                // Сохраняем результат
                _calculatorModel.LastResult = result;
                _currentExpression = formattedResult;
                _justCalculated = true;

                UpdateDisplay();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка вычисления: {ex.Message}");
            }
        }

        private void OnSquareRootClicked(object sender, EventArgs e)
        {
            if (_justCalculated)
            {
                // После расчета начинаем новое выражение с корнем
                _currentExpression = "√(";
                _justCalculated = false;
            }
            else
            {
                // УБИРАЕМ автоматическое умножение - просто добавляем корень
                _currentExpression += "√(";
            }

            UpdateDisplay();
        }

        private void OnOpenParenthesisClicked(object sender, EventArgs e)
        {
            if (_justCalculated)
            {
                // После расчета начинаем новое выражение со скобкой
                _currentExpression = "(";
                _justCalculated = false;
            }
            else
            {
                // УБИРАЕМ автоматическое умножение - просто добавляем скобку
                _currentExpression += "(";
            }

            UpdateDisplay();
        }

        private void OnCloseParenthesisClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentExpression) &&
                !_currentExpression.TrimEnd().EndsWith(" ") &&
                !_currentExpression.EndsWith("("))
            {
                _currentExpression += ")";
                UpdateDisplay();
            }
        }

        private void OnClearClicked(object sender, EventArgs e)
        {
            _currentExpression = "";
            _justCalculated = false;
            _calculatorModel.Reset();
            UpdateDisplay();
        }

        private void OnClearEntryClicked(object sender, EventArgs e)
        {
            if (_justCalculated)
            {
                // После расчета CE работает как C
                OnClearClicked(sender, e);
            }
            else
            {
                // Удаляем последний элемент (число или оператор)
                if (_currentExpression.TrimEnd().EndsWith(" "))
                {
                    // Удаляем оператор
                    var trimmed = _currentExpression.TrimEnd();
                    var lastSpaceIndex = trimmed.LastIndexOf(' ');
                    if (lastSpaceIndex > 0)
                    {
                        _currentExpression = trimmed.Substring(0, lastSpaceIndex + 1);
                    }
                    else
                    {
                        _currentExpression = "";
                    }
                }
                else
                {
                    // Удаляем последнее число
                    var parts = _currentExpression.Split(new[] { " + ", " − ", " × ", " ÷ " }, StringSplitOptions.None);
                    if (parts.Length > 1)
                    {
                        parts[^1] = "";
                        _currentExpression = string.Join(" ", parts);
                        _currentExpression = _currentExpression.TrimEnd() + " ";
                    }
                    else
                    {
                        _currentExpression = "";
                    }
                }

                UpdateDisplay();
            }
        }

        private void OnBackspaceClicked(object sender, EventArgs e)
        {
            if (_justCalculated)
            {
                // После расчета начинаем редактировать результат
                _justCalculated = false;
                if (_currentExpression.Length > 1)
                {
                    _currentExpression = _currentExpression[..^1];
                }
                else
                {
                    _currentExpression = "";
                }
            }
            else if (!string.IsNullOrEmpty(_currentExpression))
            {
                // Умное удаление последнего символа
                if (_currentExpression.EndsWith("√("))
                {
                    _currentExpression = _currentExpression[..^2]; // убираем "√("
                }
                else if (_currentExpression.EndsWith(" + ") || _currentExpression.EndsWith(" − ") ||
                         _currentExpression.EndsWith(" × ") || _currentExpression.EndsWith(" ÷ "))
                {
                    _currentExpression = _currentExpression[..^3]; // убираем " op "
                }
                else if (_currentExpression.EndsWith(")"))
                {
                    // При удалении закрывающей скобки нужно проверить, не нарушается ли баланс
                    _currentExpression = _currentExpression[..^1];
                }
                else if (_currentExpression.EndsWith("("))
                {
                    // Если удаляем открывающую скобку, проверяем, не было ли перед ней корня
                    if (_currentExpression.Length >= 2 && _currentExpression.EndsWith("√("))
                    {
                        _currentExpression = _currentExpression[..^2]; // убираем "√("
                    }
                    else
                    {
                        _currentExpression = _currentExpression[..^1]; // убираем "("
                    }
                }
                else
                {
                    _currentExpression = _currentExpression[..^1]; // убираем один символ
                }
            }

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            // Основной дисплей показывает текущее выражение
            DisplayLabel.Text = string.IsNullOrEmpty(_currentExpression) ? "0" : _currentExpression;

            // Верхний дисплей показывает статус
            if (_justCalculated)
            {
                ExpressionLabel.Text = "Результат";
            }
            else
            {
                ExpressionLabel.Text = string.IsNullOrEmpty(_currentExpression) ? "Введите выражение" : "Ввод...";
            }
        }

        private void ShowError(string message)
        {
            DisplayLabel.Text = "Ошибка";
            ExpressionLabel.Text = message;

            _currentExpression = "";
            _justCalculated = false;
            _calculatorModel.Reset();
        }
    }
}