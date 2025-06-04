using System;

namespace StudyMateProject.Models
{
    // Модель для вычисления сумм рядов
    public class SeriesCalculation
    {
        // Уникальный идентификатор
        public Guid Id { get; set; } = Guid.NewGuid();

        // Формула общего члена ряда
        public string Formula { get; set; } = string.Empty;

        // Переменная (обычно n)
        public string Variable { get; set; } = "n";

        // Начальный индекс
        public int StartIndex { get; set; } = 1;

        // Конечный индекс (если конечная сумма)
        public int? EndIndex { get; set; }

        // Является ли ряд бесконечным
        public bool IsInfinite => !EndIndex.HasValue;

        // Результат суммирования
        public double Result { get; set; }

        // Сходится ли ряд (для бесконечных рядов)
        public bool Converges { get; set; } = true;

        // Количество вычисленных членов (для бесконечных рядов)
        public int ComputedTerms { get; set; }

        // Погрешность (для бесконечных рядов)
        public double Error { get; set; }

        // Время вычисления
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public override string ToString()
        {
            var endStr = IsInfinite ? "∞" : EndIndex.ToString();
            return $"Σ[{Variable}={StartIndex}→{endStr}] {Formula} = {Result:F6}";
        }
    }
}
