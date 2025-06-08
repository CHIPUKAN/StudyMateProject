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

                // Обрабатываем функции
                expression = ProcessSqrtFunction(expression);
                expression = ProcessPowerOperations(expression);

                // Используем DataTable для вычисления
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
                // Подготавливаем выражение
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
                if (Regex.IsMatch(prepared, @"[^0-9+\-*/.() \^√qrts]"))
                    return false;

                // Проверяем, что выражение не заканчивается оператором
                string trimmed = prepared.Trim();
                if (Regex.IsMatch(trimmed, @"[+\-*/.^]$"))
                    return false;

                // Проверяем, что выражение не начинается с недопустимого оператора
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

        public double CalculateSin(double value, bool isRadianMode = true)
        {
            double radians = isRadianMode ? value : value * Math.PI / 180;
            return Math.Sin(radians);
        }

        public double CalculateCos(double value, bool isRadianMode = true)
        {
            double radians = isRadianMode ? value : value * Math.PI / 180;
            return Math.Cos(radians);
        }

        public double CalculateTan(double value, bool isRadianMode = true)
        {
            double radians = isRadianMode ? value : value * Math.PI / 180;
            return Math.Tan(radians);
        }

        public double CalculateNaturalLog(double value)
        {
            if (value <= 0)
                throw new ArgumentException("Логарифм определен только для положительных чисел");
            return Math.Log(value);
        }

        public double CalculateLog10(double value)
        {
            if (value <= 0)
                throw new ArgumentException("Логарифм определен только для положительных чисел");
            return Math.Log10(value);
        }

        private string PrepareExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return "";

            // Заменяем отображаемые символы на математические операторы
            expression = expression.Replace("×", "*")
                                 .Replace("÷", "/")
                                 .Replace("−", "-")
                                 .Replace(",", ".");

            // Убираем лишние пробелы
            expression = Regex.Replace(expression, @"\s+", "");

            return expression;
        }

        private string ProcessSqrtFunction(string expression)
        {
            // Обрабатываем функцию квадратного корня √(выражение) с учетом вложенных скобок
            int iterations = 0;
            while (expression.Contains("√(") && iterations < 10)
            {
                // Ищем самое внутреннее выражение √(...)
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

                if (closeIndex == -1) break; // Нет закрывающей скобки

                // Извлекаем выражение внутри корня
                string innerExpression = expression.Substring(sqrtIndex + 2, closeIndex - sqrtIndex - 2);

                try
                {
                    // Вычисляем внутреннее выражение
                    var table = new DataTable();
                    var innerResult = table.Compute(innerExpression, null);

                    if (innerResult != DBNull.Value)
                    {
                        double value = Convert.ToDouble(innerResult);
                        if (value < 0)
                            throw new ArgumentException("Нельзя извлечь корень из отрицательного числа");

                        double sqrtResult = Math.Sqrt(value);
                        string resultStr = sqrtResult.ToString("G15", CultureInfo.InvariantCulture);

                        // Заменяем √(выражение) на результат
                        expression = expression.Substring(0, sqrtIndex) + resultStr + expression.Substring(closeIndex + 1);
                    }
                    else
                    {
                        break; // Не удалось вычислить
                    }
                }
                catch
                {
                    // Если не удалось вычислить, пробуем как простое число
                    if (double.TryParse(innerExpression, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                    {
                        if (value < 0)
                            throw new ArgumentException("Нельзя извлечь корень из отрицательного числа");

                        double sqrtResult = Math.Sqrt(value);
                        string resultStr = sqrtResult.ToString("G15", CultureInfo.InvariantCulture);

                        // Заменяем √(число) на результат
                        expression = expression.Substring(0, sqrtIndex) + resultStr + expression.Substring(closeIndex + 1);
                    }
                    else
                    {
                        break; // Не удалось обработать
                    }
                }

                iterations++;
            }

            return expression;
        }

        private string ProcessPowerOperations(string expression)
        {
            // Обрабатываем операции возведения в степень
            string pattern = @"(\d+(?:\.\d+)?(?:[Ee][+-]?\d+)?)\^(\d+(?:\.\d+)?(?:[Ee][+-]?\d+)?)";

            int iterations = 0;
            while (Regex.IsMatch(expression, pattern) && iterations < 10)
            {
                expression = Regex.Replace(expression, pattern, match =>
                {
                    string baseValue = match.Groups[1].Value;
                    string exponent = match.Groups[2].Value;

                    if (double.TryParse(baseValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double baseNum) &&
                        double.TryParse(exponent, NumberStyles.Float, CultureInfo.InvariantCulture, out double expNum))
                    {
                        try
                        {
                            double result = Math.Pow(baseNum, expNum);
                            return result.ToString("G15", CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                            return match.Value;
                        }
                    }

                    return match.Value;
                });

                iterations++;
            }

            return expression;
        }
    }
}