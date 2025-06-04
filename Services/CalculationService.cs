using System;
using System.Threading.Tasks;
using StudyMateProject.Models;
using StudyMateProject.Helpers;
using StudyMateProject.Services;

namespace StudyMateProject.Services
{
    public class CalculationService : ICalculationService
    {
        private readonly MathExpressionParser _parser;

        public CalculationService()
        {
            _parser = new MathExpressionParser();
        }

        public double PI => Math.PI;
        public double E => Math.E;

        public async Task<CalculationResult> EvaluateAsync(string expression)
        {
            return await Task.Run(() =>
            {
                var result = new CalculationResult
                {
                    Expression = expression,
                    Type = DetermineCalculationType(expression)
                };

                try
                {
                    var value = _parser.Evaluate(expression);
                    result.Result = value;
                    result.FormattedResult = MathFormatter.FormatNumber(value);
                }
                catch (Exception ex)
                {
                    result.HasError = true;
                    result.ErrorMessage = ex.Message;
                    result.FormattedResult = CalculatorConstants.ERROR_PREFIX + ex.Message;
                }

                return result;
            });
        }

        // Базовые арифметические операции
        public CalculationResult Add(double a, double b)
        {
            var result = a + b;
            return CreateResult($"{a} + {b}", result, CalculationType.Basic);
        }

        public CalculationResult Subtract(double a, double b)
        {
            var result = a - b;
            return CreateResult($"{a} - {b}", result, CalculationType.Basic);
        }

        public CalculationResult Multiply(double a, double b)
        {
            var result = a * b;
            return CreateResult($"{a} × {b}", result, CalculationType.Basic);
        }

        public CalculationResult Divide(double a, double b)
        {
            if (Math.Abs(b) < CalculatorConstants.DEFAULT_TOLERANCE)
            {
                return CreateErrorResult($"{a} ÷ {b}", CalculatorConstants.ERROR_DIVISION_BY_ZERO);
            }

            var result = a / b;
            return CreateResult($"{a} ÷ {b}", result, CalculationType.Basic);
        }

        public CalculationResult Power(double baseValue, double exponent)
        {
            try
            {
                var result = Math.Pow(baseValue, exponent);

                if (double.IsNaN(result))
                {
                    return CreateErrorResult($"{baseValue}^{exponent}", CalculatorConstants.ERROR_DOMAIN_ERROR);
                }

                return CreateResult($"{baseValue}^{exponent}", result, CalculationType.Scientific);
            }
            catch (Exception ex)
            {
                return CreateErrorResult($"{baseValue}^{exponent}", ex.Message);
            }
        }

        public CalculationResult SquareRoot(double value)
        {
            if (value < 0)
            {
                return CreateErrorResult($"√{value}", CalculatorConstants.ERROR_DOMAIN_ERROR);
            }

            var result = Math.Sqrt(value);
            return CreateResult($"√{value}", result, CalculationType.Scientific);
        }

        public CalculationResult Percentage(double value, double percent)
        {
            var result = (value * percent) / 100;
            return CreateResult($"{percent}% of {value}", result, CalculationType.Basic);
        }

        // Тригонометрические функции
        public CalculationResult Sin(double angle, bool isRadians = false)
        {
            var radians = isRadians ? angle : DegreesToRadians(angle);
            var result = Math.Sin(radians);
            var unit = isRadians ? "rad" : "°";
            return CreateResult($"sin({angle}{unit})", result, CalculationType.Scientific);
        }

        public CalculationResult Cos(double angle, bool isRadians = false)
        {
            var radians = isRadians ? angle : DegreesToRadians(angle);
            var result = Math.Cos(radians);
            var unit = isRadians ? "rad" : "°";
            return CreateResult($"cos({angle}{unit})", result, CalculationType.Scientific);
        }

        public CalculationResult Tan(double angle, bool isRadians = false)
        {
            var radians = isRadians ? angle : DegreesToRadians(angle);
            var result = Math.Tan(radians);
            var unit = isRadians ? "rad" : "°";
            return CreateResult($"tan({angle}{unit})", result, CalculationType.Scientific);
        }

        public CalculationResult ArcSin(double value)
        {
            if (Math.Abs(value) > 1)
            {
                return CreateErrorResult($"arcsin({value})", CalculatorConstants.ERROR_DOMAIN_ERROR);
            }

            var result = RadiansToDegrees(Math.Asin(value));
            return CreateResult($"arcsin({value})", result, CalculationType.Scientific);
        }

        public CalculationResult ArcCos(double value)
        {
            if (Math.Abs(value) > 1)
            {
                return CreateErrorResult($"arccos({value})", CalculatorConstants.ERROR_DOMAIN_ERROR);
            }

            var result = RadiansToDegrees(Math.Acos(value));
            return CreateResult($"arccos({value})", result, CalculationType.Scientific);
        }

        public CalculationResult ArcTan(double value)
        {
            var result = RadiansToDegrees(Math.Atan(value));
            return CreateResult($"arctan({value})", result, CalculationType.Scientific);
        }

