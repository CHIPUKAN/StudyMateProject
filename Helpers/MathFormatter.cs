using System;
using System.Globalization;
using System.Text;
using StudyMateProject.Models;
using StudyMateProject.Models;

namespace StudyMateProject.Helpers
{
    public static class MathFormatter
    {
        public static string FormatNumber(double number, int decimalPlaces = CalculatorConstants.DEFAULT_DECIMAL_PLACES)
        {
            if (double.IsNaN(number))
                return "NaN";
            if (double.IsPositiveInfinity(number))
                return CalculatorConstants.INFINITY_SYMBOL;
            if (double.IsNegativeInfinity(number))
                return "-" + CalculatorConstants.INFINITY_SYMBOL;

            // Очень большие или маленькие числа - научная нотация 
            if (Math.Abs(number) >= 1e15 || (Math.Abs(number) <= 1e-6 && number != 0))
            {
                return number.ToString($"E{Math.Min(decimalPlaces, 6)}", CultureInfo.InvariantCulture);
            }

            // Обычное форматирование
            var result = number.ToString($"F{decimalPlaces}", CultureInfo.InvariantCulture);

            // Убираем лишние нули в конце
            if (result.Contains("."))
            {
                result = result.TrimEnd('0').TrimEnd('.');
            }

            return result;
        }

        // Форматирует число в научной нотации
        public static string FormatScientific(double number, int decimalPlaces = 6)
        {
            if (double.IsNaN(number) || double.IsInfinity(number))
                return FormatNumber(number);
            return number.ToString($"E{decimalPlaces}", CultureInfo.InvariantCulture);
        }

        // Форматирует число в научной нотации
        public static string FormatPercentage(double number, int decimalPlaces = 6)
        {
            if (double.IsNaN(number) || double.IsInfinity(number))
                return FormatNumber(number);
            return (number * 100).ToString($"F{decimalPlaces}") + "%";
        }

        // Форматирует комплексное число
        public static string FormatComplexNumber(double real, double imaginary, int decimalPlaces = 4)
        {
            var realStr = FormatNumber(real, decimalPlaces);
            var imagStr = FormatNumber(Math.Abs(imaginary), decimalPlaces);

            if (imaginary == 0)
                return realStr;

            if (real == 0)
                return imaginary == 1 ? "i" : imaginary == -1 ? "-i" : $"{FormatNumber(imaginary, decimalPlaces)}i";

            var sign = imaginary >= 0 ? "+" : "-";
            var imagPart = Math.Abs(imaginary) == 1 ? "i" : $"{imagStr}i";

            return $"{realStr} {sign} {imagPart}";
        }

        // Форматирует матрицу для отображения
        public static string FormatMatrix(MatrixModel matrix, int decimalPlaces = CalculatorConstants.MATRIX_DECIMAL_PLACES)
        {
            if (matrix == null)
                return "null";

            var sb = new StringBuilder();
            var maxWidth = 0;

            // Находим максимальную ширину элемента для выравнивания
            for (int i = 0; i < matrix.Rows; i++)
            {
                for (int j = 0; j < matrix.Columns; j++)
                {
                    var formatted = FormatNumber(matrix[i, j], decimalPlaces);
                    maxWidth = Math.Max(maxWidth, formatted.Length);
                }
            }

            // Формируем строки матрицы
            for (int i = 0; i < matrix.Rows; i++)
            {
                sb.Append("│");
                for (int j = 0; j < matrix.Columns; j++)
                {
                    var formatted = FormatNumber(matrix[i, j], decimalPlaces);
                    sb.Append(formatted.PadLeft(maxWidth));
                    if (j < matrix.Columns - 1)
                        sb.Append("  ");
                }
                sb.AppendLine("│");
            }

            return sb.ToString();
        }

        // Форматирует интеграл для отображения
        public static string FormatIntegral(IntegralCalculation integral)
        {
            var lowerLimit = FormatNumber(integral.LowerLimit, 2);
            var upperLimit = FormatNumber(integral.UpperLimit, 2);
            var result = FormatNumber(integral.Result, 6);

            return $"∫[{lowerLimit}→{upperLimit}] {integral.Function} dx = {result}";
        }

        // Форматирует предел для отображения
        public static string FormatLimit(LimitCalculation limit)
        {
            var approach = limit.IsInfinity ? CalculatorConstants.INFINITY_SYMBOL : FormatNumber(limit.ApproachingValue, 2);
            var result = limit.IsResultInfinity ? CalculatorConstants.INFINITY_SYMBOL : FormatNumber(limit.Result, 6);

            var approachSymbol = limit.ApproachType switch
            {
                ApproachType.LeftSided => "←",
                ApproachType.RightSided => "→",
                _ => "→"
            };

            return $"lim({limit.Variable}{approachSymbol}{approach}) {limit.Function} = {result}";
        }

        // Форматирует ряд для отображения
        public static string FormatSeries(SeriesCalculation series)
        {
            var endIndex = series.IsInfinite ? CalculatorConstants.INFINITY_SYMBOL : series.EndIndex.ToString();
            var result = FormatNumber(series.Result, 6);

            return $"Σ[{series.Variable}={series.StartIndex}→{endIndex}] {series.Formula} = {result}";
        }

        // Форматирует угол в зависимости от режима (градусы/радианы)
        public static string FormatAngle(double angle, bool isRadians = false)
        {
            var value = FormatNumber(angle, 4);
            var unit = isRadians ? "rad" : CalculatorConstants.DEGREES_SYMBOL;
            return $"{value} {unit}";
        }

        // Форматирует время выполнения
        public static string FormatExecutionTime(TimeSpan time)
        {
            if (time.TotalMilliseconds < 1)
                return $"{time.TotalMicroseconds:F0} μs";

            if (time.TotalSeconds < 1)
                return $"{time.TotalMilliseconds:F1} ms";

            return $"{time.TotalSeconds:F2} s";
        }

        // Форматирует размер матрицы
        public static string FormatMatrixSize(int rows, int columns)
        {
            return $"{rows}×{columns}";
        }

        // Форматирует результат для истории
        public static string FormatHistoryEntry(CalculationResult result)
        {
            var timestamp = result.Timestamp.ToString("HH:mm:ss");
            var typeIcon = result.Type switch
            {
                CalculationType.Basic => "🔢",
                CalculationType.Scientific => "🧮",
                CalculationType.Matrix => "📊",
                CalculationType.Advanced => "📈",
                CalculationType.Conversion => "🔄",
                _ => "❓"
            };

            return $"{typeIcon} {timestamp} | {result.Expression} = {result.FormattedResult}";
        }

        // Сокращает длинное выражение для отображения
        public static string TruncateExpression(string expression, int maxLength = 50)
        {
            if (string.IsNullOrEmpty(expression) || expression.Length <= maxLength)
                return expression;

            return expression.Substring(0, maxLength - 3) + "...";
        }

        // Проверяет, нужно ли использовать научную нотацию
        public static bool ShouldUseScientificNotation(double number)
        {
            return Math.Abs(number) >= 1e12 || (Math.Abs(number) <= 1e-4 && number != 0);
        }
    }
}
