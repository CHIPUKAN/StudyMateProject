using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudyMateProject.Models;
using StudyMateProject.Services;

namespace StudyMateProject.ViewModels
{
    // Компактная ViewModel для мини-калькулятора, встраиваемого в заметки
    // Наследуется от CalculatorViewModel для переиспользования основной логики
    public partial class MiniCalculatorViewModel : ObservableObject
    {
        #region Private Fields

        // Базовая ViewModel калькулятора для переиспользования логики
        private readonly CalculatorViewModel _baseCalculator;

        // Сервис интеграции для работы с заметками
        private readonly ICalculatorIntegrationService _integrationService;

        // ID заметки, в которую встроен калькулятор
        private string? _currentNoteId;

        // Позиция курсора в заметке для вставки результата
        private int _cursorPosition = -1;

        #endregion

        #region Observable Properties

        // Компактный дисплей - показывает только результат, без истории
        [ObservableProperty]
        private string displayText = "0";

        // Показывать ли кнопки научных функций в мини-режиме
        [ObservableProperty]
        private bool showScientificButtons = true;

        // Показывать ли кнопку "Развернуть в полноэкранный режим"
        [ObservableProperty]
        private bool showExpandButton = true;

        // Показывать ли кнопку "Вставить результат в заметку"
        [ObservableProperty]
        private bool showInsertButton = true;

        // Компактный режим - еще меньше кнопок для экономии места
        [ObservableProperty]
        private bool isCompactMode = false;

        // Прозрачность калькулятора (для ненавязчивого отображения)
        [ObservableProperty]
        private double opacity = 1.0;

        // Размер кнопок в мини-режиме
        [ObservableProperty]
        private double buttonSize = 40.0;

        #endregion

        #region Commands для мини-калькулятора

        // Все основные команды калькулятора
        public ICommand NumberCommand { get; private set; }
        public ICommand OperatorCommand { get; private set; }
        public ICommand CalculateCommand { get; private set; }
        public ICommand ClearCommand { get; private set; }
        public ICommand BackspaceCommand { get; private set; }
        public ICommand DecimalCommand { get; private set; }

        // Основные научные функции (ограниченный набор для экономии места)
        public ICommand SinCommand { get; private set; }
        public ICommand CosCommand { get; private set; }
        public ICommand LogCommand { get; private set; }
        public ICommand SqrtCommand { get; private set; }
        public ICommand PiCommand { get; private set; }

        // Специальные команды для мини-режима
        public ICommand InsertResultCommand { get; private set; }    // Вставить результат в заметку
        public ICommand ExpandToFullCommand { get; private set; }    // Развернуть в полноэкранный режим
        public ICommand ToggleCompactCommand { get; private set; }   // Переключить компактный режим
        public ICommand CloseCommand { get; private set; }           // Закрыть мини-калькулятор

        #endregion

        #region Constructor

        public MiniCalculatorViewModel(
            CalculatorViewModel baseCalculator,
            ICalculatorIntegrationService integrationService)
        {
            _baseCalculator = baseCalculator ?? throw new ArgumentNullException(nameof(baseCalculator));
            _integrationService = integrationService ?? throw new ArgumentNullException(nameof(integrationService));

            // Инициализируем команды
            InitializeCommands();

            // Подписываемся на изменения в базовом калькуляторе
            _baseCalculator.PropertyChanged += OnBaseCalculatorPropertyChanged;

            // Синхронизируем начальное состояние
            DisplayText = _baseCalculator.DisplayText;

            // Настраиваем мини-режим
            SetupMiniMode();
        }

        #endregion

        #region Command Initialization

        private void InitializeCommands()
        {
            // Проксируем команды к базовому калькулятору, но добавляем свою логику
            NumberCommand = new RelayCommand<string>(OnNumberPressed);
            OperatorCommand = new RelayCommand<string>(OnOperatorPressed);
            CalculateCommand = new RelayCommand(OnCalculate);
            ClearCommand = new RelayCommand(OnClear);
            BackspaceCommand = new RelayCommand(OnBackspace);
            DecimalCommand = new RelayCommand(OnDecimalPressed);

            // Ограниченный набор научных функций для мини-режима
            SinCommand = new RelayCommand(() => OnScientificFunction("sin"));
            CosCommand = new RelayCommand(() => OnScientificFunction("cos"));
            LogCommand = new RelayCommand(() => OnScientificFunction("log"));
            SqrtCommand = new RelayCommand(() => OnScientificFunction("sqrt"));
            PiCommand = new RelayCommand(() => OnConstantPressed("π"));

            // Специальные команды мини-калькулятора
            InsertResultCommand = new AsyncRelayCommand(OnInsertResultAsync);
            ExpandToFullCommand = new AsyncRelayCommand(OnExpandToFullAsync);
            ToggleCompactCommand = new RelayCommand(OnToggleCompact);
            CloseCommand = new RelayCommand(OnClose);
        }

        #endregion

        #region Command Handlers

        // Проксируем нажатие цифр к базовому калькулятору
        private void OnNumberPressed(string? number)
        {
            if (!string.IsNullOrEmpty(number))
            {
                // Используем метод базового калькулятора
                _baseCalculator.InsertText(number);
            }
        }

        private void OnOperatorPressed(string? op)
        {
            if (!string.IsNullOrEmpty(op))
            {
                _baseCalculator.InsertText($" {op} ");
            }
        }

