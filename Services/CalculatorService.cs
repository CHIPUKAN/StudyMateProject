using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StudyMateProject.Services
{
    public class CalculatorService : ICalculatorService
    {
        public double Calculate(string expression)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(expression))
                    return 0;

                // Подготавливаем выражение
                expression = PrepareExpression(expression);

                // Обрабатываем научные функции
                expression = ProcessScientificFunctions(expression);
                expression = ProcessSqrtFunction(expression);
                expression = ProcessCubeRootFunction(expression);
                expression = ProcessFactorialFunction(expression);
                expression = ProcessPowerOperations(expression);

                // Используем DataTable для вычисления базовых операций
                var table = new DataTable();
                var result = table.Compute(expression, null);

                if (result == DBNull.Value)
                    throw new InvalidOperationException("Невозможно вычислить выражение");

                double calculatedResult = Convert.ToDouble(result);

                if (double.IsNaN(calculatedResult) || double.IsInfinity(calculatedResult))
                    throw new InvalidOperationException("Результат вычисления некорректен");

                return calculatedResult;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка вычисления: {ex.Message}");
            }
        }

        public bool IsValidExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            try
            {
                string prepared = PrepareExpression(expression);

                // Проверяем балансировку скобок
                int openParens = 0;
                foreach (char c in prepared)
                {
                    if (c == '(') openParens++;
                    if (c == ')') openParens--;
                    if (openParens < 0) return false;
                }
                if (openParens != 0) return false;

                // Проверяем на недопустимые символы
                if (Regex.IsMatch(prepared, @"[^0-9+\-*/.() \^√²³sincostanlgbqrtfac]"))
                    return false;

                // Проверяем окончания
                string trimmed = prepared.Trim();
                if (Regex.IsMatch(trimmed, @"[+\-*/.^]$"))
                    return false;

                if (Regex.IsMatch(trimmed, @"^[+*/.^]"))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public string FormatResult(double result)
        {
            if (double.IsNaN(result))
                return "NaN";
            if (double.IsPositiveInfinity(result))
                return "∞";
            if (double.IsNegativeInfinity(result))
                return "-∞";
            if (result == 0)
                return "0";

            // Для очень маленьких чисел
            if (Math.Abs(result) > 0 && Math.Abs(result) < 0.000001)
                return result.ToString("E6", CultureInfo.InvariantCulture);

            // Для очень больших чисел
            if (Math.Abs(result) >= 1000000000000)
                return result.ToString("E6", CultureInfo.InvariantCulture);

            // Для целых чисел
            if (result == Math.Floor(result) && Math.Abs(result) < 1000000000)
                return result.ToString("F0", CultureInfo.InvariantCulture);

            // Для обычных чисел с ограничением десятичных знаков
            string formatted = result.ToString("F10", CultureInfo.InvariantCulture)
                                    .TrimEnd('0')
                                    .TrimEnd('.');

            return string.IsNullOrEmpty(formatted) ? "0" : formatted;
        }

        #region Basic Mathematical Functions

        public double CalculateSquareRoot(double value)
        {
            if (value < 0)
                throw new ArgumentException("Нельзя извлечь квадратный корень из отрицательного числа");
            return Math.Sqrt(value);
        }

        public double CalculatePower(double baseValue, double exponent)
        {
            var result = Math.Pow(baseValue, exponent);
            if (double.IsNaN(result) || double.IsInfinity(result))
                throw new InvalidOperationException("Результат возведения в степень некорректен");
            return result;
        }

        public double CalculateCubeRoot(double value)
        {
            if (value < 0)
                return -Math.Pow(-value, 1.0 / 3.0);
            return Math.Pow(value, 1.0 / 3.0);
        }

        public double CalculateFactorial(double value)
        {
            if (value < 0 || value != Math.Floor(value))
                throw new ArgumentException("Факториал определен только для неотрицательных целых чисел");

            if (value > 170)
                throw new ArgumentException("Слишком большое число для вычисления факториала");

            double result = 1;
            for (int i = 2; i <= value; i++)
            {
                result *= i;
            }
            return result;
        }

        #endregion

        #region Expression Processing

        private string PrepareExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return "";

            // Заменяем символы на числовые значения ТОЛЬКО при вычислении
            expression = expression.Replace("π", Math.PI.ToString("G17", CultureInfo.InvariantCulture))
                                 .Replace("e", Math.E.ToString("G17", CultureInfo.InvariantCulture));

            // Заменяем отображаемые символы на математические операторы
            expression = expression.Replace("×", "*")
                                 .Replace("÷", "/")
                                 .Replace("−", "-")
                                 .Replace(",", ".")
                                 .Replace("²", "^2")
                                 .Replace("³", "^3");

            // Убираем лишние пробелы
            expression = Regex.Replace(expression, @"\s+", "");

            return expression;
        }

        private string ProcessScientificFunctions(string expression)
        {
            // Обрабатываем тригонометрические функции (в радианах)
            expression = ProcessFunction(expression, "sin", x => Math.Sin(x));
            expression = ProcessFunction(expression, "cos", x => Math.Cos(x));
            expression = ProcessFunction(expression, "tan", x => Math.Tan(x));
            expression = ProcessFunction(expression, "cot", x => {
                double tanValue = Math.Tan(x);
                if (Math.Abs(tanValue) < 1e-10)
                    throw new ArgumentException("Котангенс не определен для этого значения");
                return 1.0 / tanValue;
            });

            // Логарифмические функции
            expression = ProcessFunction(expression, "ln", x => {
                if (x <= 0) throw new ArgumentException("Логарифм определен только для положительных чисел");
                return Math.Log(x);
            });

            expression = ProcessFunction(expression, "log", x => {
                if (x <= 0) throw new ArgumentException("Логарифм определен только для положительных чисел");
                return Math.Log10(x);
            });

            return expression;
        }

        private string ProcessFunction(string expression, string functionName, Func<double, double> function)
        {
            int iterations = 0;
            while (expression.Contains($"{functionName}(") && iterations < 20)
            {
                // Ищем самое внутреннее выражение функции (справа налево)
                int funcIndex = expression.LastIndexOf($"{functionName}(");
                if (funcIndex == -1) break;

                // Находим соответствующую закрывающую скобку
                int openParens = 0;
                int closeIndex = -1;

                for (int i = funcIndex + functionName.Length + 1; i < expression.Length; i++)
                {
                    if (expression[i] == '(')
                        openParens++;
                    else if (expression[i] == ')')
                    {
                        if (openParens == 0)
                        {
                            closeIndex = i;
                            break;
                        }
                        openParens--;
                    }
                }

                if (closeIndex == -1) break;

                // Извлекаем выражение внутри функции
                string innerExpression = expression.Substring(funcIndex + functionName.Length + 1,
                                                             closeIndex - funcIndex - functionName.Length - 1);

                try
                {
                    double value;

                    // Рекурсивно обрабатываем сложные выражения
                    if (innerExpression.Contains("sin") || innerExpression.Contains("cos") ||
                        innerExpression.Contains("tan") || innerExpression.Contains("cot") ||
                        innerExpression.Contains("ln") || innerExpression.Contains("log") ||
                        innerExpression.Contains("√") || innerExpression.Contains("∛") ||
                        innerExpression.Contains("fact") || innerExpression.Contains("^"))
                    {
                        value = Calculate(innerExpression);
                    }
                    else
                    {
                        // Простое числовое выражение
                        var table = new DataTable();
                        var innerResult = table.Compute(innerExpression, null);
                        value = Convert.ToDouble(innerResult);
                    }

                    double result = function(value);
                    string resultStr = result.ToString("G15", CultureInfo.InvariantCulture);

                    // Заменяем функция(выражение) на результат
                    expression = expression.Substring(0, funcIndex) + resultStr + expression.Substring(closeIndex + 1);
                }
                catch
                {
                    break;
                }

                iterations++;
            }

            return expression;
        }

        private string ProcessSqrtFunction(string expression)
        {
            int iterations = 0;
            while (expression.Contains("√(") && iterations < 20)
            {
                int sqrtIndex = expression.LastIndexOf("√(");
                if (sqrtIndex == -1) break;

                // Находим соответствующую закрывающую скобку
                int openParens = 0;
                int closeIndex = -1;

                for (int i = sqrtIndex + 2; i < expression.Length; i++)
                {
                    if (expression[i] == '(')
                        openParens++;
                    else if (expression[i] == ')')
                    {
                        if (openParens == 0)
                        {
                            closeIndex = i;
                            break;
                        }
                        openParens--;
                    }
                }

                if (closeIndex == -1) break;

                string innerExpression = expression.Substring(sqrtIndex + 2, closeIndex - sqrtIndex - 2);

                try
                {
                    double value;

                    if (innerExpression.Contains("sin") || innerExpression.Contains("cos") ||
                        innerExpression.Contains("tan") || innerExpression.Contains("cot") ||
                        innerExpression.Contains("ln") || innerExpression.Contains("log") ||
                        innerExpression.Contains("√") || innerExpression.Contains("∛") ||
                        innerExpression.Contains("fact") || innerExpression.Contains("^"))
                    {
                        value = Calculate(innerExpression);
                    }
                    else
                    {
                        var table = new DataTable();
                        var innerResult = table.Compute(innerExpression, null);
                        value = Convert.ToDouble(innerResult);
                    }

                    if (value < 0)
                        throw new ArgumentException("Нельзя извлечь корень из отрицательного числа");

                    double sqrtResult = Math.Sqrt(value);
                    string resultStr = sqrtResult.ToString("G15", CultureInfo.InvariantCulture);

                    expression = expression.Substring(0, sqrtIndex) + resultStr + expression.Substring(closeIndex + 1);
                }
                catch
                {
                    break;
                }

                iterations++;
            }

            return expression;
        }

        private string ProcessCubeRootFunction(string expression)
        {
            int iterations = 0;
            while (expression.Contains("∛(") && iterations < 20)
            {
                int cbrtIndex = expression.LastIndexOf("∛(");
                if (cbrtIndex == -1) break;

                // Находим соответствующую закрывающую скобку
                int openParens = 0;
                int closeIndex = -1;

                for (int i = cbrtIndex + 2; i < expression.Length; i++)
                {
                    if (expression[i] == '(')
                        openParens++;
                    else if (expression[i] == ')')
                    {
                        if (openParens == 0)
                        {
                            closeIndex = i;
                            break;
                        }
                        openParens--;
                    }
                }

                if (closeIndex == -1) break;

                string innerExpression = expression.Substring(cbrtIndex + 2, closeIndex - cbrtIndex - 2);

                try
                {
                    double value;

                    if (innerExpression.Contains("sin") || innerExpression.Contains("cos") ||
                        innerExpression.Contains("tan") || innerExpression.Contains("cot") ||
                        innerExpression.Contains("ln") || innerExpression.Contains("log") ||
                        innerExpression.Contains("√") || innerExpression.Contains("∛") ||
                        innerExpression.Contains("fact") || innerExpression.Contains("^"))
                    {
                        value = Calculate(innerExpression);
                    }
                    else
                    {
                        var table = new DataTable();
                        var innerResult = table.Compute(innerExpression, null);
                        value = Convert.ToDouble(innerResult);
                    }

                    double cbrtResult = CalculateCubeRoot(value);
                    string resultStr = cbrtResult.ToString("G15", CultureInfo.InvariantCulture);

                    expression = expression.Substring(0, cbrtIndex) + resultStr + expression.Substring(closeIndex + 1);
                }
                catch
                {
                    break;
                }

                iterations++;
            }

            return expression;
        }

        private string ProcessFactorialFunction(string expression)
        {
            int iterations = 0;
            while (expression.Contains("fact(") && iterations < 20)
            {
                int factIndex = expression.LastIndexOf("fact(");
                if (factIndex == -1) break;

                // Находим соответствующую закрывающую скобку
                int openParens = 0;
                int closeIndex = -1;

                for (int i = factIndex + 5; i < expression.Length; i++)
                {
                    if (expression[i] == '(')
                        openParens++;
                    else if (expression[i] == ')')
                    {
                        if (openParens == 0)
                        {
                            closeIndex = i;
                            break;
                        }
                        openParens--;
                    }
                }

                if (closeIndex == -1) break;

                string innerExpression = expression.Substring(factIndex + 5, closeIndex - factIndex - 5);

                try
                {
                    double value;

                    if (innerExpression.Contains("sin") || innerExpression.Contains("cos") ||
                        innerExpression.Contains("tan") || innerExpression.Contains("cot") ||
                        innerExpression.Contains("ln") || innerExpression.Contains("log") ||
                        innerExpression.Contains("√") || innerExpression.Contains("∛") ||
                        innerExpression.Contains("fact") || innerExpression.Contains("^"))
                    {
                        value = Calculate(innerExpression);
                    }
                    else
                    {
                        var table = new DataTable();
                        var innerResult = table.Compute(innerExpression, null);
                        value = Convert.ToDouble(innerResult);
                    }

                    double factResult = CalculateFactorial(value);
                    string resultStr = factResult.ToString("G15", CultureInfo.InvariantCulture);

                    expression = expression.Substring(0, factIndex) + resultStr + expression.Substring(closeIndex + 1);
                }
                catch
                {
                    break;
                }

                iterations++;
            }

            return expression;
        }

        private string ProcessPowerOperations(string expression)
        {
            // Обрабатываем операции возведения в степень справа налево
            string pattern = @"(\d+(?:\.\d+)?(?:[Ee][+-]?\d+)?)\^(\d+(?:\.\d+)?(?:[Ee][+-]?\d+)?)";

            int iterations = 0;
            while (Regex.IsMatch(expression, pattern) && iterations < 20)
            {
                var matches = Regex.Matches(expression, pattern);
                var lastMatch = matches[matches.Count - 1];

                string baseValue = lastMatch.Groups[1].Value;
                string exponent = lastMatch.Groups[2].Value;

                if (double.TryParse(baseValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double baseNum) &&
                    double.TryParse(exponent, NumberStyles.Float, CultureInfo.InvariantCulture, out double expNum))
                {
                    try
                    {
                        double result = Math.Pow(baseNum, expNum);
                        if (double.IsNaN(result) || double.IsInfinity(result))
                            throw new InvalidOperationException("Результат степени некорректен");

                        string resultStr = result.ToString("G15", CultureInfo.InvariantCulture);
                        expression = expression.Substring(0, lastMatch.Index) + resultStr +
                                   expression.Substring(lastMatch.Index + lastMatch.Length);
                    }
                    catch
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }

                iterations++;
            }

            return expression;
        }

        #endregion
    }
}