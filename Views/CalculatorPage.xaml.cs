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
        private bool _isScientificMode = false;

        public CalculatorPage()
        {
            InitializeComponent();
            _calculatorService = new CalculatorService();
            _calculatorModel = new CalculatorModel();
            UpdateDisplay();
        }

        #region Mode Switching
        private void OnToggleCalculatorModeClicked(object sender, EventArgs e)
        {
            _isScientificMode = !_isScientificMode;

            if (_isScientificMode)
            {
                SwitchToScientificMode();
            }
            else
            {
                SwitchToBasicMode();
            }
        }

        private void SwitchToBasicMode()
        {
            BasicCalculatorPanel.IsVisible = true;
            ScientificCalculatorPanel.IsVisible = false;
            CalculatorModeButton.Text = "📊";
        }

        private void SwitchToScientificMode()
        {
            BasicCalculatorPanel.IsVisible = false;
            ScientificCalculatorPanel.IsVisible = true;
            CalculatorModeButtonScientific.Text = "🔢";
        }
        #endregion

        #region Basic Operations
        private void OnNumberClicked(object sender, EventArgs e)
        {
            if (sender is not Button button) return;
            string number = button.Text;

            if (_justCalculated)
            {
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
                _currentExpression = _calculatorService.FormatResult(_calculatorModel.LastResult) + mathOperator;
                _justCalculated = false;
            }
            else if (!string.IsNullOrEmpty(_currentExpression))
            {
                if (_currentExpression.TrimEnd().EndsWith(" "))
                {
                    var trimmed = _currentExpression.TrimEnd();
                    var lastSpaceIndex = trimmed.LastIndexOf(' ');
                    if (lastSpaceIndex > 0)
                    {
                        _currentExpression = trimmed.Substring(0, lastSpaceIndex) + mathOperator;
                    }
                }
                else
                {
                    _currentExpression += mathOperator;
                }
            }
            else if (_calculatorModel.LastResult != 0)
            {
                _currentExpression = _calculatorService.FormatResult(_calculatorModel.LastResult) + mathOperator;
            }

            UpdateDisplay();
        }

        private void OnDecimalClicked(object sender, EventArgs e)
        {
            if (_justCalculated)
            {
                _currentExpression = "0.";
                _justCalculated = false;
            }
            else
            {
                var parts = _currentExpression.Split(new[] { " + ", " − ", " × ", " ÷ " }, StringSplitOptions.None);
                string lastPart = parts[^1].Trim();

                if (!lastPart.Contains("."))
                {
                    if (string.IsNullOrEmpty(lastPart) || lastPart.EndsWith("("))
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
                    double percentValue = _calculatorModel.LastResult / 100;
                    _currentExpression = _calculatorService.FormatResult(percentValue);
                    _justCalculated = false;
                }
                else if (!string.IsNullOrEmpty(_currentExpression))
                {
                    var parts = _currentExpression.Split(new[] { " + ", " − ", " × ", " ÷ " }, StringSplitOptions.None);
                    string lastPart = parts[^1].Trim();

                    if (double.TryParse(lastPart, out double number))
                    {
                        double percentValue = number / 100;
                        string formattedPercent = _calculatorService.FormatResult(percentValue);

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

                string historyEntry = $"{_currentExpression.Trim()} = {formattedResult}";
                _calculatorModel.AddToHistory(historyEntry);

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
                _currentExpression = "√(";
                _justCalculated = false;
            }
            else
            {
                _currentExpression += "√(";
            }

            UpdateDisplay();
        }

        private void OnOpenParenthesisClicked(object sender, EventArgs e)
        {
            if (_justCalculated)
            {
                _currentExpression = "(";
                _justCalculated = false;
            }
            else
            {
                _currentExpression += "(";
            }

            UpdateDisplay();
        }

        private void OnCloseParenthesisClicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentExpression) &&
                !_currentExpression.TrimEnd().EndsWith(" ") &&
                !_currentExpression.EndsWith("(") &&
                CountOpenParentheses() > CountCloseParentheses())
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
                OnClearClicked(sender, e);
            }
            else
            {
                if (_currentExpression.TrimEnd().EndsWith(" "))
                {
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
                // Умное удаление для научных функций
                if (_currentExpression.EndsWith("sin(") || _currentExpression.EndsWith("cos(") ||
                    _currentExpression.EndsWith("tan(") || _currentExpression.EndsWith("cot(") ||
                    _currentExpression.EndsWith("log("))
                {
                    _currentExpression = _currentExpression[..^4]; // убираем "func("
                }
                else if (_currentExpression.EndsWith("atan("))
                {
                    _currentExpression = _currentExpression[..^5]; // убираем "atan("
                }
                else if (_currentExpression.EndsWith("ln("))
                {
                    _currentExpression = _currentExpression[..^3]; // убираем "ln("
                }
                else if (_currentExpression.EndsWith("√("))
                {
                    _currentExpression = _currentExpression[..^2]; // убираем "√("
                }
                else if (_currentExpression.EndsWith(" + ") || _currentExpression.EndsWith(" − ") ||
                         _currentExpression.EndsWith(" × ") || _currentExpression.EndsWith(" ÷ "))
                {
                    _currentExpression = _currentExpression[..^3]; // убираем " op "
                }
                else
                {
                    _currentExpression = _currentExpression[..^1]; // убираем один символ
                }
            }

            UpdateDisplay();
        }
        #endregion

        #region Scientific Functions
        private void OnSinClicked(object sender, EventArgs e)
        {
            if (_justCalculated)
            {
                _currentExpression = "sin(";
                _justCalculated = false;
            }
            else
            {
                _currentExpression += "sin(";
            }
            UpdateDisplay();
        }

        private void OnCosClicked(object sender, EventArgs e)
        {
            if (_justCalculated)
            {
                _currentExpression = "cos(";
                _justCalculated = false;
            }
            else
            {
                _currentExpression += "cos(";
            }
            UpdateDisplay();
        }

        private void OnTanClicked(object sender, EventArgs e)
        {
            if (_justCalculated)
            {
                _currentExpression = "tan(";
                _justCalculated = false;
            }
            else
            {
                _currentExpression += "tan(";
            }
            UpdateDisplay();
        }

        private void OnCotClicked(object sender, EventArgs e)
        {
            if (_justCalculated)
            {
                _currentExpression = "cot(";
                _justCalculated = false;
            }
            else
            {
                _currentExpression += "cot(";
            }
            UpdateDisplay();
        }

        private void OnArcTanClicked(object sender, EventArgs e)
        {
            if (_justCalculated)
            {
                _currentExpression = "atan(";
                _justCalculated = false;
            }
            else
            {
                _currentExpression += "atan(";
            }
            UpdateDisplay();
        }

        private void OnLnClicked(object sender, EventArgs e)
        {
            if (_justCalculated)
            {
                _currentExpression = "ln(";
                _justCalculated = false;
            }
            else
            {
                _currentExpression += "ln(";
            }
            UpdateDisplay();
        }

        private void OnLogClicked(object sender, EventArgs e)
        {
            if (_justCalculated)
            {
                _currentExpression = "log(";
                _justCalculated = false;
            }
            else
            {
                _currentExpression += "log(";
            }
            UpdateDisplay();
        }

        private void OnSquareClicked(object sender, EventArgs e)
        {
            // x² - возведение в квадрат последнего числа или результата
            if (_justCalculated)
            {
                _currentExpression = $"{_calculatorService.FormatResult(_calculatorModel.LastResult)}²";
                _justCalculated = false;
            }
            else if (!string.IsNullOrEmpty(_currentExpression))
            {
                // Применяем квадрат к последнему числу
                if (_currentExpression.EndsWith(")"))
                {
                    // Если заканчивается закрывающей скобкой, добавляем степень ко всему выражению в скобках
                    _currentExpression += "²";
                }
                else
                {
                    // Находим последнее число и добавляем к нему степень
                    var parts = _currentExpression.Split(new[] { " + ", " − ", " × ", " ÷ " }, StringSplitOptions.None);
                    string lastPart = parts[^1].Trim();

                    if (double.TryParse(lastPart, out _))
                    {
                        parts[^1] = lastPart + "²";
                        _currentExpression = string.Join(" ", parts).Replace("  ", " ÷ ").Replace("  ", " × ").Replace("  ", " − ").Replace("  ", " + ");
                    }
                    else
                    {
                        _currentExpression += "²";
                    }
                }
            }
            UpdateDisplay();
        }

        private void OnCubeClicked(object sender, EventArgs e)
        {
            // x³ - возведение в куб последнего числа или результата
            if (_justCalculated)
            {
                _currentExpression = $"{_calculatorService.FormatResult(_calculatorModel.LastResult)}³";
                _justCalculated = false;
            }
            else if (!string.IsNullOrEmpty(_currentExpression))
            {
                if (_currentExpression.EndsWith(")"))
                {
                    _currentExpression += "³";
                }
                else
                {
                    var parts = _currentExpression.Split(new[] { " + ", " − ", " × ", " ÷ " }, StringSplitOptions.None);
                    string lastPart = parts[^1].Trim();

                    if (double.TryParse(lastPart, out _))
                    {
                        parts[^1] = lastPart + "³";
                        _currentExpression = string.Join(" ", parts).Replace("  ", " ÷ ").Replace("  ", " × ").Replace("  ", " − ").Replace("  ", " + ");
                    }
                    else
                    {
                        _currentExpression += "³";
                    }
                }
            }
            UpdateDisplay();
        }

        private void OnPowerClicked(object sender, EventArgs e)
        {
            // x^y - возведение в произвольную степень
            if (_justCalculated)
            {
                _currentExpression = _calculatorService.FormatResult(_calculatorModel.LastResult) + "^";
                _justCalculated = false;
            }
            else if (!string.IsNullOrEmpty(_currentExpression) &&
                     !_currentExpression.TrimEnd().EndsWith("^"))
            {
                _currentExpression += "^";
            }
            UpdateDisplay();
        }

        private void OnPiClicked(object sender, EventArgs e)
        {
            string piValue = _calculatorService.FormatResult(Math.PI);

            if (_justCalculated)
            {
                _currentExpression = piValue;
                _justCalculated = false;
            }
            else
            {
                _currentExpression += piValue;
            }
            UpdateDisplay();
        }

        private void OnEClicked(object sender, EventArgs e)
        {
            string eValue = _calculatorService.FormatResult(Math.E);

            if (_justCalculated)
            {
                _currentExpression = eValue;
                _justCalculated = false;
            }
            else
            {
                _currentExpression += eValue;
            }
            UpdateDisplay();
        }
        #endregion

        #region Helper Methods
        private int CountOpenParentheses()
        {
            int count = 0;
            foreach (char c in _currentExpression)
            {
                if (c == '(') count++;
            }
            return count;
        }

        private int CountCloseParentheses()
        {
            int count = 0;
            foreach (char c in _currentExpression)
            {
                if (c == ')') count++;
            }
            return count;
        }

        private void UpdateDisplay()
        {
            DisplayLabel.Text = string.IsNullOrEmpty(_currentExpression) ? "0" : _currentExpression;

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
        #endregion
    }
}