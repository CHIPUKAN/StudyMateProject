using System;
using System.Threading.Tasks;
using StudyMateProject.Models;

namespace StudyMateProject.Services
{
    // Интерфейс для базовых вычислений калькулятора
    public interface ICalculationService
    {
        // Вычисляет математическое выражение из строки
        Task<CalculationResult> EvaluateAsync(string expression);

        // Выполняет базовые арифметические операции
        CalculationResult Add(double a, double b);
        CalculationResult Subtract(double a, double b);
        CalculationResult Multiply(double a, double b);
        CalculationResult Divide(double a, double b);
        CalculationResult Power(double baseValue, double exponent);
        CalculationResult SquareRoot(double value);
        CalculationResult Percentage(double value, double percent);

        // Тригонометрические функции
        CalculationResult Sin(double angle, bool isRadians = false);
        CalculationResult Cos(double angle, bool isRadians = false);
        CalculationResult Tan(double angle, bool isRadians = false);
        CalculationResult ArcSin(double value);
        CalculationResult ArcCos(double value);
        CalculationResult ArcTan(double value);

        // Логарифмические функции
        CalculationResult Log(double value, double baseValue = 10);
        CalculationResult Ln(double value);
        CalculationResult Exp(double exponent);

        // Дополнительные научные функции
        CalculationResult Factorial(double n);
        CalculationResult Combination(int n, int r);
        CalculationResult Permutation(int n, int r);
        CalculationResult Absolute(double value);

        // Константы
        double PI { get; }
        double E { get; }

        // Конвертация углов
        double DegreesToRadians(double degrees);
        double RadiansToDegrees(double radians);

        // Проверка валидности выражения
        bool IsValidExpression(string expression);

        // Форматирование числа для отображения
        string FormatNumber(double number, int decimalPlaces = 10);
    }
}