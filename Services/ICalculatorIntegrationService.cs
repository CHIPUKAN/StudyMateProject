using System;
using System.Threading.Tasks;
using StudyMateProject.Models;
using StudyMateProject.Helpers;

namespace StudyMateProject.Services
{
    // Интерфейс для интеграции калькулятора с другими модулями приложения
    public interface ICalculatorIntegrationService
    {
        // События для уведомления о действиях калькулятора
        event EventHandler<CalculationCompletedEventArgs> CalculationCompleted;
        event EventHandler<ResultInsertRequestedEventArgs> ResultInsertRequested;
        event EventHandler<CalculatorModeChangedEventArgs> ModeChanged;

        // Управление режимами калькулятора
        Task SetModeAsync(CalculatorMode mode);
        CalculatorMode GetCurrentMode();

        // Интеграция с заметками
        Task<bool> InsertResultIntoNoteAsync(string noteId, CalculationResult result, int cursorPosition = -1);
        Task<string> FormatResultForNote(CalculationResult result, ResultFormatType formatType = ResultFormatType.Standard);

        // Сохранение и загрузка вычислений в заметки
        Task SaveCalculationToNoteAsync(string noteId, CalculationResult calculation);
        Task<CalculationResult[]> LoadCalculationsFromNoteAsync(string noteId);

        // Управление историей в контексте заметок
        Task<CalculationHistory> GetNoteCalculationHistoryAsync(string noteId);
        Task ClearNoteHistoryAsync(string noteId);

        // Экспорт результатов
        Task<string> ExportCalculationsAsync(CalculationResult[] calculations, ExportFormat format);
        Task<bool> ShareCalculationAsync(CalculationResult calculation, ShareTarget target);

        // Настройки калькулятора в контексте заметок
        Task<CalculatorSettings> GetCalculatorSettingsAsync();
        Task SaveCalculatorSettingsAsync(CalculatorSettings settings);

        // Валидация и предварительный просмотр
        Task<ValidationResult> ValidateExpressionAsync(string expression);
        Task<string> GetExpressionPreviewAsync(string expression);

        // Интеграция с напоминаниями
        Task CreateReminderFromCalculationAsync(CalculationResult calculation, DateTime reminderTime, string reminderText = "");

        // Поиск и фильтрация
        Task<CalculationResult[]> SearchCalculationsAsync(string query, CalculationType? type = null, DateTime? fromDate = null);
        Task<CalculationResult[]> GetRecentCalculationsAsync(int count = 10, CalculationType? type = null);

        // Синхронизация между устройствами
        Task SyncCalculationHistoryAsync();
        Task<bool> IsHistorySyncEnabledAsync();
        void NotifyCalculationCompleted(CalculationResult result);
    }

    // Режимы работы калькулятора
    public enum CalculatorMode
    {
        // Полноэкранный режим (отдельная вкладка)
        FullScreen,

        // Мини-режим (встроенный в заметки)
        Mini,

        // Popup режим (модальное окно)
        Popup,

        // Режим только для просмотра
        ReadOnly
    }

    // Типы форматирования результатов
    public enum ResultFormatType
    {
        // Стандартный формат (число)
        Standard,

        // С выражением (2+3 = 5)
        WithExpression,

        // Математическая нотация
        Mathematical,

        // Только результат
        ResultOnly,

        // Подробный (с деталями вычисления)
        Detailed
    }

    // Форматы экспорта
    public enum ExportFormat
    {
        // Простой текст
        PlainText,

        // Markdown
        Markdown,

        // LaTeX
        LaTeX,

        // JSON
        Json,

        // CSV (для табличных данных)
        Csv
    }

    // Цели для шаринга
    public enum ShareTarget
    {
        // Копировать в буфер обмена
        Clipboard,

        // Отправить в другое приложение
        ShareSheet,

        // Сохранить в файл
        File,

        // Отправить по email
        Email
    }

    // Настройки калькулятора
    public class CalculatorSettings
    {
        // Режим углов (градусы/радианы)
        public AngleMode AngleMode { get; set; } = AngleMode.Degrees;

        // Количество знаков после запятой
        public int DecimalPlaces { get; set; } = 10;

        // Использовать научную нотацию для больших чисел
        public bool UseScientificNotation { get; set; } = true;

        // Порог для научной нотации
        public double ScientificNotationThreshold { get; set; } = 1e12;

        // Автоматически вставлять результат в заметку
        public bool AutoInsertResults { get; set; } = false;

        // Сохранять историю вычислений
        public bool SaveHistory { get; set; } = true;

        // Максимальный размер истории
        public int MaxHistorySize { get; set; } = 100;

        // Показывать подсказки
        public bool ShowHints { get; set; } = true;

        // Звуковые эффекты
        public bool PlaySounds { get; set; } = false;

        // Вибрация при нажатии кнопок
        public bool HapticFeedback { get; set; } = true;

        // Тема калькулятора
        public CalculatorTheme Theme { get; set; } = CalculatorTheme.Auto;

        // Размер шрифта
        public FontSize FontSize { get; set; } = FontSize.Medium;
    }

    // Режимы углов
    public enum AngleMode
    {
        Degrees,
        Radians,
        Gradians
    }

    // Темы калькулятора
    public enum CalculatorTheme
    {
        Auto,   // Следует системной теме
        Light,
        Dark,
        HighContrast
    }

    // Размеры шрифта
    public enum FontSize
    {
        Small,
        Medium,
        Large,
        ExtraLarge
    }

    // Результат валидации выражения
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string[] Suggestions { get; set; } = Array.Empty<string>();
        public ExpressionInfo Info { get; set; } = new ExpressionInfo();
    }

    // События
    public class CalculationCompletedEventArgs : EventArgs
    {
        public CalculationResult Result { get; set; } = new CalculationResult();
        public CalculatorMode Mode { get; set; }
        public string? NoteId { get; set; }
    }

    public class ResultInsertRequestedEventArgs : EventArgs
    {
        public CalculationResult Result { get; set; } = new CalculationResult();
        public string FormattedResult { get; set; } = string.Empty;
        public int CursorPosition { get; set; } = -1;
        public string? NoteId { get; set; }
    }

    public class CalculatorModeChangedEventArgs : EventArgs
    {
        public CalculatorMode OldMode { get; set; }
        public CalculatorMode NewMode { get; set; }
        public string? Context { get; set; }
    }
}