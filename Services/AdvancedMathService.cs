using System;
using System.Threading.Tasks;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.Integration;
using StudyMateProject.Models;
using StudyMateProject.Helpers;

namespace StudyMateProject.Services
{
    // Сервис для продвинутых математических операций
    public class AdvancedMathService : IAdvancedMathService
    {
        private readonly MathExpressionParser _parser;

        public AdvancedMathService()
        {
            _parser = new MathExpressionParser();
        }

        // Интегралы
        public async Task<CalculationResult> IntegrateAsync(string function, double lowerLimit, double upperLimit,
            IntegrationMethod method = IntegrationMethod.Adaptive, int steps = 1000)
        {
            var calculation = new IntegralCalculation
            {
                Function = function,
                LowerLimit = lowerLimit,
                UpperLimit = upperLimit,
                Method = method,
                Steps = steps
            };

            return await IntegrateAsync(calculation);
        }

        public async Task<CalculationResult> IntegrateAsync(IntegralCalculation calculation)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!IsValidFunction(calculation.Function, "x"))
                    {
                        return CreateErrorResult($"∫ {calculation.Function} dx", CalculatorConstants.ERROR_INVALID_FUNCTION);
                    }

                    // Создаем функцию для интегрирования
                    Func<double, double> func = x =>
                    {
                        _parser.SetVariable("x", x);
                        return _parser.Evaluate(calculation.Function);
                    };

                    double result;
                    double error = 0;

                    switch (calculation.Method)
                    {
                        case IntegrationMethod.Trapezoidal:
                            result = Integrate.OnClosedInterval(func, calculation.LowerLimit,
                                calculation.UpperLimit, calculation.Steps);
                            break;

                        case IntegrationMethod.Simpson:
                            result = SimpsonRule(func, calculation.LowerLimit, calculation.UpperLimit, calculation.Steps);
                            break;

                        case IntegrationMethod.Gaussian:
                            result = GaussLegendreRule.Integrate(func, calculation.LowerLimit,
                                calculation.UpperLimit, 32);
                            break;

                        default: // Adaptive
                            result = Integrate.OnClosedInterval(func, calculation.LowerLimit, calculation.UpperLimit);
                            break;
                    }

                    calculation.Result = result;
                    calculation.Error = error;

