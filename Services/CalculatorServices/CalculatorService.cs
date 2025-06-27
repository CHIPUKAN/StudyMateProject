using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StudyMateTest.Services.CalculatorServices
{
    public class CalculatorService : ICalculatorService
    {
        public double Calculate(string expression)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(expression))
                    throw new InvalidOperationException("Пустое выражение");

                expression = PrepareExpression(expression);
                expression = ProcessScientificFunctions(expression);
                expression = ProcessSqrtFunction(expression);
                expression = ProcessFactorialFunction(expression);
                expression = ProcessPowerOperations(expression);

                if (expression.Contains("ERROR_VALUE"))
                    return double.NaN;

                var table = new DataTable();
                var result = table.Compute(expression, null);

                if (result == DBNull.Value)
                    return double.NaN;

                double calculatedResult = Convert.ToDouble(result);

                if (double.IsNaN(calculatedResult) || double.IsInfinity(calculatedResult))
                    return double.NaN;

                return calculatedResult;
            }
            catch
            {
                return double.NaN;
            }
        }

        public bool IsValidExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            try
            {
                if (HasMultipleMinuses(expression))
                    return false;

                string prepared = PrepareExpression(expression);

                int openParens = 0;
                foreach (char c in prepared)
                {
                    if (c == '(') openParens++;
                    if (c == ')') openParens--;
                    if (openParens < 0) return false;
                }
                if (openParens != 0) return false;

                if (Regex.IsMatch(prepared, @"[^0-9+\-*/.() \^√²³sincostanlgbqrtfac]"))
                    return false;

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

        private bool HasMultipleMinuses(string expression)
        {
            string temp = Regex.Replace(expression, @"\(\s*−\s*\d+(?:\.\d+)?\s*\)", "NEGATIVE_NUMBER");

            if (Regex.IsMatch(temp, @"−\s*−"))
                return true;

            if (Regex.IsMatch(temp, @"[+×÷]\s*[+×÷]"))
                return true;

            return false;
        }

        public string FormatResult(double result)
        {
            if (double.IsNaN(result))
                return "Ошибка";
            if (double.IsPositiveInfinity(result))
                return "Ошибка";
            if (double.IsNegativeInfinity(result))
                return "Ошибка";
            if (result == 0)
                return "0";

            if (Math.Abs(result) > 0 && Math.Abs(result) < 0.000001)
                return result.ToString("E6", CultureInfo.InvariantCulture);

            if (Math.Abs(result) >= 1000000000000)
                return result.ToString("E6", CultureInfo.InvariantCulture);

            if (result == Math.Floor(result) && Math.Abs(result) < 1000000000)
                return result.ToString("F0", CultureInfo.InvariantCulture);

            string formatted = result.ToString("F10", CultureInfo.InvariantCulture)
                                    .TrimEnd('0')
                                    .TrimEnd('.');

            return string.IsNullOrEmpty(formatted) ? "0" : formatted;
        }

        #region Basic Mathematical Functions

        public double CalculateSquareRoot(double value)
        {
            if (value < 0)
                return double.NaN;
            return Math.Sqrt(value);
        }

        public double CalculatePower(double baseValue, double exponent)
        {
            var result = Math.Pow(baseValue, exponent);
            if (double.IsNaN(result) || double.IsInfinity(result))
                return double.NaN;
            return result;
        }

        public double CalculateCubeRoot(double value)
        {
            try
            {
                if (value == 0)
                    return 0;

                if (value > 0)
                {
                    return Math.Pow(value, 1.0 / 3.0);
                }
                else
                {
                    return -Math.Pow(-value, 1.0 / 3.0);
                }
            }
            catch
            {
                return double.NaN;
            }
        }

        public double CalculateFactorial(double value)
        {
            if (value < 0) return double.NaN;
            if (value != Math.Floor(value)) return double.NaN;
            if (value > 170) return double.NaN;

            if (value == 0 || value == 1)
                return 1;

            double result = 1;
            for (int i = 2; i <= value; i++)
            {
                result *= i;
                if (double.IsInfinity(result))
                    return double.NaN;
            }
            return result;
        }

        #endregion

        #region Expression Processing

        private string PrepareExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return "";

            expression = expression.Replace("∛(", "cbrt(");

            expression = expression.Replace("π", Math.PI.ToString("G17", CultureInfo.InvariantCulture))
                                 .Replace("e", Math.E.ToString("G17", CultureInfo.InvariantCulture));

            expression = expression.Replace("×", "*")
                                 .Replace("÷", "/")
                                 .Replace("−", "-")
                                 .Replace(",", ".")
                                 .Replace("²", "^2")
                                 .Replace("³", "^3");

            expression = Regex.Replace(expression, @"\s+", "");

            return expression;
        }

        private string ProcessScientificFunctions(string expression)
        {
            try
            {
                expression = ProcessFunction(expression, "sin", x => {
                    double result = SmartSin(x);
                    if (double.IsNaN(result) || double.IsInfinity(result))
                        return double.NaN;
                    return result;
                });

                expression = ProcessFunction(expression, "cos", x => {
                    double result = SmartCos(x);
                    if (double.IsNaN(result) || double.IsInfinity(result))
                        return double.NaN;
                    return result;
                });

                expression = ProcessFunction(expression, "tan", x => {
                    double result = SmartTan(x);
                    if (double.IsNaN(result) || double.IsInfinity(result))
                        return double.NaN;
                    return result;
                });

                expression = ProcessFunction(expression, "cot", x => {
                    double result = SmartCot(x);
                    if (double.IsNaN(result) || double.IsInfinity(result))
                        return double.NaN;
                    return result;
                });

                expression = ProcessFunction(expression, "ln", x => {
                    if (x <= 0) return double.NaN;
                    double result = Math.Log(x);
                    if (double.IsNaN(result) || double.IsInfinity(result))
                        return double.NaN;
                    return result;
                });

                expression = ProcessFunction(expression, "log", x => {
                    if (x <= 0) return double.NaN;
                    double result = Math.Log10(x);
                    if (double.IsNaN(result) || double.IsInfinity(result))
                        return double.NaN;
                    return result;
                });

                expression = ProcessFunction(expression, "cbrt", x => {
                    if (x == 0) return 0;

                    if (x > 0)
                    {
                        return Math.Pow(x, 1.0 / 3.0);
                    }
                    else
                    {
                        return -Math.Pow(-x, 1.0 / 3.0);
                    }
                });

                return expression;
            }
            catch
            {
                return expression;
            }
        }

        #endregion

        #region Smart Trigonometric Functions

        private double SmartSin(double x)
        {
            double result = Math.Sin(x);
            return ApplyTrigonometricCorrection(x, result, TrigFunction.Sin);
        }

        private double SmartCos(double x)
        {
            double result = Math.Cos(x);
            return ApplyTrigonometricCorrection(x, result, TrigFunction.Cos);
        }

        private double SmartTan(double x)
        {
            if (IsCloseToOddMultipleOfPiHalf(x))
            {
                return double.NaN;
            }

            double sinValue = SmartSin(x);
            double cosValue = SmartCos(x);

            if (Math.Abs(cosValue) < 1e-15)
                return double.NaN;

            double result = sinValue / cosValue;

            if (Math.Abs(result) > 1e15)
                return double.NaN;

            return CleanupSmallValues(result);
        }

        private double SmartCot(double x)
        {
            if (IsCloseToMultipleOfPi(x))
            {
                return double.NaN;
            }

            double sinValue = SmartSin(x);
            double cosValue = SmartCos(x);

            if (Math.Abs(sinValue) < 1e-15)
                return double.NaN;

            double result = cosValue / sinValue;

            if (Math.Abs(result) > 1e15)
                return double.NaN;

            return CleanupSmallValues(result);
        }

        private enum TrigFunction
        {
            Sin, Cos
        }

        private double ApplyTrigonometricCorrection(double x, double result, TrigFunction function)
        {
            const double tolerance = 1e-10;

            double normalizedX = ((x % (2 * Math.PI)) + 2 * Math.PI) % (2 * Math.PI);

            if (function == TrigFunction.Sin)
            {
                if (IsCloseToMultipleOfPi(normalizedX))
                    return 0;

                if (Math.Abs(normalizedX - Math.PI / 2) < tolerance)
                    return 1;
                if (Math.Abs(normalizedX - 3 * Math.PI / 2) < tolerance)
                    return -1;
            }

            if (function == TrigFunction.Cos)
            {
                if (IsCloseToOddMultipleOfPiHalf(normalizedX))
                    return 0;

                if (Math.Abs(normalizedX) < tolerance || Math.Abs(normalizedX - 2 * Math.PI) < tolerance)
                    return 1;
                if (Math.Abs(normalizedX - Math.PI) < tolerance)
                    return -1;
            }

            return CleanupSmallValues(result);
        }

        private bool IsCloseToMultipleOfPi(double x)
        {
            const double tolerance = 1e-10;
            double quotient = x / Math.PI;
            double remainder = Math.Abs(quotient - Math.Round(quotient));
            return remainder < tolerance / Math.PI;
        }

        private bool IsCloseToOddMultipleOfPiHalf(double x)
        {
            const double tolerance = 1e-10;
            double quotient = x / (Math.PI / 2);
            double roundedQuotient = Math.Round(quotient);

            if (Math.Abs(roundedQuotient % 2) < tolerance || Math.Abs((roundedQuotient % 2) - 2) < tolerance)
                return false;

            double remainder = Math.Abs(quotient - roundedQuotient);
            return remainder < tolerance / (Math.PI / 2);
        }

        private double CleanupSmallValues(double value)
        {
            if (Math.Abs(value) < 1e-15)
                return 0;

            if (Math.Abs(Math.Abs(value) - 1) < 1e-15)
                return Math.Sign(value);

            return value;
        }

        #endregion

        #region Function Processing

        private string ProcessFunction(string expression, string functionName, Func<double, double> function)
        {
            int iterations = 0;
            while (expression.Contains($"{functionName}(") && iterations < 20)
            {
                int funcIndex = expression.LastIndexOf($"{functionName}(");
                if (funcIndex == -1) break;

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

                string innerExpression = expression.Substring(funcIndex + functionName.Length + 1,
                                                             closeIndex - funcIndex - functionName.Length - 1);

                try
                {
                    double value;

                    if (innerExpression.Contains("sin") || innerExpression.Contains("cos") ||
                        innerExpression.Contains("tan") || innerExpression.Contains("cot") ||
                        innerExpression.Contains("ln") || innerExpression.Contains("log") ||
                        innerExpression.Contains("√") || innerExpression.Contains("cbrt") ||
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

                    if (double.IsNaN(value) || double.IsInfinity(value))
                    {
                        expression = expression.Substring(0, funcIndex) + "ERROR_VALUE" + expression.Substring(closeIndex + 1);
                        break;
                    }

                    double result = function(value);

                    if (double.IsNaN(result) || double.IsInfinity(result))
                    {
                        expression = expression.Substring(0, funcIndex) + "ERROR_VALUE" + expression.Substring(closeIndex + 1);
                        break;
                    }

                    string resultStr = result.ToString("G15", CultureInfo.InvariantCulture);

                    expression = expression.Substring(0, funcIndex) + resultStr + expression.Substring(closeIndex + 1);
                }
                catch
                {
                    expression = expression.Substring(0, funcIndex) + "ERROR_VALUE" + expression.Substring(closeIndex + 1);
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
                        innerExpression.Contains("√") || innerExpression.Contains("cbrt") ||
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

                    if (value < 0 || double.IsNaN(value) || double.IsInfinity(value))
                    {
                        expression = expression.Substring(0, sqrtIndex) + "ERROR_VALUE" + expression.Substring(closeIndex + 1);
                        break;
                    }

                    double sqrtResult = Math.Sqrt(value);

                    if (double.IsNaN(sqrtResult) || double.IsInfinity(sqrtResult))
                    {
                        expression = expression.Substring(0, sqrtIndex) + "ERROR_VALUE" + expression.Substring(closeIndex + 1);
                        break;
                    }

                    string resultStr = sqrtResult.ToString("G15", CultureInfo.InvariantCulture);

                    expression = expression.Substring(0, sqrtIndex) + resultStr + expression.Substring(closeIndex + 1);
                }
                catch
                {
                    expression = expression.Substring(0, sqrtIndex) + "ERROR_VALUE" + expression.Substring(closeIndex + 1);
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
                        innerExpression.Contains("√") || innerExpression.Contains("cbrt") ||
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

                    if (double.IsNaN(value) || double.IsInfinity(value))
                    {
                        expression = expression.Substring(0, factIndex) + "ERROR_VALUE" + expression.Substring(closeIndex + 1);
                        break;
                    }

                    double factResult = CalculateFactorial(value);

                    if (double.IsNaN(factResult) || double.IsInfinity(factResult))
                    {
                        expression = expression.Substring(0, factIndex) + "ERROR_VALUE" + expression.Substring(closeIndex + 1);
                        break;
                    }

                    string resultStr = factResult.ToString("G15", CultureInfo.InvariantCulture);

                    expression = expression.Substring(0, factIndex) + resultStr + expression.Substring(closeIndex + 1);
                }
                catch
                {
                    expression = expression.Substring(0, factIndex) + "ERROR_VALUE" + expression.Substring(closeIndex + 1);
                    break;
                }

                iterations++;
            }

            return expression;
        }

        private string ProcessPowerOperations(string expression)
        {
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
                        {
                            expression = expression.Substring(0, lastMatch.Index) + "ERROR_VALUE" +
                                       expression.Substring(lastMatch.Index + lastMatch.Length);
                            break;
                        }

                        string resultStr = result.ToString("G15", CultureInfo.InvariantCulture);
                        expression = expression.Substring(0, lastMatch.Index) + resultStr +
                                   expression.Substring(lastMatch.Index + lastMatch.Length);
                    }
                    catch
                    {
                        expression = expression.Substring(0, lastMatch.Index) + "ERROR_VALUE" +
                                   expression.Substring(lastMatch.Index + lastMatch.Length);
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