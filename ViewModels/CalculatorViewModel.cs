using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudyMateProject.Models;
using StudyMateProject.Services;
using StudyMateProject.Helpers;

namespace StudyMateProject.ViewModels
{
    public partial class CalculatorViewModel : ObservableObject
    {
        private readonly ICalculationService _calculationService;
        private readonly ICalculatorIntegrationService _integrationService;

        private readonly CalculationHistory _history; // История всех вычислений
        
        private CalculatorSettings _settings; // Текущие настройки калькулятора

        private bool _justCalculated = false; // Флаг, показывающий, был ли расчет сейчас

        private bool _waitingForOperand = false; // Флаг показывающий, ожидается ли ввод операнда после оператора

        // Основной дисплей калькулятора - показывает текущее выражение или результат
        // [ObservableProperty] автоматически создает свойство с уведомлениями
        [ObservableProperty]
        public string displayText = "0";

        // Доп. дисплей для показа истории операций
        [ObservableProperty]
        public string historyText = "";

        // Режим калькулятора
        [ObservableProperty]
        private CalculatorMode currentMode = CalculatorMode.FullScreen;

        // Показывать ли научные функции (sin, cos, log и т.д.)
        [ObservableProperty]
        private bool showScientificFunctions = true;

        // Показывать ли панель истории
        [ObservableProperty]
        private bool showHistory = false;

        // Режим углов - градусы или радианы для тригонометрических функций
        [ObservableProperty]
        private AngleMode angleMode = AngleMode.Degrees;

        // Флаг показывающий, идет ли сейчас вычисление (для показа загрузки)
        [ObservableProperty]
        private bool isCalculating = false;

        // Текст ошибки, если что-то пошло не так
        [ObservableProperty]
        private string errorMessage = "";

        // Показывать ли сообщение об ошибке
        [ObservableProperty]
        private bool hasError = false;


        public ObservableCollection<CalculationResult> RecentResults { get; }  // Коллекция для быстрого доступа к последним результатам

        public ObservableCollection<CalculationResult> HistoryItems { get; } // Коллекция для отображения историй вычислений

        public ICommand NumberCommand { get; private set; } // Команда для ввода 0-9

        public ICommand OperatorCommand { get; private set; } // Команда для ввода операторов (+, -, *, /)

        public ICommand CalculateCommand { get; private set; } // Команда для выполнения вычисления (кнопка "=")
      
        public ICommand ClearCommand { get; private set; } // Команда для очистки дисплея (кнопка "C")
        
        public ICommand BackspaceCommand { get; private set; } // Команда для удаления последнего символа (кнопка "⌫")
        
        public ICommand DecimalCommand { get; private set; } // Команда для ввода десятичной точки

        // Команды для научных функций
        public ICommand SinCommand { get; private set; }
        public ICommand CosCommand { get; private set; }
        public ICommand TanCommand { get; private set; }
        public ICommand LogCommand { get; private set; }
        public ICommand LnCommand { get; private set; }
        public ICommand SqrtCommand { get; private set; }
        public ICommand PowerCommand { get; private set; }
        public ICommand FactorialCommand { get; private set; }

        // Команды для констант
        public ICommand PiCommand { get; private set; }
        public ICommand ECommand { get; private set; }

        // Команды для управления
        public ICommand ToggleHistoryCommand { get; private set; }
        public ICommand ClearHistoryCommand { get; private set; }
        public ICommand ToggleAngleModeCommand { get; private set; }
        public ICommand CopyResultCommand { get; private set; }
        public ICommand InsertFromHistoryCommand { get; private set; }

        public CalculatorViewModel (ICalculationService calculationService, ICalculatorIntegrationService integrationService)
        {
            // Внедряем зависимости через конструктор (Dependency Injection)
            _calculationService = calculationService ?? throw new ArgumentNullException(nameof(calculationService));
            _integrationService = integrationService ?? throw new ArgumentNullException(nameof(_integrationService));


            // Инициализируем коллекции
            HistoryItems = new ObservableCollection<CalculationResult>();
            RecentResults = new ObservableCollection<CalculationResult>();

            // Создаем новую историю вычислений
            _history = new CalculationHistory();

            // Загружаем настройки
            LoadSettingsAsync();

            // Инициализируем команды
            InitializeCommands();

            // Подписываемся на события сервиса интеграции
            _integrationService.CalculationCompleted += OnCalculationCompleted;
        }

        private void InitializeCommands()
        {
            NumberCommand = new RelayCommand<string>(OnNumberPressed); // Команды ввода чисел - передаем параметр (цифру) в метод

            OperatorCommand = new RelayCommand<string>(OnOperatorPressed); // Команды операторов

            CalculateCommand = new AsyncRelayCommand(OnCalculateAsync); // Команда вычисления - асинхронная, так как может занять время

            // Команды управления
            ClearCommand = new RelayCommand(OnClear);
            BackspaceCommand = new RelayCommand(OnBackspace);
            DecimalCommand = new RelayCommand(OnDecimalPressed);

            // Научные функции - каждая своя команда для удобства
            ]SinCommand = new RelayCommand(() => OnScientificFunction("sin"));
            CosCommand = new RelayCommand(() => OnScientificFunction("cos"));
            TanCommand = new RelayCommand(() => OnScientificFunction("tan"));
            LogCommand = new RelayCommand(() => OnScientificFunction("log"));
            LnCommand = new RelayCommand(() => OnScientificFunction("ln"));
            SqrtCommand = new RelayCommand(() => OnScientificFunction("sqrt"));
            PowerCommand = new RelayCommand(() => OnOperatorPressed("^"));
            FactorialCommand = new RelayCommand(() => OnScientificFunction("fact"));