        // Логарифмические функции
        public CalculationResult Log(double value, double baseValue = 10)
        {
            if (value <= 0)
            {
                return CreateErrorResult($"log({value})", CalculatorConstants.ERROR_DOMAIN_ERROR);
            }

            if (baseValue <= 0 || baseValue == 1)
            {
                return CreateErrorResult($"log({value}, {baseValue})", CalculatorConstants.ERROR_DOMAIN_ERROR);
            }

            var result = Math.Log(value, baseValue);
            var expression = baseValue == 10 ? $"log({value})" : $"log({value}, {baseValue})";
            return CreateResult(expression, result, CalculationType.Scientific);
        }

        public CalculationResult Ln(double value)
        {
            if (value <= 0)
            {
                return CreateErrorResult($"ln({value})", CalculatorConstants.ERROR_DOMAIN_ERROR);
            }

            var result = Math.Log(value);
            return CreateResult($"ln({value})", result, CalculationType.Scientific);
        }

        public CalculationResult Exp(double exponent)
        {
            try
            {
                var result = Math.Exp(exponent);

                if (double.IsInfinity(result))
                {
                    return CreateErrorResult($"e^{exponent}", CalculatorConstants.ERROR_OVERFLOW);
                }

                return CreateResult($"e^{exponent}", result, CalculationType.Scientific);
            }
            catch (Exception ex)
            {
                return CreateErrorResult($"e^{exponent}", ex.Message);
            }
        }

        // Дополнительные научные функции
        public CalculationResult Factorial(double n)
        {
            if (n < 0 || n != Math.Floor(n))
            {
                return CreateErrorResult($"{n}!", "Факториал определен только для неотрицательных целых чисел");
            }

            if (n > CalculatorConstants.MAX_FACTORIAL)
            {
                return CreateErrorResult($"{n}!", CalculatorConstants.ERROR_OVERFLOW);
            }

            double result = 1;
            for (int i = 2; i <= n; i++)
            {
                result *= i;
            }

            return CreateResult($"{n}!", result, CalculationType.Scientific);
        }

        public CalculationResult Combination(int n, int r)
        {
            if (n < 0 || r < 0 || r > n)
            {
                return CreateErrorResult($"C({n},{r})", "Неверные параметры для сочетаний");
            }

            try
            {
                var nFact = Factorial(n);
                var rFact = Factorial(r);
                var nrFact = Factorial(n - r);

                if (nFact.HasError || rFact.HasError || nrFact.HasError)
                {
                    return CreateErrorResult($"C({n},{r})", CalculatorConstants.ERROR_OVERFLOW);
                }

                var result = (double)nFact.Result / ((double)rFact.Result * (double)nrFact.Result);
                return CreateResult($"C({n},{r})", result, CalculationType.Scientific);
            }
            catch (Exception ex)
            {
                return CreateErrorResult($"C({n},{r})", ex.Message);
            }
        }

        public CalculationResult Permutation(int n, int r)
        {
            if (n < 0 || r < 0 || r > n)
            {
                return CreateErrorResult($"P({n},{r})", "Неверные параметры для размещений");
            }

            try
            {
                var nFact = Factorial(n);
                var nrFact = Factorial(n - r);

                if (nFact.HasError || nrFact.HasError)
                {
                    return CreateErrorResult($"P({n},{r})", CalculatorConstants.ERROR_OVERFLOW);
                }

                var result = (double)nFact.Result / (double)nrFact.Result;
                return CreateResult($"P({n},{r})", result, CalculationType.Scientific);
            }
            catch (Exception ex)
            {
                return CreateErrorResult($"P({n},{r})", ex.Message);
            }
        }

        public CalculationResult Absolute(double value)
        {
            var result = Math.Abs(value);
            return CreateResult($"|{value}|", result, CalculationType.Basic);
        }

        // Конвертация углов
        public double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public double RadiansToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }

        // Проверка валидности выражения
        public bool IsValidExpression(string expression)
        {
            return _parser.IsValidExpression(expression);
        }

        // Форматирование числа для отображения
        public string FormatNumber(double number, int decimalPlaces = 10)
        {
            return MathFormatter.FormatNumber(number, decimalPlaces);
        }

        // Вспомогательные методы
        private CalculationResult CreateResult(string expression, double result, CalculationType type)
        {
            return new CalculationResult
            {
                Expression = expression,
                Result = result,
                FormattedResult = MathFormatter.FormatNumber(result),
                Type = type,
                HasError = false
            };
        }

        private CalculationResult CreateErrorResult(string expression, string errorMessage)
        {
            return new CalculationResult
            {
                Expression = expression,
                HasError = true,
                ErrorMessage = errorMessage,
                FormattedResult = CalculatorConstants.ERROR_PREFIX + errorMessage
            };
        }

        private CalculationType DetermineCalculationType(string expression)
        {
            var info = _parser.ParseExpressionInfo(expression);

            if (info.ContainsTrigonometry || info.ContainsLogarithms || info.ContainsRoots)
                return CalculationType.Scientific;

            if (expression.Contains("fact") || expression.Contains("!"))
                return CalculationType.Scientific;

            return CalculationType.Basic;
        }
    }
}
