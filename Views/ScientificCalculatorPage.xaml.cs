using Microsoft.Maui.Controls;
using System;
using StudyMateProject.Models;
using StudyMateProject.Services;

namespace StudyMateProject.Views
{
    public partial class ScientificCalculatorPage : ContentPage
    {
        private readonly ICalculatorService _calculatorService;
        private readonly CalculatorModel _calculatorModel;
        private string _displayExpression = ""; // Для красивого отображения
        private bool _isRadianMode = true; // По умолчанию радианы
        private bool _lastInputWasOperator = false;
        private bool _lastInputWasFunction = false;

        public ScientificCalculatorPage()
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
                // Начинаем новое вычисление
                _calculatorModel.DisplayValue = number == "0" ? "0" : number;
                _calculatorModel.CurrentExpression = number;
                _displayExpression = number;
                _calculatorModel.IsNewCalculation = false;
            }
            else
            {
                // Продолжаем ввод
                if (_lastInputWasOperator || _lastInputWasFunction)
                {
                    // После оператора или функции - новое число
                    _calculatorModel.DisplayValue = number;
                    _calculatorModel.CurrentExpression += number;
                    _displayExpression += number;
                    _lastInputWasOperator = false;
                    _lastInputWasFunction = false;
                }
                else
                {
                    // Добавляем к текущему числу
                    if (_calculatorModel.DisplayValue == "0" && number != "0")
                    {
                        _calculatorModel.DisplayValue = number;
                        // Заменяем последний символ в выражении
                        if (_calculatorModel.CurrentExpression.EndsWith("0"))
                        {
                            _calculatorModel.CurrentExpression = _calculatorModel.CurrentExpression[..^1] + number;
                            _displayExpression = _displayExpression[..^1] + number;
                        }
                    }
                    else
                    {
                        _calculatorModel.DisplayValue += number;
                        _calculatorModel.CurrentExpression += number;
                        _displayExpression += number;
                    }
                }
            }

            _calculatorModel.WaitingForOperand = false;
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

            if (_calculatorModel.IsNewCalculation && _calculatorModel.LastResult != 0)
            {
                // Начинаем новое выражение с предыдущим результатом
                _calculatorModel.CurrentExpression = $"{_calculatorModel.LastResult} {mathOperator} ";
                _displayExpression = $"{_calculatorService.FormatResult(_calculatorModel.LastResult)} {operatorSymbol} ";
                _calculatorModel.IsNewCalculation = false;
            }
            else if (!_lastInputWasOperator && !string.IsNullOrEmpty(_calculatorModel.CurrentExpression))
            {
                // Добавляем оператор к существующему выражению
                _calculatorModel.CurrentExpression += $" {mathOperator} ";
                _displayExpression += $" {operatorSymbol} ";
            }
            else if (_lastInputWasOperator && !string.IsNullOrEmpty(_calculatorModel.CurrentExpression))
            {
                // Заменяем последний оператор
                var parts = _calculatorModel.CurrentExpression.TrimEnd().Split(' ');
                if (parts.Length >= 2)
                {
                    parts[^1] = mathOperator;
                    _calculatorModel.CurrentExpression = string.Join(" ", parts) + " ";
                }

                var displayParts = _displayExpression.TrimEnd().Split(' ');
                if (displayParts.Length >= 2)
                {
                    displayParts[^1] = operatorSymbol;
                    _displayExpression = string.Join(" ", displayParts) + " ";
                }
            }

            _calculatorModel.LastOperation = mathOperator;
            _calculatorModel.WaitingForOperand = true;
            _calculatorModel.HasDecimalPoint = false;
            _lastInputWasOperator = true;
            _lastInputWasFunction = false;