        private void OnCalculate()
        {
            // Выполняем вычисление через базовый калькулятор
            _baseCalculator.CalculateCommand.Execute(null);
        }

        private void OnClear()
        {
            _baseCalculator.ClearCommand.Execute(null);
        }

        private void OnBackspace()
        {
            _baseCalculator.BackspaceCommand.Execute(null);
        }

        private void OnDecimalPressed()
        {
            _baseCalculator.DecimalCommand.Execute(null);
        }

        private void OnScientificFunction(string function)
        {
            _baseCalculator.InsertText($"{function}(");
        }

        private void OnConstantPressed(string constant)
        {
            _baseCalculator.InsertText(constant);
        }

        #endregion

        #region Mini-Calculator Specific Commands

        // Вставка результата в заметку
        private async Task OnInsertResultAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_currentNoteId))
                {
                    // Если нет привязки к заметке, просто копируем в буфер
                    await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.SetTextAsync(DisplayText);
                    return;
                }

                // Создаем результат для вставки
                var result = new CalculationResult
                {
                    Expression = _baseCalculator.HistoryText,
                    FormattedResult = DisplayText,
                    Type = CalculationType.Basic,
                    Timestamp = DateTime.Now
                };

                // Вставляем результат в заметку через сервис интеграции
                var success = await _integrationService.InsertResultIntoNoteAsync(
                    _currentNoteId,
                    result,
                    _cursorPosition);

                if (success)
                {
                    // Опционально: закрываем мини-калькулятор после вставки
                    // OnClose();
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок - показываем уведомление или логируем
                System.Diagnostics.Debug.WriteLine($"Ошибка вставки результата: {ex.Message}");
            }
        }

        // Развертывание в полноэкранный режим
        private async Task OnExpandToFullAsync()
        {
            try
            {
                // Переключаем базовый калькулятор в полноэкранный режим
                await _baseCalculator.SetModeAsync(CalculatorMode.FullScreen);

                // Уведомляем о желании открыть полноэкранный калькулятор
                // Это будет обработано на уровне навигации
                ExpandRequested?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка развертывания: {ex.Message}");
            }
        }

        // Переключение компактного режима
        private void OnToggleCompact()
        {
            IsCompactMode = !IsCompactMode;

            // В компактном режиме скрываем научные функции и уменьшаем кнопки
            if (IsCompactMode)
            {
                ShowScientificButtons = false;
                ButtonSize = 35.0;
                Opacity = 0.9;
            }
            else
            {
                ShowScientificButtons = true;
                ButtonSize = 40.0;
                Opacity = 1.0;
            }
        }

        // Закрытие мини-калькулятора
        private void OnClose()
        {
            // Уведомляем о желании закрыть калькулятор
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Public Methods

        // Привязка калькулятора к конкретной заметке
        public void AttachToNote(string noteId, int cursorPosition = -1)
        {
            _currentNoteId = noteId;
            _cursorPosition = cursorPosition;

            // Показываем кнопку вставки только если привязаны к заметке
            ShowInsertButton = !string.IsNullOrEmpty(_currentNoteId);
        }

        // Отвязка от заметки
        public void DetachFromNote()
        {
            _currentNoteId = null;
            _cursorPosition = -1;
            ShowInsertButton = false;
        }

        // Установка размера для адаптации к доступному пространству
        public void SetSize(double availableWidth, double availableHeight)
        {
            // Адаптируем размер кнопок под доступное пространство
            var optimalButtonSize = Math.Min(availableWidth / 6, availableHeight / 8);
            ButtonSize = Math.Max(30, Math.Min(50, optimalButtonSize));

            // В очень маленьком пространстве включаем компактный режим
            if (availableWidth < 300 || availableHeight < 200)
            {
                IsCompactMode = true;
                OnToggleCompact();
            }
        }

        // Получение текущего результата для внешнего использования
        public string GetCurrentResult()
        {
            return DisplayText;
        }

        // Вставка текста извне (например, из контекстного меню заметки)
        public void InsertText(string text)
        {
            _baseCalculator.InsertText(text);
        }

        #endregion

        #region Event Handlers

        // Синхронизация с базовым калькулятором
        private void OnBaseCalculatorPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Синхронизируем отображение с базовым калькулятором
            if (e.PropertyName == nameof(_baseCalculator.DisplayText))
            {
                DisplayText = _baseCalculator.DisplayText;
            }
        }

        #endregion

        #region Private Helper Methods

        // Настройка мини-режима при инициализации
        private void SetupMiniMode()
        {
            // Устанавливаем режим калькулятора
            _baseCalculator.SetModeAsync(CalculatorMode.Mini);

            // Настройки по умолчанию для мини-режима
            ShowScientificButtons = true;  // Показываем основные научные функции
            ShowExpandButton = true;       // Показываем кнопку развертывания
            ButtonSize = 40.0;            // Средний размер кнопок
            Opacity = 1.0;                // Полная непрозрачность
        }

        #endregion

        #region Events

        // События для уведомления родительского View
        public event EventHandler? ExpandRequested;    // Запрос на развертывание
        public event EventHandler? CloseRequested;     // Запрос на закрытие
        public event EventHandler<string>? ResultInserted;  // Результат вставлен

        #endregion

        #region Cleanup

        public void Dispose()
        {
            // Отписываемся от событий базового калькулятора
            if (_baseCalculator != null)
            {
                _baseCalculator.PropertyChanged -= OnBaseCalculatorPropertyChanged;
            }
        }

        #endregion
    }
}