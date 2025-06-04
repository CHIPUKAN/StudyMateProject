using System;

namespace StudyMateProject.Models
{
    // Модель для вычисления интегралов
    public class IntegralCalculation
    {
        // Уникальный идентификатор
        public Guid Id { get; set; } = Guid.NewGuid();

        // Функция для интегрирования (как строка)
        public string Function { get; set; } = string.Empty;

        // Нижний предел интегрирования
        public double LowerLimit { get; set; }

        // Верхний предел интегрирования
        public double UpperLimit { get; set; }

        // Результат интегрирования
        public double Result { get; set; }

        // Погрешность вычисления
        public double Error { get; set; }

        // Метод интегрирования
        public IntegrationMethod Method { get; set; } = IntegrationMethod.Adaptive;

        // Количество шагов для численного интегрирования
        public int Steps { get; set; } = 1000;

        // Время вычисления
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public override string ToString()
        {
            return $"∫[{LowerLimit}→{UpperLimit}] {Function} dx = {Result:F6}";
        }
    }

    // Методы интегрирования
    public enum IntegrationMethod
    {
        // Адаптивный метод
        Adaptive,

        // Метод трапеций
        Trapezoidal,

        // Метод Симпсона
        Simpson,

        // Метод Гаусса
        Gaussian
    }
}