            UpdateDisplay();
        }

        private void OnScientificFunctionClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            string function = button.Text;

            // Автоматически добавляем функцию с скобками
            if (_calculatorModel.IsNewCalculation)
            {
                _calculatorModel.CurrentExpression = $"{function}(";
                _displayExpression = $"{function}(";
                _calculatorModel.DisplayValue = "0";
                _calculatorModel.IsNewCalculation = false;
            }
            else if (_lastInputWasOperator || _lastInputWasFunction)
            {
                _calculatorModel.CurrentExpression += $"{function}(";
                _displayExpression += $"{function}(";
            }
            else
            {
                // Если вводим функцию после числа, добавляем умножение
                _calculatorModel.CurrentExpression += $" * {function}(";
                _displayExpression += $" × {function}(";
            }

            _calculatorModel.WaitingForOperand = true;
            _lastInputWasFunction = true;
            _lastInputWasOperator = false;
            UpdateDisplay();
        }

        private void OnPowerClicked(object sender, EventArgs e)
        {
            // x² - возводим текущее число в квадрат
            if (!_lastInputWasOperator && !string.IsNullOrEmpty(_calculatorModel.DisplayValue))
            {
                _calculatorModel.CurrentExpression += "^2";
                _displayExpression += "²";
                _lastInputWasFunction = false;
                _lastInputWasOperator = false;
                UpdateDisplay();
            }
        }

        private void OnCubeClicked(object sender, EventArgs e)
        {
            // x³ - возводим текущее число в куб
            if (!_lastInputWasOperator && !string.IsNullOrEmpty(_calculatorModel.DisplayValue))
            {
                _calculatorModel.CurrentExpression += "^3";
                _displayExpression += "³";
                _lastInputWasFunction = false;
                _lastInputWasOperator = false;
                UpdateDisplay();
            }
        }

        private void OnPowerYClicked(object sender, EventArgs e)
        {
            // x^y - добавляем оператор степени
            if (!_lastInputWasOperator && !string.IsNullOrEmpty(_calculatorModel.CurrentExpression))
            {
                _calculatorModel.CurrentExpression += " ^ ";
                _displayExpression += " ^ ";
                _calculatorModel.WaitingForOperand = true;
                _lastInputWasOperator = true;
                _lastInputWasFunction = false;
                UpdateDisplay();
            }
        }

        private void OnSquareRootClicked(object sender, EventArgs e)
        {
            // Автоматически добавляем функцию с скобками
            if (_calculatorModel.IsNewCalculation)
            {
                _calculatorModel.CurrentExpression = "sqrt(";
                _displayExpression = "√(";
                _calculatorModel.DisplayValue = "0";
                _calculatorModel.IsNewCalculation = false;
            }
            else if (_lastInputWasOperator || _lastInputWasFunction)
            {
                _calculatorModel.CurrentExpression += "sqrt(";
                _displayExpression += "√(";
            }
            else
            {
                // Если вводим функцию после числа, добавляем умножение
                _calculatorModel.CurrentExpression += " * sqrt(";
                _displayExpression += " × √(";
            }

            _calculatorModel.WaitingForOperand = true;
            _lastInputWasFunction = true;
            _lastInputWasOperator = false;
            UpdateDisplay();
        }

        private void OnPiClicked(object sender, EventArgs e)
        {
            string piValue = Math.PI.ToString("G15");

            if (_calculatorModel.IsNewCalculation)
            {
                _calculatorModel.DisplayValue = piValue;
                _calculatorModel.CurrentExpression = piValue;
                _displayExpression = "π";
                _calculatorModel.IsNewCalculation = false;
            }
            else if (_lastInputWasOperator || _lastInputWasFunction)
            {
                _calculatorModel.DisplayValue = piValue;
                _calculatorModel.CurrentExpression += piValue;
                _displayExpression += "π";
                _lastInputWasOperator = false;
                _lastInputWasFunction = false;
            }
            else
            {
                // Если вводим константу после числа, добавляем умножение
                _calculatorModel.CurrentExpression += $" * {piValue}";
                _displayExpression += " × π";
            }

            _calculatorModel.WaitingForOperand = false;
            UpdateDisplay();
        }

        private void OnEulerClicked(object sender, EventArgs e)
        {
            string eValue = Math.E.ToString("G15");

            if (_calculatorModel.IsNewCalculation)
            {
                _calculatorModel.DisplayValue = eValue;
                _calculatorModel.CurrentExpression = eValue;
                _displayExpression = "e";
                _calculatorModel.IsNewCalculation = false;
            }
            else if (_lastInputWasOperator || _lastInputWasFunction)
            {
                _calculatorModel.DisplayValue = eValue;
                _calculatorModel.CurrentExpression += eValue;
                _displayExpression += "e";
                _lastInputWasOperator = false;
                _lastInputWasFunction = false;
            }
            else
            {
                // Если вводим константу после числа, добавляем умножение
                _calculatorModel.CurrentExpression += $" * {eValue}";
                _displayExpression += " × e";
            }

            _calculatorModel.WaitingForOperand = false;
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
                _displayExpression = "0.";
                _calculatorModel.IsNewCalculation = false;
            }
            else if (_lastInputWasOperator || _lastInputWasFunction)
            {
                _calculatorModel.DisplayValue = "0.";
                _calculatorModel.CurrentExpression += "0.";
                _displayExpression += "0.";
                _lastInputWasOperator = false;
                _lastInputWasFunction = false;
            }
            else
            {
                _calculatorModel.DisplayValue += ".";
                _calculatorModel.CurrentExpression += ".";
                _displayExpression += ".";
            }

            _calculatorModel.HasDecimalPoint = true;
            _calculatorModel.WaitingForOperand = false;
            UpdateDisplay();
        }

        private void OnOpenParenthesisClicked(object sender, EventArgs e)
        {
            if (_calculatorModel.IsNewCalculation)
            {
                _calculatorModel.CurrentExpression = "(";
                _displayExpression = "(";
                _calculatorModel.DisplayValue = "0";
                _calculatorModel.IsNewCalculation = false;
            }
            else if (_lastInputWasOperator || _lastInputWasFunction)
            {
                _calculatorModel.CurrentExpression += "(";
                _displayExpression += "(";
            }
            else
            {
                // Если вводим скобку после числа, добавляем умножение
                _calculatorModel.CurrentExpression += " * (";
                _displayExpression += " × (";
            }

            _calculatorModel.WaitingForOperand = true;
            _lastInputWasFunction = true;
            _lastInputWasOperator = false;
            UpdateDisplay();
        }

        private void OnCloseParenthesisClicked(object sender, EventArgs e)
        {
            if (!_lastInputWasOperator && !string.IsNullOrEmpty(_calculatorModel.CurrentExpression))
            {
                _calculatorModel.CurrentExpression += ")";
                _displayExpression += ")";
                _calculatorModel.WaitingForOperand = false;
                _lastInputWasFunction = false;
                _lastInputWasOperator = false;
                UpdateDisplay();
            }
        }

        private void OnEqualsClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_calculatorModel.CurrentExpression))
                    return;

                // Убираем лишние пробелы и проверяем выражение
                string expression = _calculatorModel.CurrentExpression.Trim();
                if (expression.EndsWith(" "))
                {
                    // Если выражение заканчивается оператором, убираем его
                    var parts = expression.TrimEnd().Split(' ');
                    if (parts.Length >= 2)
                    {
                        expression = string.Join(" ", parts[..^1]);
                        _calculatorModel.CurrentExpression = expression;
                    }
                }

                // Обрабатываем научные функции
                expression = ProcessScientificFunctions(expression);

                if (!_calculatorService.IsValidExpression(expression))
                {
                    ShowError("Некорректное выражение");
                    return;
                }

                double result = _calculatorService.Calculate(expression);
                string formattedResult = _calculatorService.FormatResult(result);

                string historyEntry = $"{_displayExpression.Trim()} = {formattedResult}";
                _calculatorModel.AddToHistory(historyEntry);

                _calculatorModel.LastResult = result;
                _calculatorModel.DisplayValue = formattedResult;
                _displayExpression = formattedResult;
                _calculatorModel.IsNewCalculation = true;
                _calculatorModel.HasDecimalPoint = formattedResult.Contains(".");
                _calculatorModel.WaitingForOperand = false;
                _lastInputWasOperator = false;
                _lastInputWasFunction = false;

                UpdateDisplay();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}");
            }
        }

        private void OnClearClicked(object sender, EventArgs e)
        {
            _calculatorModel.Reset();
            _displayExpression = "";
            _lastInputWasOperator = false;
            _lastInputWasFunction = false;
            UpdateDisplay();
        }

        private void OnClearEntryClicked(object sender, EventArgs e)
        {
            if (_lastInputWasOperator)
            {
                // Удаляем последний оператор
                var parts = _calculatorModel.CurrentExpression.TrimEnd().Split(' ');
                if (parts.Length >= 2)
                {
                    _calculatorModel.CurrentExpression = string.Join(" ", parts[..^1]);
                }

                var displayParts = _displayExpression.TrimEnd().Split(' ');
                if (displayParts.Length >= 2)
                {
                    _displayExpression = string.Join(" ", displayParts[..^1]);
                }

                _lastInputWasOperator = false;
            }
            else
            {
                // Удаляем последнее число или сбрасываем к 0
                _calculatorModel.DisplayValue = "0";
                _calculatorModel.HasDecimalPoint = false;
            }

            UpdateDisplay();
        }

        private string ProcessScientificFunctions(string expression)
        {
            // Заменяем научные функции на их математические эквиваленты
            try
            {
                // Обрабатываем sin, cos, tan, ln функции
                expression = ProcessTrigFunctions(expression, "sin");
                expression = ProcessTrigFunctions(expression, "cos");
                expression = ProcessTrigFunctions(expression, "tan");
                expression = ProcessLogFunction(expression);
                expression = ProcessSqrtFunction(expression);

                return expression;
            }
            catch
            {
                return expression; // Возвращаем исходное выражение если есть ошибки
            }
        }

        private string ProcessTrigFunctions(string expression, string function)
        {
            // Находим и вычисляем тригонометрические функции
            int index = 0;
            while ((index = expression.IndexOf($"{function}(", index)) != -1)
            {
                int openParen = index + function.Length;
                int closeParen = FindMatchingParen(expression, openParen);

                if (closeParen != -1)
                {
                    string innerExpression = expression.Substring(openParen + 1, closeParen - openParen - 1);

                    // Вычисляем внутреннее выражение
                    if (double.TryParse(innerExpression, out double value))
                    {
                        double result = function switch
                        {
                            "sin" => _calculatorService.CalculateSin(value, _isRadianMode),
                            "cos" => _calculatorService.CalculateCos(value, _isRadianMode),
                            "tan" => _calculatorService.CalculateTan(value, _isRadianMode),
                            _ => value
                        };

                        string replacement = result.ToString("G15");
                        expression = expression.Substring(0, index) + replacement + expression.Substring(closeParen + 1);
                        index = index + replacement.Length;
                    }
                    else
                    {
                        index = closeParen + 1;
                    }
                }
                else
                {
                    break;
                }
            }

            return expression;
        }

        private string ProcessLogFunction(string expression)
        {
            // Обрабатываем ln функцию
            int index = 0;
            while ((index = expression.IndexOf("ln(", index)) != -1)
            {
                int openParen = index + 2;
                int closeParen = FindMatchingParen(expression, openParen);

                if (closeParen != -1)
                {
                    string innerExpression = expression.Substring(openParen + 1, closeParen - openParen - 1);

                    if (double.TryParse(innerExpression, out double value))
                    {
                        double result = _calculatorService.CalculateNaturalLog(value);
                        string replacement = result.ToString("G15");
                        expression = expression.Substring(0, index) + replacement + expression.Substring(closeParen + 1);
                        index = index + replacement.Length;
                    }
                    else
                    {
                        index = closeParen + 1;
                    }
                }
                else
                {
                    break;
                }
            }

            return expression;
        }

        private string ProcessSqrtFunction(string expression)
        {
            // Обрабатываем sqrt функцию
            int index = 0;
            while ((index = expression.IndexOf("sqrt(", index)) != -1)
            {
                int openParen = index + 4;
                int closeParen = FindMatchingParen(expression, openParen);

                if (closeParen != -1)
                {
                    string innerExpression = expression.Substring(openParen + 1, closeParen - openParen - 1);

                    if (double.TryParse(innerExpression, out double value))
                    {
                        double result = _calculatorService.CalculateSquareRoot(value);
                        string replacement = result.ToString("G15");
                        expression = expression.Substring(0, index) + replacement + expression.Substring(closeParen + 1);
                        index = index + replacement.Length;
                    }
                    else
                    {
                        index = closeParen + 1;
                    }
                }
                else
                {
                    break;
                }
            }

            return expression;
        }

        private int FindMatchingParen(string expression, int openIndex)
        {
            int count = 1;
            for (int i = openIndex + 1; i < expression.Length; i++)
            {
                if (expression[i] == '(') count++;
                else if (expression[i] == ')') count--;

                if (count == 0) return i;
            }
            return -1;
        }

        private void UpdateDisplay()
        {
            try
            {
                // Находим элементы по имени для безопасного доступа
                var displayLabel = this.FindByName<Label>("DisplayLabel");
                var expressionLabel = this.FindByName<Label>("ExpressionLabel");

                if (displayLabel != null)
                    displayLabel.Text = _calculatorModel.DisplayValue;

                if (expressionLabel != null)
                    expressionLabel.Text = string.IsNullOrEmpty(_displayExpression) ? "0" : _displayExpression;
            }
            catch
            {
                // Игнорируем ошибки если элементы еще не готовы
            }
        }

        private void ShowError(string message)
        {
            try
            {
                var displayLabel = this.FindByName<Label>("DisplayLabel");
                var expressionLabel = this.FindByName<Label>("ExpressionLabel");

                if (displayLabel != null)
                    displayLabel.Text = "Ошибка";

                if (expressionLabel != null)
                    expressionLabel.Text = message;
            }
            catch
            {
                // Игнорируем ошибки если элементы еще не готовы
            }

            _calculatorModel.Reset();
            _displayExpression = "";
            _lastInputWasOperator = false;
            _lastInputWasFunction = false;
        }
    }
}