            // Константы
            PiCommand = new RelayCommand(() => OnConstantPressed("π"));
            ECommand = new RelayCommand(() => OnConstantPressed("e"));

            // Команды управления интерфейсом
            ToggleHistoryCommand = new RelayCommand(OnToggleHistory);
            ClearHistoryCommand = new RelayCommand(OnClearHistory);
            ToggleAngleModeCommand = new RelayCommand(OnToggleAngleMode);
            CopyResultCommand = new RelayCommand(OnCopyResult);
            InsertFromHistoryCommand = new RelayCommand<CalculationResult>(OnInsertFromHistory);
        }
        
        private void OnNumberPressed(string? number) 
        {
            if (string.IsNullOrEmpty(number)) return;

            // Если только что был выполнен расчет, очищаем дисплей для нового числа
            if (_justCalculated)
            {
                DisplayText = "0";
                _justCalculated = false;
            }

            // Если дисплей показывает "0" или мы ждем операнд, заменяем содержимое
            if (DisplayText == "0" || _waitingForOperand)
            {
                DisplayText = number;
                _waitingForOperand = false;
            }
            else
            {
                // Иначе добавляем цифру к существующему числу
                DisplayText += number;
            }

            // Очищаем ошибку, если она была
            ClearError();
        }

        // Обработчик нажатия операторов (+, -, *, /)
        private void OnOperatorPressed(string? op)
        {
            if (string.IsNullOrEmpty(op)) return;

            // Если мы не ждем операнд и выражение не пустое, можно добавить оператор
            if (!_waitingForOperand && !string.IsNullOrEmpty(DisplayText))
            {
                // Добавляем пробелы вокруг оператора для читаемости
                DisplayText += $" {op} ";
                _waitingForOperand = true;
                _justCalculated = false;
            }

            ClearError();
        }

        // Асинхронный обработчик вычисления - может занять время для сложных операций
        private async Task OnCalculateAsync()
        {
            try
            {
                // Показываем индикатор загрузки
                IsCalculating = true;
                ClearError();

                // Если нечего вычислять, выходим
                if (string.IsNullOrWhiteSpace(DisplayText) || DisplayText == "0")
                    return;

                // Выполняем вычисление через сервис
                var result = await _calculationService.EvaluateAsync(DisplayText);

                // Обрабатываем результат
                if (result.HasError)
                {
                    // Показываем ошибку пользователю
                    ShowError(result.ErrorMessage);
                }
                else
                {
                    HistoryText = DisplayText;// Обновляем историю для отображения

                    DisplayText = result.FormattedResult; // Показываем результат
         
                    AddToHistory(result); // Добавляем в историю

                    // Устанавливаем флаги состояния
                    _justCalculated = true;
                    _waitingForOperand = false;
                 
                    _integrationService.NotifyCalculationCompleted(result);// Уведомляем сервис интеграции о завершении вычисления
                }
            }
            catch (Exception ex)
            {
                // Обрабатываем неожиданные ошибки
                ShowError($"Ошибка вычисления: {ex.Message}");
            }
            finally
            {
                // Всегда скрываем индикатор загрузки
                IsCalculating = false;
            }
        }

        // Очистка дисплея
        private void OnClear()
        {
            DisplayText = "0";
            HistoryText = "";
            _justCalculated = false;
            _waitingForOperand = false;
            ClearError();
        }

        // Удаление последнего символа
        private void OnBackspace()
        {
            // Если только что вычисляли, Backspace работает как Clear
            if (_justCalculated)
            {
                OnClear();
                return;
            }

            // Удаляем последний символ
            if (DisplayText.Length > 1)
            {
                DisplayText = DisplayText[..^1];
            }
            else
            {
                DisplayText = "0";
            }

            ClearError();
        }

        // Ввод десятичной точки
        private void OnDecimalPressed()
        {
            // Если только что вычисляли, начинаем новое число
            if (_justCalculated)
            {
                DisplayText = "0.";
                _justCalculated = false;
                return;
            }

            // Если ждем операнд, начинаем новое десятичное число
            if (_waitingForOperand)
            {
                DisplayText += "0.";
                _waitingForOperand = false;
                return;
            }

            // Проверяем, нет ли уже десятичной точки в текущем числе
            string[] parts = DisplayText.Split(' ');
            string lastPart = parts[^1];

            if (!lastPart.Contains('.'))
            {
                DisplayText += ".";
            }

            ClearError();
        }

