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
        private bool _isMatrixMode = false;

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
            CalculatorSelectorOverlay.IsVisible = true;
            CalculatorSelectorOverlay.FadeTo(1, 250, Easing.CubicOut);
        }

        private async void OnBasicCalculatorSelected(object sender, EventArgs e)
        {
            await CalculatorSelectorOverlay.FadeTo(0, 200, Easing.CubicIn);
            CalculatorSelectorOverlay.IsVisible = false;
            SwitchToBasicMode();
        }

        private async void OnScientificCalculatorSelected(object sender, EventArgs e)
        {
            await CalculatorSelectorOverlay.FadeTo(0, 200, Easing.CubicIn);
            CalculatorSelectorOverlay.IsVisible = false;
            SwitchToScientificMode();
        }

        private async void OnCancelSelection(object sender, EventArgs e)
        {
            await CalculatorSelectorOverlay.FadeTo(0, 200, Easing.CubicIn);
            CalculatorSelectorOverlay.IsVisible = false;
        }

        private void SwitchToBasicMode()
        {
            BasicCalculatorPanel.IsVisible = true;
            ScientificCalculatorPanel.IsVisible = false;
            MatrixCalculatorPanel.IsVisible = false;
            _isScientificMode = false;
            _isMatrixMode = false;
        }

        private void SwitchToScientificMode()
        {
            BasicCalculatorPanel.IsVisible = false;
            ScientificCalculatorPanel.IsVisible = true;
            MatrixCalculatorPanel.IsVisible = false;
            _isScientificMode = true;
            _isMatrixMode = false;
        }

        private async void OnMatrixCalculatorSelected(object sender, EventArgs e)
        {
            await CalculatorSelectorOverlay.FadeTo(0, 200, Easing.CubicIn);
            CalculatorSelectorOverlay.IsVisible = false;
            SwitchToMatrixMode();
        }

        private void SwitchToMatrixMode()
        {
            BasicCalculatorPanel.IsVisible = false;
            ScientificCalculatorPanel.IsVisible = false;
            MatrixCalculatorPanel.IsVisible = true;
            _isScientificMode = false;
            _isMatrixMode = true;
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
                    _currentExpression = _currentExpression[..^4];
                }
                else if (_currentExpression.EndsWith("ln("))
                {
                    _currentExpression = _currentExpression[..^3];
                }
                else if (_currentExpression.EndsWith("√(") || _currentExpression.EndsWith("∛("))
                {
                    _currentExpression = _currentExpression[..^2];
                }
                else if (_currentExpression.EndsWith(" + ") || _currentExpression.EndsWith(" − ") ||
                         _currentExpression.EndsWith(" × ") || _currentExpression.EndsWith(" ÷ "))
                {
                    _currentExpression = _currentExpression[..^3];
                }
                else
                {
                    _currentExpression = _currentExpression[..^1];
                }
            }

            UpdateDisplay();
        }
        #endregion

        #region Scientific Functions

        // Тригонометрические функции
        private void OnScientificFunctionClicked(object sender, EventArgs e)
        {
            if (sender is not Button button) return;
            string function = button.Text;

            if (_justCalculated)
            {
                _currentExpression = $"{function}(";
                _justCalculated = false;
            }
            else
            {
                _currentExpression += $"{function}(";
            }

            UpdateDisplay();
        }

        // Степенные функции
        private void OnSquareClicked(object sender, EventArgs e)
        {
            if (_justCalculated)
            {
                _currentExpression = $"{_calculatorService.FormatResult(_calculatorModel.LastResult)}²";
                _justCalculated = false;
            }
            else if (!string.IsNullOrEmpty(_currentExpression))
            {
                if (_currentExpression.EndsWith(")"))
                {
                    _currentExpression += "²";
                }
                else
                {
                    var parts = _currentExpression.Split(new[] { " + ", " − ", " × ", " ÷ " }, StringSplitOptions.None);
                    string lastPart = parts[^1].Trim();

                    if (double.TryParse(lastPart, out _) || lastPart == "π" || lastPart == "e")
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

                    if (double.TryParse(lastPart, out _) || lastPart == "π" || lastPart == "e")
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

        private void OnPowerYClicked(object sender, EventArgs e)
        {
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

        private void OnFactorialClicked(object sender, EventArgs e)
        {
            if (_justCalculated)
            {
                _currentExpression = "fact(";
                _justCalculated = false;
            }
            else
            {
                _currentExpression += "fact(";
            }
            UpdateDisplay();
        }

        // Корни
        private void OnCubeRootClicked(object sender, EventArgs e)
        {
            if (_justCalculated)
            {
                _currentExpression = "∛(";
                _justCalculated = false;
            }
            else
            {
                _currentExpression += "∛(";
            }
            UpdateDisplay();
        }

        // Константы
        private void OnPiClicked(object sender, EventArgs e)
        {
            if (_justCalculated)
            {
                _currentExpression = "π";
                _justCalculated = false;
            }
            else
            {
                _currentExpression += "π";
            }
            UpdateDisplay();
        }

        private void OnEClicked(object sender, EventArgs e)
        {
            if (_justCalculated)
            {
                _currentExpression = "e";
                _justCalculated = false;
            }
            else
            {
                _currentExpression += "e";
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