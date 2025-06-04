using System;

namespace StudyMateProject.Models
{
    // Модель для вычисления пределов
    public class LimitCalculation
    {
        // Уникальный идентификатор
        public Guid Id { get; set; } = Guid.NewGuid();

        // Функция для вычисления предела (как строка)
        public string Function { get; set; } = string.Empty;

        // Переменная (обычно x)
        public string Variable { get; set; } = "x";

        // Точка, к которой стремится переменная
        public double ApproachingValue { get; set; }

        // Стремится ли к бесконечности
        public bool IsInfinity { get; set; } = false;

        // Тип приближения (слева, справа, двухсторонний)
        public ApproachType ApproachType { get; set; } = ApproachType.TwoSided;

        // Результат вычисления предела
        public double Result { get; set; }

        // Является ли результат бесконечностью
        public bool IsResultInfinity { get; set; } = false;

        // Существует ли предел
        public bool Exists { get; set; } = true;

        // Время вычисления
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public override string ToString()
        {
            var approach = IsInfinity ? "∞" : ApproachingValue.ToString();
            var result = IsResultInfinity ? "∞" : Result.ToString("F6");
            return $"lim({Variable}→{approach}) {Function} = {result}";
        }
    }

    // Типы приближения для пределов
    public enum ApproachType
    {
        // Двухсторонний предел
        TwoSided,

        // Предел слева
        LeftSided,

        // Предел справа
        RightSided
    }
}