                    return new CalculationResult
                    {
                        Expression = MathFormatter.FormatIntegral(calculation),
                        Result = calculation,
                        FormattedResult = MathFormatter.FormatNumber(result, CalculatorConstants.SCIENTIFIC_DECIMAL_PLACES),
                        Type = CalculationType.Advanced,
                        Details = $"Метод: {calculation.Method}, Шаги: {calculation.Steps}"
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult($"∫ {calculation.Function} dx", ex.Message);
                }
            });
        }

        // Пределы
        public async Task<CalculationResult> CalculateLimitAsync(string function, string variable, double approachingValue,
            ApproachType approachType = ApproachType.TwoSided)
        {
            var calculation = new LimitCalculation
            {
                Function = function,
                Variable = variable,
                ApproachingValue = approachingValue,
                ApproachType = approachType
            };

            return await CalculateLimitAsync(calculation);
        }

        public async Task<CalculationResult> CalculateLimitAsync(LimitCalculation calculation)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!IsValidFunction(calculation.Function, calculation.Variable))
                    {
                        return CreateErrorResult($"lim {calculation.Function}", CalculatorConstants.ERROR_INVALID_FUNCTION);
                    }

                    double result = CalculateNumericalLimit(calculation);

                    calculation.Result = result;
                    calculation.IsResultInfinity = double.IsInfinity(result);
                    calculation.Exists = !double.IsNaN(result);

                    return new CalculationResult
                    {
                        Expression = MathFormatter.FormatLimit(calculation),
                        Result = calculation,
                        FormattedResult = calculation.IsResultInfinity ?
                            CalculatorConstants.INFINITY_SYMBOL :
                            MathFormatter.FormatNumber(result, CalculatorConstants.SCIENTIFIC_DECIMAL_PLACES),
                        Type = CalculationType.Advanced
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult($"lim {calculation.Function}", ex.Message);
                }
            });
        }

        public async Task<CalculationResult> CalculateLimitAtInfinityAsync(string function, string variable,
            bool positiveInfinity = true)
        {
            var calculation = new LimitCalculation
            {
                Function = function,
                Variable = variable,
                IsInfinity = true,
                ApproachingValue = positiveInfinity ? double.PositiveInfinity : double.NegativeInfinity
            };

            return await CalculateLimitAsync(calculation);
        }

        // Суммы рядов
        public async Task<CalculationResult> CalculateSeriesAsync(string formula, string variable, int startIndex,
            int? endIndex = null, int maxTerms = 10000)
        {
            var calculation = new SeriesCalculation
            {
                Formula = formula,
                Variable = variable,
                StartIndex = startIndex,
                EndIndex = endIndex
            };

            return await CalculateSeriesAsync(calculation);
        }

        public async Task<CalculationResult> CalculateSeriesAsync(SeriesCalculation calculation)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!IsValidFunction(calculation.Formula, calculation.Variable))
                    {
                        return CreateErrorResult($"Σ {calculation.Formula}", CalculatorConstants.ERROR_INVALID_FUNCTION);
                    }

                    double sum = 0;
                    int computedTerms = 0;
                    double previousSum = 0;
                    bool converged = false;

                    int endIndex = calculation.EndIndex ?? (calculation.StartIndex + CalculatorConstants.MAX_SERIES_TERMS);

                    for (int n = calculation.StartIndex; n <= endIndex && computedTerms < CalculatorConstants.MAX_SERIES_TERMS; n++)
                    {
                        try
                        {
                            _parser.SetVariable(calculation.Variable, n);
                            double term = _parser.Evaluate(calculation.Formula);

                            if (double.IsNaN(term) || double.IsInfinity(term))
                                break;

                            sum += term;
                            computedTerms++;

                            // Проверка сходимости для бесконечных рядов
                            if (calculation.IsInfinite && computedTerms > 100)
                            {
                                if (Math.Abs(sum - previousSum) < CalculatorConstants.DEFAULT_TOLERANCE)
                                {
                                    converged = true;
                                    break;
                                }
                                previousSum = sum;
                            }
                        }
                        catch
                        {
                            break;
                        }
                    }

                    calculation.Result = sum;
                    calculation.ComputedTerms = computedTerms;
                    calculation.Converges = calculation.IsInfinite ? converged : true;
                    calculation.Error = calculation.IsInfinite && converged ?
                        Math.Abs(sum - previousSum) : 0;

                    return new CalculationResult
                    {
                        Expression = MathFormatter.FormatSeries(calculation),
                        Result = calculation,
                        FormattedResult = MathFormatter.FormatNumber(sum, CalculatorConstants.SCIENTIFIC_DECIMAL_PLACES),
                        Type = CalculationType.Advanced,
                        Details = $"Вычислено членов: {computedTerms}" +
                                 (calculation.IsInfinite ? $", Сходится: {(calculation.Converges ? "Да" : "Нет")}" : "")
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult($"Σ {calculation.Formula}", ex.Message);
                }
            });
        }

        // Производные (численные)
        public async Task<CalculationResult> DerivativeAsync(string function, string variable, double point,
            double stepSize = 1e-8)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!IsValidFunction(function, variable))
                    {
                        return CreateErrorResult($"d/d{variable} {function}", CalculatorConstants.ERROR_INVALID_FUNCTION);
                    }

                    // Численная производная методом центральных разностей
                    Func<double, double> func = x =>
                    {
                        _parser.SetVariable(variable, x);
                        return _parser.Evaluate(function);
                    };

                    double derivative = (func(point + stepSize) - func(point - stepSize)) / (2 * stepSize);

                    return new CalculationResult
                    {
                        Expression = $"d/d{variable} {function} | {variable}={point}",
                        Result = derivative,
                        FormattedResult = MathFormatter.FormatNumber(derivative, CalculatorConstants.SCIENTIFIC_DECIMAL_PLACES),
                        Type = CalculationType.Advanced
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult($"d/d{variable} {function}", ex.Message);
                }
            });
        }

        // Производные символьные (базовая реализация)
        public async Task<CalculationResult> SymbolicDerivativeAsync(string function, string variable)
        {
            return await Task.Run(() =>
            {
                // Простая символьная дифференциация для основных функций
                try
                {
                    string derivative = DifferentiateSymbolically(function, variable);

                    return new CalculationResult
                    {
                        Expression = $"d/d{variable} ({function})",
                        Result = derivative,
                        FormattedResult = derivative,
                        Type = CalculationType.Advanced
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult($"d/d{variable} {function}", ex.Message);
                }
            });
        }

        // Решение уравнений
        public async Task<CalculationResult> SolveEquationAsync(string equation, string variable,
            double initialGuess = 0, double tolerance = 1e-10)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Метод Ньютона для решения уравнений
                    Func<double, double> func = x =>
                    {
                        _parser.SetVariable(variable, x);
                        return _parser.Evaluate(equation);
                    };

                    double x = initialGuess;
                    const double h = 1e-8;
                    const int maxIterations = 1000;

                    for (int i = 0; i < maxIterations; i++)
                    {
                        double fx = func(x);
                        if (Math.Abs(fx) < tolerance)
                            break;

                        // Численная производная
                        double dfx = (func(x + h) - func(x - h)) / (2 * h);

                        if (Math.Abs(dfx) < tolerance)
                            throw new InvalidOperationException("Производная близка к нулю");

                        x = x - fx / dfx;
                    }

                    return new CalculationResult
                    {
                        Expression = $"Solve: {equation} = 0",
                        Result = x,
                        FormattedResult = $"{variable} = {MathFormatter.FormatNumber(x, CalculatorConstants.SCIENTIFIC_DECIMAL_PLACES)}",
                        Type = CalculationType.Advanced
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult($"Solve: {equation}", ex.Message);
                }
            });
        }

        // Нахождение корней функции
        public async Task<CalculationResult> FindRootsAsync(string function, string variable,
            double lowerBound, double upperBound, double tolerance = 1e-10)
        {
            return await Task.Run(() =>
            {
                try
                {
                    Func<double, double> func = x =>
                    {
                        _parser.SetVariable(variable, x);
                        return _parser.Evaluate(function);
                    };

                    // Метод бисекции
                    double a = lowerBound;
                    double b = upperBound;

                    if (func(a) * func(b) >= 0)
                    {
                        return CreateErrorResult($"Roots of {function}",
                            "Функция должна иметь разные знаки на концах интервала");
                    }

                    double root = 0;
                    const int maxIterations = 1000;

                    for (int i = 0; i < maxIterations; i++)
                    {
                        root = (a + b) / 2;
                        double fRoot = func(root);

                        if (Math.Abs(fRoot) < tolerance || (b - a) / 2 < tolerance)
                            break;

                        if (func(a) * fRoot < 0)
                            b = root;
                        else
                            a = root;
                    }

                    return new CalculationResult
                    {
                        Expression = $"Root of {function} in [{lowerBound}, {upperBound}]",
                        Result = root,
                        FormattedResult = $"{variable} = {MathFormatter.FormatNumber(root, CalculatorConstants.SCIENTIFIC_DECIMAL_PLACES)}",
                        Type = CalculationType.Advanced
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult($"Roots of {function}", ex.Message);
                }
            });
        }

        // Статистические функции
        public async Task<CalculationResult> MeanAsync(double[] values)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (values == null || values.Length == 0)
                        return CreateErrorResult("Mean", "Массив не может быть пустым");

                    double mean = values.Average();

                    return new CalculationResult
                    {
                        Expression = $"Mean of {values.Length} values",
                        Result = mean,
                        FormattedResult = MathFormatter.FormatNumber(mean),
                        Type = CalculationType.Advanced
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult("Mean", ex.Message);
                }
            });
        }

        public async Task<CalculationResult> MedianAsync(double[] values)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (values == null || values.Length == 0)
                        return CreateErrorResult("Median", "Массив не может быть пустым");

                    var sorted = values.OrderBy(x => x).ToArray();
                    double median;

                    if (sorted.Length % 2 == 0)
                        median = (sorted[sorted.Length / 2 - 1] + sorted[sorted.Length / 2]) / 2;
                    else
                        median = sorted[sorted.Length / 2];

                    return new CalculationResult
                    {
                        Expression = $"Median of {values.Length} values",
                        Result = median,
                        FormattedResult = MathFormatter.FormatNumber(median),
                        Type = CalculationType.Advanced
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult("Median", ex.Message);
                }
            });
        }

        public async Task<CalculationResult> StandardDeviationAsync(double[] values)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (values == null || values.Length == 0)
                        return CreateErrorResult("StdDev", "Массив не может быть пустым");

                    double mean = values.Average();
                    double variance = values.Sum(x => Math.Pow(x - mean, 2)) / values.Length;
                    double stdDev = Math.Sqrt(variance);

                    return new CalculationResult
                    {
                        Expression = $"StdDev of {values.Length} values",
                        Result = stdDev,
                        FormattedResult = MathFormatter.FormatNumber(stdDev),
                        Type = CalculationType.Advanced
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult("StdDev", ex.Message);
                }
            });
        }

        public async Task<CalculationResult> VarianceAsync(double[] values)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (values == null || values.Length == 0)
                        return CreateErrorResult("Variance", "Массив не может быть пустым");

                    double mean = values.Average();
                    double variance = values.Sum(x => Math.Pow(x - mean, 2)) / values.Length;

                    return new CalculationResult
                    {
                        Expression = $"Variance of {values.Length} values",
                        Result = variance,
                        FormattedResult = MathFormatter.FormatNumber(variance),
                        Type = CalculationType.Advanced
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult("Variance", ex.Message);
                }
            });
        }

        // Проверка сходимости ряда
        public bool CheckSeriesConvergence(string formula, string variable, int startIndex, int testTerms = 1000)
        {
            try
            {
                double previousSum = 0;
                double currentSum = 0;

                for (int n = startIndex; n < startIndex + testTerms; n++)
                {
                    _parser.SetVariable(variable, n);
                    double term = _parser.Evaluate(formula);

                    if (double.IsNaN(term) || double.IsInfinity(term))
                        return false;

                    currentSum += term;

                    if (n > startIndex + 100 && Math.Abs(currentSum - previousSum) < CalculatorConstants.DEFAULT_TOLERANCE)
                        return true;

                    previousSum = currentSum;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        // Валидация функций
        public bool IsValidFunction(string function, string variable)
        {
            try
            {
                _parser.SetVariable(variable, 1.0);
                _parser.Evaluate(function);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryParseFunction(string function, out Func<double, double> compiledFunction)
        {
            compiledFunction = null;
            try
            {
                compiledFunction = x =>
                {
                    _parser.SetVariable("x", x);
                    return _parser.Evaluate(function);
                };

                // Тестируем функцию
                compiledFunction(1.0);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Вычисление функции в точке
        public async Task<CalculationResult> EvaluateFunctionAsync(string function, string variable, double value)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _parser.SetVariable(variable, value);
                    double result = _parser.Evaluate(function);

                    return new CalculationResult
                    {
                        Expression = $"{function} | {variable}={value}",
                        Result = result,
                        FormattedResult = MathFormatter.FormatNumber(result),
                        Type = CalculationType.Advanced
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult($"{function} | {variable}={value}", ex.Message);
                }
            });
        }

        // Построение таблицы значений функции
        public async Task<CalculationResult> GenerateFunctionTableAsync(string function, string variable,
            double start, double end, double step)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var points = new List<(double x, double y)>();

                    for (double x = start; x <= end; x += step)
                    {
                        try
                        {
                            _parser.SetVariable(variable, x);
                            double y = _parser.Evaluate(function);

                            if (!double.IsNaN(y) && !double.IsInfinity(y))
                                points.Add((x, y));
                        }
                        catch
                        {
                            // Пропускаем точки, где функция не определена
                        }
                    }

                    var table = string.Join("\n", points.Select(p =>
                        $"{variable} = {MathFormatter.FormatNumber(p.x, 2)}, f({variable}) = {MathFormatter.FormatNumber(p.y, 4)}"));

                    return new CalculationResult
                    {
                        Expression = $"Table for {function}",
                        Result = points,
                        FormattedResult = table,
                        Type = CalculationType.Advanced,
                        Details = $"Построено {points.Count} точек"
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult($"Table for {function}", ex.Message);
                }
            });
        }

        // Комплексные числа
        public async Task<CalculationResult> ComplexAddAsync(double real1, double imag1, double real2, double imag2)
        {
            return await Task.Run(() =>
            {
                try
                {
                    double realResult = real1 + real2;
                    double imagResult = imag1 + imag2;

                    return new CalculationResult
                    {
                        Expression = $"({MathFormatter.FormatComplexNumber(real1, imag1)}) + ({MathFormatter.FormatComplexNumber(real2, imag2)})",
                        Result = new { Real = realResult, Imaginary = imagResult },
                        FormattedResult = MathFormatter.FormatComplexNumber(realResult, imagResult),
                        Type = CalculationType.Advanced
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult("Complex addition", ex.Message);
                }
            });
        }

        public async Task<CalculationResult> ComplexMultiplyAsync(double real1, double imag1, double real2, double imag2)
        {
            return await Task.Run(() =>
            {
                try
                {
                    double realResult = real1 * real2 - imag1 * imag2;
                    double imagResult = real1 * imag2 + imag1 * real2;

                    return new CalculationResult
                    {
                        Expression = $"({MathFormatter.FormatComplexNumber(real1, imag1)}) × ({MathFormatter.FormatComplexNumber(real2, imag2)})",
                        Result = new { Real = realResult, Imaginary = imagResult },
                        FormattedResult = MathFormatter.FormatComplexNumber(realResult, imagResult),
                        Type = CalculationType.Advanced
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult("Complex multiplication", ex.Message);
                }
            });
        }

        public async Task<CalculationResult> ComplexPowerAsync(double real, double imag, double power)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Преобразуем в полярную форму
                    double modulus = Math.Sqrt(real * real + imag * imag);
                    double argument = Math.Atan2(imag, real);

                    // Возводим в степень
                    double newModulus = Math.Pow(modulus, power);
                    double newArgument = argument * power;

                    // Преобразуем обратно в декартову форму
                    double realResult = newModulus * Math.Cos(newArgument);
                    double imagResult = newModulus * Math.Sin(newArgument);

                    return new CalculationResult
                    {
                        Expression = $"({MathFormatter.FormatComplexNumber(real, imag)})^{power}",
                        Result = new { Real = realResult, Imaginary = imagResult },
                        FormattedResult = MathFormatter.FormatComplexNumber(realResult, imagResult),
                        Type = CalculationType.Advanced
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult("Complex power", ex.Message);
                }
            });
        }

        public async Task<CalculationResult> ComplexModulusAsync(double real, double imag)
        {
            return await Task.Run(() =>
            {
                try
                {
                    double modulus = Math.Sqrt(real * real + imag * imag);

                    return new CalculationResult
                    {
                        Expression = $"|{MathFormatter.FormatComplexNumber(real, imag)}|",
                        Result = modulus,
                        FormattedResult = MathFormatter.FormatNumber(modulus),
                        Type = CalculationType.Advanced
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult("Complex modulus", ex.Message);
                }
            });
        }

        // Вспомогательные методы
        private double CalculateNumericalLimit(LimitCalculation calculation)
        {
            const double epsilon = 1e-10;
            const int maxSteps = 20;

            Func<double, double> func = x =>
            {
                _parser.SetVariable(calculation.Variable, x);
                return _parser.Evaluate(calculation.Function);
            };

            double step = 1.0;
            double? leftLimit = null;
            double? rightLimit = null;

            // Вычисляем предел приближаясь к точке
            for (int i = 0; i < maxSteps; i++)
            {
                step /= 10;

                try
                {
                    if (calculation.ApproachType != ApproachType.RightSided)
                        leftLimit = func(calculation.ApproachingValue - step);

                    if (calculation.ApproachType != ApproachType.LeftSided)
                        rightLimit = func(calculation.ApproachingValue + step);
                }
                catch
                {
                    continue;
                }
            }

            return calculation.ApproachType switch
            {
                ApproachType.LeftSided => leftLimit ?? double.NaN,
                ApproachType.RightSided => rightLimit ?? double.NaN,
                _ => (leftLimit.HasValue && rightLimit.HasValue &&
                      Math.Abs(leftLimit.Value - rightLimit.Value) < epsilon) ?
                     leftLimit.Value : double.NaN
            };
        }

        private double SimpsonRule(Func<double, double> func, double a, double b, int n)
        {
            if (n % 2 == 1) n++; // Simpson's rule requires even number of intervals

            double h = (b - a) / n;
            double sum = func(a) + func(b);

            for (int i = 1; i < n; i++)
            {
                double x = a + i * h;
                sum += (i % 2 == 0 ? 2 : 4) * func(x);
            }

            return sum * h / 3;
        }

        private string DifferentiateSymbolically(string function, string variable)
        {
            // Простая символьная дифференциация для базовых функций
            // В реальном проекте стоит использовать более продвинутую библиотеку

            function = function.Replace(" ", "").ToLower();

            // Константы
            if (!function.Contains(variable))
                return "0";

            // Переменная
            if (function == variable)
                return "1";

            // Степени
            if (function == $"{variable}^2")
                return $"2*{variable}";
            if (function == $"{variable}^3")
                return $"3*{variable}^2";

            // Основные функции
            if (function == $"sin({variable})")
                return $"cos({variable})";
            if (function == $"cos({variable})")
                return $"-sin({variable})";
            if (function == $"tan({variable})")
                return $"1/cos({variable})^2";
            if (function == $"ln({variable})")
                return $"1/{variable}";
            if (function == $"exp({variable})")
                return $"exp({variable})";
            if (function == $"sqrt({variable})")
                return $"1/(2*sqrt({variable}))";

            // Для сложных функций возвращаем символическое обозначение
            return $"d/d{variable}({function})";
        }

        private CalculationResult CreateErrorResult(string expression, string errorMessage)
        {
            return new CalculationResult
            {
                Expression = expression,
                HasError = true,
                ErrorMessage = errorMessage,
                FormattedResult = CalculatorConstants.ERROR_PREFIX + errorMessage,
                Type = CalculationType.Advanced
            };
        }
    }
}