using System;

namespace StudyMateProject.Models
{
    // Модель результата вычисления
    public class CalculationResult
    {
        // Уникальный идентификатор вычисления
        public Guid Id { get; set; } = Guid.NewGuid();

        // Исходное выражение
        public string Expression { get; set; } = string.Empty;

        // Результат вычисления
        public object Result { get; set; } = 0;

        // Отформатированный результат для отображения
        public string FormattedResult { get; set; } = "0";

        // Тип вычисления (Basic, Scientific, Matrix, Advanced)
        public CalculationType Type { get; set; } = CalculationType.Basic;

        // Время выполнения вычисления
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Была ли ошибка при вычислении
        public bool HasError { get; set; } = false;

        // Сообщение об ошибке (если есть)
        public string ErrorMessage { get; set; } = string.Empty;

        // Дополнительные детали для сложных вычислений
        public string Details { get; set; } = string.Empty;

        // Возвращает строковое представление результата
        public override string ToString()
        {
            return HasError ? $"Error: {ErrorMessage}" : $"{Expression} = {FormattedResult}";
        }
    }

    // Типы вычислений
    public enum CalculationType
    {
        // Базовые арифметические операции
        Basic = 0,

        // Научные функции (тригонометрия, логарифмы и т.д.)
        Scientific = 1,

        // Операции с матрицами
        Matrix = 2,

        // Продвинутые операции (интегралы, пределы, ряды)
        Advanced = 3,

        // Конвертация единиц измерения
        Conversion = 4
    }
}