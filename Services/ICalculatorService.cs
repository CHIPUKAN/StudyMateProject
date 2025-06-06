using System;
using System.Collections.Generic;

namespace StudyMateProject.Services
{
    public interface ICalculatorService
    {       
        /// Вычисляет математическое выражение
        /// <param name="expression">Математическое выражение в виде строки</param>
        /// <returns>Результат вычисления</returns>
        double Calculate(string expression);

        
        /// Форматирует результат для отображения        
        /// <param name="result">Число для форматирования</param>
        /// <returns>Отформатированная строка</returns>

        string FormatResult(double result);

        
        /// Проверяет корректность математического выражения
        /// <param name="expression">Выражение для проверки</param>
        /// <returns>true, если выражение корректно</returns>

        bool IsValidExpression(string expression);
        
        /// Вычисляет квадратный корень
        /// <param name="value">Число</param>
        /// <returns>Квадратный корень</returns>

        double CalculateSquareRoot(double value);        
        /// Возводит число в степень        
        /// <param name="baseValue">Основание</param>
        /// <param name="exponent">Показатель степени</param>
        /// <returns>Результат возведения в степень</returns>

        double CalculatePower(double baseValue, double exponent);        
        /// Вычисляет процент от числа
        /// <param name="value">Число</param>
        /// <param name="percentage">Процент</param>
        /// <returns>Процент от числа</returns>

        double CalculatePercentage(double value, double percentage);
    }
}