using System;
using System.Threading.Tasks;
using StudyMateProject.Models;

namespace StudyMateProject.Services
{
    // Интерфейс для продвинутых математических операций
    public interface IAdvancedMathService
    {
        // Интегралы
        Task<CalculationResult> IntegrateAsync(string function, double lowerLimit, double upperLimit,
            IntegrationMethod method = IntegrationMethod.Adaptive, int steps = 1000);

        Task<CalculationResult> IntegrateAsync(IntegralCalculation calculation);

        // Пределы
        Task<CalculationResult> CalculateLimitAsync(string function, string variable, double approachingValue,
            ApproachType approachType = ApproachType.TwoSided);

        Task<CalculationResult> CalculateLimitAsync(LimitCalculation calculation);

        // Пределы в бесконечности
        Task<CalculationResult> CalculateLimitAtInfinityAsync(string function, string variable,
            bool positiveInfinity = true);

        // Суммы рядов
        Task<CalculationResult> CalculateSeriesAsync(string formula, string variable, int startIndex,
            int? endIndex = null, int maxTerms = 10000);

        Task<CalculationResult> CalculateSeriesAsync(SeriesCalculation calculation);

        // Производные (численные)
        Task<CalculationResult> DerivativeAsync(string function, string variable, double point,
            double stepSize = 1e-8);

        // Производные символьные (если возможно)
        Task<CalculationResult> SymbolicDerivativeAsync(string function, string variable);

        // Решение уравнений
        Task<CalculationResult> SolveEquationAsync(string equation, string variable,
            double initialGuess = 0, double tolerance = 1e-10);

        // Нахождение корней функции
        Task<CalculationResult> FindRootsAsync(string function, string variable,
            double lowerBound, double upperBound, double tolerance = 1e-10);

        // Статистические функции
        Task<CalculationResult> MeanAsync(double[] values);
        Task<CalculationResult> MedianAsync(double[] values);
        Task<CalculationResult> StandardDeviationAsync(double[] values);
        Task<CalculationResult> VarianceAsync(double[] values);

        // Проверка сходимости ряда
        bool CheckSeriesConvergence(string formula, string variable, int startIndex, int testTerms = 1000);

        // Валидация функций
        bool IsValidFunction(string function, string variable);

        // Парсинг функций
        bool TryParseFunction(string function, out Func<double, double> compiledFunction);

        // Вычисление функции в точке
        Task<CalculationResult> EvaluateFunctionAsync(string function, string variable, double value);

        // Построение таблицы значений функции
        Task<CalculationResult> GenerateFunctionTableAsync(string function, string variable,
            double start, double end, double step);

        // Комплексные числа
        Task<CalculationResult> ComplexAddAsync(double real1, double imag1, double real2, double imag2);
        Task<CalculationResult> ComplexMultiplyAsync(double real1, double imag1, double real2, double imag2);
        Task<CalculationResult> ComplexPowerAsync(double real, double imag, double power);
        Task<CalculationResult> ComplexModulusAsync(double real, double imag);
    }
}