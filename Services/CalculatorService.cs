using System;
using System.Globalization;
using System.Text.RegularExpressions;
using MathNet.Numerics;
using StudyMateProject.Services;

namespace StudyMateProject.Services
{
    public class CalculatorService : ICalculatorService
    {
        public double Calculate(string expression)
        {
            try
            {
                // Заменяем символы для корректного парсинга
                expression = PrepareExpression(expression);

                // Простой парсер выражений (можно расширить)
                var result = EvaluateExpression(expression);

                // Проверяем результат на корректность
                if (double.IsNaN(result) || double.IsInfinity(result))
                {
                    throw new InvalidOperationException("Некорректный результат вычисления");
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка вычисления: {ex.Message}");
            }
        }

        public string FormatResult(double result)
        {
            // Если результат - целое число, показываем без десятичной части
            if (result == Math.Floor(result) && result <= long.MaxValue && result >= long.MinValue)
            {
                return result.ToString("0", CultureInfo.InvariantCulture);
            }

            // Для дробных чисел ограничиваем количество знаков после запятой
            string formatted = result.ToString("G15", CultureInfo.InvariantCulture);

            // Если число очень большое или очень маленькое, используем научную нотацию
            if (Math.Abs(result) >= 1e10 || (Math.Abs(result) <= 1e-6 && result != 0))
            {
                return result.ToString("E2", CultureInfo.InvariantCulture);
            }

            return formatted;
        }

        public bool IsValidExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            try
            {
                // Проверяем наличие недопустимых символов
                var validPattern = @"^[0-9+\-*/().√^ \t]+$";
                if (!Regex.IsMatch(expression, validPattern))
                    return false;

                // Проверяем парность скобок
                int openBrackets = 0;
                foreach (char c in expression)
                {
                    if (c == '(') openBrackets++;
                    if (c == ')') openBrackets--;
                    if (openBrackets < 0) return false;
                }

                return openBrackets == 0;
            }
            catch
            {
                return false;
            }
        }

        public double CalculateSquareRoot(double value)
        {
            if (value < 0)
                throw new ArgumentException("Невозможно извлечь корень из отрицательного числа");

            return Math.Sqrt(value);
        }

        public double CalculatePower(double baseValue, double exponent)
        {
            return Math.Pow(baseValue, exponent);
        }

        public double CalculatePercentage(double value, double percentage)
        {
            return value * percentage / 100;
        }

        private string PrepareExpression(string expression)
        {
            // Заменяем символы для парсинга
            expression = expression.Replace("√", "sqrt");
            expression = expression.Replace("^", "**"); // для возведения в степень
            expression = expression.Replace(",", "."); // для десятичных чисел

            return expression;
        }

        private double EvaluateExpression(string expression)
        {
            // Простой калькулятор выражений
            // Для базовой версии используем встроенные возможности .NET

            // Удаляем пробелы
            expression = expression.Replace(" ", "");

            // Для простых выражений используем рекурсивный парсер
            return ParseExpression(expression);
        }

        private double ParseExpression(string expression)
        {
            // Простой парсер для базовых операций
            // Обрабатываем скобки
            while (expression.Contains("("))
            {
                int start = expression.LastIndexOf('(');
                int end = expression.IndexOf(')', start);

                if (end == -1)
                    throw new ArgumentException("Несбалансированные скобки");

                string subExpression = expression.Substring(start + 1, end - start - 1);
                double subResult = ParseExpression(subExpression);

                expression = expression.Substring(0, start) +
                           subResult.ToString(CultureInfo.InvariantCulture) +
                           expression.Substring(end + 1);
            }

            // Обрабатываем функции (sqrt)
            expression = ProcessFunctions(expression);

            // Обрабатываем операции по приоритету
            return EvaluateSimpleExpression(expression);
        }

        private string ProcessFunctions(string expression)
        {
            // Обработка sqrt
            while (expression.Contains("sqrt"))
            {
                int sqrtIndex = expression.IndexOf("sqrt");
                int numStart = sqrtIndex + 4;

                // Находим число после sqrt
                string number = "";
                for (int i = numStart; i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.'); i++)
                {
                    number += expression[i];
                }

                if (!double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                    throw new ArgumentException("Некорректное число для sqrt");

                double result = CalculateSquareRoot(value);
                expression = expression.Replace($"sqrt{number}", result.ToString(CultureInfo.InvariantCulture));
            }

            return expression;
        }

        private double EvaluateSimpleExpression(string expression)
        {
            // Простая реализация для базовых операций +, -, *, /
            // В реальном проекте лучше использовать более сложный парсер

            try
            {
                // Используем встроенные возможности для простых выражений
                var dataTable = new System.Data.DataTable();
                var result = dataTable.Compute(expression, null);
                return Convert.ToDouble(result);
            }
            catch
            {
                // Fallback для простых случаев
                if (double.TryParse(expression, NumberStyles.Float, CultureInfo.InvariantCulture, out double simpleResult))
                    return simpleResult;

                throw new ArgumentException("Невозможно вычислить выражение");
            }
        }
    }
}