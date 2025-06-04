using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StudyMateProject.Models
{
    // Модель истории вычислений
    public class CalculationHistory
    {
        // Коллекция всех вычислений
        public ObservableCollection<CalculationResult> Calculations { get; set; }

        // Максимальное количество записей в истории
        public int MaxHistorySize { get; set; } = 100;

        public CalculationHistory()
        {
            Calculations = new ObservableCollection<CalculationResult>();
        }

        // Добавляет новое вычисление в историю
        public void AddCalculation(CalculationResult result)
        {
            if (result == null) return;

            // Добавляем в начало списка (последние вычисления сверху)
            Calculations.Insert(0, result);

            // Удаляем старые записи, если превышен лимит
            while (Calculations.Count > MaxHistorySize)
            {
                Calculations.RemoveAt(Calculations.Count - 1);
            }
        }

        // Получает последние вычисления определенного типа
        public List<CalculationResult> GetByType(CalculationType type, int count = 10)
        {
            return Calculations
                .Where(c => c.Type == type)
                .Take(count)
                .ToList();
        }

        // Получает последние успешные вычисления
        public List<CalculationResult> GetSuccessful(int count = 10)
        {
            return Calculations
                .Where(c => !c.HasError)
                .Take(count)
                .ToList();
        }

        // Очищает всю историю
        public void Clear()
        {
            Calculations.Clear();
        }

        // Удаляет конкретное вычисление
        public void Remove(CalculationResult result)
        {
            if (result != null)
            {
                Calculations.Remove(result);
            }
        }

        // Поиск в истории по выражению
        public List<CalculationResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<CalculationResult>();

            query = query.ToLower();
            return Calculations
                .Where(c => c.Expression.ToLower().Contains(query) ||
                           c.FormattedResult.ToLower().Contains(query))
                .ToList();
        }

        // Получает количество вычислений по типам
        public Dictionary<CalculationType, int> GetStatistics()
        {
            return Calculations
                .GroupBy(c => c.Type)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}