        // Обработчик научных функций
        private void OnScientificFunction(string function)
        {
            // Добавляем функцию с открывающей скобкой
            if (_justCalculated || DisplayText == "0")
            {
                DisplayText = $"{function}(";
            }
            else
            {
                DisplayText += $"{function}(";
            }

            _justCalculated = false;
            _waitingForOperand = false;
            ClearError();
        }

        // Обработчик констант (π, e)
        private void OnConstantPressed(string constant)
        {
            if (_justCalculated || DisplayText == "0" || _waitingForOperand)
            {
                DisplayText = constant;
                _waitingForOperand = false;
            }
            else
            {
                DisplayText += constant;
            }

            _justCalculated = false;
            ClearError();
        }

        // Переключение отображения истории
        private void OnToggleHistory()
        {
            ShowHistory = !ShowHistory;
        }

        // Очистка истории
        private void OnClearHistory()
        {
            _history.Clear();
            HistoryItems.Clear();
            RecentResults.Clear();
        }

        // Переключение режима углов (градусы/радианы)
        private void OnToggleAngleMode()
        {
            AngleMode = AngleMode == AngleMode.Degrees ? AngleMode.Radians : AngleMode.Degrees;

            // Сохраняем настройку
            if (_settings != null)
            {
                _settings.AngleMode = AngleMode;
                SaveSettingsAsync();
            }
        }

        // Копирование результата в буфер обмена
        private async void OnCopyResult()
        {
            try
            {
                // Используем встроенный API .NET MAUI для работы с буфером обмена
                await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.SetTextAsync(DisplayText);

                // Можно показать всплывающее уведомление
                // await Shell.Current.DisplayAlert("Скопировано", "Результат скопирован в буфер обмена", "OK");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка копирования: {ex.Message}");
            }
        }

        // Вставка результата из истории
        private void OnInsertFromHistory(CalculationResult? result)
        {
            if (result == null) return;

            DisplayText = result.FormattedResult;
            _justCalculated = true;
            _waitingForOperand = false;
            ClearError();
        }

        // Добавление результата в историю
        private void AddToHistory(CalculationResult result)
        {
            // Добавляем в модель истории
            _history.AddCalculation(result);

            // Обновляем коллекции для UI
            HistoryItems.Insert(0, result);

            // Ограничиваем количество элементов в UI коллекции для производительности
            while (HistoryItems.Count > 50)
            {
                HistoryItems.RemoveAt(HistoryItems.Count - 1);
            }

            // Обновляем последние результаты
            UpdateRecentResults();
        }

        // Обновление коллекции последних результатов
        private void UpdateRecentResults()
        {
            RecentResults.Clear();
            var recent = _history.GetSuccessful(10);

            foreach (var result in recent)
            {
                RecentResults.Add(result);
            }
        }

        // Показ ошибки пользователю
        private void ShowError(string message)
        {
            ErrorMessage = message;
            HasError = true;

            // Автоматически скрываем ошибку через 5 секунд
            Task.Delay(5000).ContinueWith(_ => ClearError());
        }

        // Очистка ошибки
        private void ClearError()
        {
            ErrorMessage = "";
            HasError = false;
        }

        // Асинхронная загрузка настроек
        private async void LoadSettingsAsync()
        {
            try
            {
                _settings = await _integrationService.GetCalculatorSettingsAsync();
                AngleMode = _settings.AngleMode;
                ShowScientificFunctions = true; // Всегда показываем в полном режиме
            }
            catch (Exception ex)
            {
                // Используем настройки по умолчанию при ошибке
                _settings = new CalculatorSettings();
                ShowError($"Ошибка загрузки настроек: {ex.Message}");
            }
        }

        // Асинхронное сохранение настроек
        private async void SaveSettingsAsync()
        {
            try
            {
                if (_settings != null)
                {
                    await _integrationService.SaveCalculatorSettingsAsync(_settings);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка сохранения настроек: {ex.Message}");
            }
        }

        // Обработчик события завершения вычисления от сервиса интеграции
        private void OnCalculationCompleted(object? sender, CalculationCompletedEventArgs e)
        {
            // Здесь можно добавить дополнительную логику
            // например, автоматическую вставку результата в заметку
        }

        // Метод для вставки текста извне (например, из заметок)
        public void InsertText(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            if (_justCalculated || DisplayText == "0")
            {
                DisplayText = text;
            }
            else
            {
                DisplayText += text;
            }

            _justCalculated = false;
            _waitingForOperand = false;
            ClearError();
        }

        // Метод для получения текущего результата
        public string GetCurrentResult()
        {
            return DisplayText;
        }

        // Метод для установки режима калькулятора
        public async Task SetModeAsync(CalculatorMode mode)
        {
            CurrentMode = mode;
            await _integrationService.SetModeAsync(mode);

            // Настраиваем отображение в зависимости от режима
            ShowScientificFunctions = mode == CalculatorMode.FullScreen;
        }

        // Метод для отписки от событий при уничтожении ViewModel
        public void Dispose()
        {
            _integrationService.CalculationCompleted -= OnCalculationCompleted;
        }
    }
}

