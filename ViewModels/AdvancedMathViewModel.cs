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
    // ViewModel для продвинутых математических операций (интегралы, пределы, ряды)
    public partial class AdvancedMathViewModel : ObservableObject
    {
        #region Private Fields

        // Сервисы для продвинутых математических операций
        private readonly IAdvancedMathService _advancedMathService;
        private readonly ICalculatorIntegrationService _integrationService;

        #endregion

        #region Observable Properties - Общие

        // Выбранный тип операции (интеграл, предел, ряд, производная)
        [ObservableProperty]
        private string selectedOperationType = "Интеграл";

        // Состояние вычислений
        [ObservableProperty]
        private bool isCalculating = false;

        [ObservableProperty]
        private string statusMessage = "Готов к вычислениям";

        [ObservableProperty]
        private bool hasError = false;

        [ObservableProperty]
        private string errorMessage = "";

        // Результат последней операции
        [ObservableProperty]
        private string operationResult = "";

        // Детальная информация о результате
        [ObservableProperty]
        private string resultDetails = "";

        #endregion

        #region Observable Properties - Интегралы

        [ObservableProperty]
        private string integralFunction = "x^2";

        [ObservableProperty]
        private double integralLowerLimit = 0;

        [ObservableProperty]
        private double integralUpperLimit = 1;

        [ObservableProperty]
        private IntegrationMethod integralMethod = IntegrationMethod.Adaptive;

        [ObservableProperty]
        private int integralSteps = 1000;

        #endregion

        #region Observable Properties - Пределы

        [ObservableProperty]
        private string limitFunction = "sin(x)/x";

        [ObservableProperty]
        private string limitVariable = "x";

        [ObservableProperty]
        private double limitApproachingValue = 0;

        [ObservableProperty]
        private bool limitToInfinity = false;

        [ObservableProperty]
        private ApproachType limitApproachType = ApproachType.TwoSided;

        #endregion

        #region Observable Properties - Ряды

        [ObservableProperty]
        private string seriesFormula = "1/n^2";

        [ObservableProperty]
        private string seriesVariable = "n";

        [ObservableProperty]
        private int seriesStartIndex = 1;

        [ObservableProperty]
        private int? seriesEndIndex = null;

        [ObservableProperty]
        private bool seriesIsInfinite = true;

        [ObservableProperty]
        private int seriesMaxTerms = 10000;

        #endregion

        #region Observable Properties - Производные

        [ObservableProperty]
        private string derivativeFunction = "x^3 + 2*x^2 + x + 1";

        [ObservableProperty]
        private string derivativeVariable = "x";

        [ObservableProperty]
        private double derivativePoint = 1;

        [ObservableProperty]
        private bool useSymbolicDerivative = false;

        #endregion

        #region Collections

        // Доступные типы операций
        public ObservableCollection<string> OperationTypes { get; private set; }

        // Методы интегрирования
        public ObservableCollection<IntegrationMethod> IntegrationMethods { get; private set; }

        // Типы приближения для пределов
        public ObservableCollection<ApproachType> ApproachTypes { get; private set; }

        // История вычислений
        public ObservableCollection<CalculationResult> CalculationHistory { get; private set; }

        // Примеры функций для быстрого выбора
        public ObservableCollection<string> FunctionExamples { get; private set; }

        #endregion

        #region Commands

        // Основные команды вычислений
        public ICommand CalculateIntegralCommand { get; private set; }
        public ICommand CalculateLimitCommand { get; private set; }
        public ICommand CalculateSeriesCommand { get; private set; }
        public ICommand CalculateDerivativeCommand { get; private set; }

        // Команды управления
        public ICommand ValidateFunctionCommand { get; private set; }
        public ICommand ClearHistoryCommand { get; private set; }
        public ICommand CopyResultCommand { get; private set; }
        public ICommand ExportResultCommand { get; private set; }
        public ICommand LoadExampleCommand { get; private set; }

        // Команды для построения графиков функций
        public ICommand PlotFunctionCommand { get; private set; }
        public ICommand GenerateFunctionTableCommand { get; private set; }

        #endregion

        #region Constructor

        public AdvancedMathViewModel(
            IAdvancedMathService advancedMathService,
            ICalculatorIntegrationService integrationService)
        {
            _advancedMathService = advancedMathService ?? throw new ArgumentNullException(nameof(advancedMathService));
            _integrationService = integrationService ?? throw new ArgumentNullException(nameof(integrationService));

            // Инициализируем коллекции
            InitializeCollections();

            // Инициализируем команды
            InitializeCommands();

            // Устанавливаем начальное состояние
            StatusMessage = "Выберите тип операции и введите параметры";
        }

        #endregion

        #region Initialization

        private void InitializeCollections()
        {
            OperationTypes = new ObservableCollection<string>
            {
                "Интеграл", "Предел", "Ряд", "Производная", "Решение уравнений"
            };

            IntegrationMethods = new ObservableCollection<IntegrationMethod>
            {
                IntegrationMethod.Adaptive,
                IntegrationMethod.Trapezoidal,
                IntegrationMethod.Simpson,
                IntegrationMethod.Gaussian
            };

            ApproachTypes = new ObservableCollection<ApproachType>
            {
                ApproachType.TwoSided,
                ApproachType.LeftSided,
                ApproachType.RightSided
            };

            CalculationHistory = new ObservableCollection<CalculationResult>();

            FunctionExamples = new ObservableCollection<string>
            {
                // Примеры для интегралов
                "x^2", "sin(x)", "cos(x)", "exp(x)", "1/x", "sqrt(x)",
                
                // Примеры для пределов
                "sin(x)/x", "(1+x)^(1/x)", "exp(-x)", "x*sin(1/x)",
                
                // Примеры для рядов
                "1/n^2", "1/n!", "(-1)^n/n", "1/(2^n)",
                
                // Примеры для производных
                "x^3 + 2*x^2 + x + 1", "sin(x)*cos(x)", "exp(x^2)", "ln(x)"
            };
        }

        private void InitializeCommands()
        {
            // Основные команды вычислений
            CalculateIntegralCommand = new AsyncRelayCommand(OnCalculateIntegralAsync);
            CalculateLimitCommand = new AsyncRelayCommand(OnCalculateLimitAsync);
            CalculateSeriesCommand = new AsyncRelayCommand(OnCalculateSeriesAsync);
            CalculateDerivativeCommand = new AsyncRelayCommand(OnCalculateDerivativeAsync);

            // Команды управления
            ValidateFunctionCommand = new AsyncRelayCommand<string>(OnValidateFunctionAsync);
            ClearHistoryCommand = new RelayCommand(OnClearHistory);
            CopyResultCommand = new AsyncRelayCommand(OnCopyResultAsync);
            ExportResultCommand = new AsyncRelayCommand(OnExportResultAsync);
            LoadExampleCommand = new RelayCommand<string>(OnLoadExample);

            // Команды для анализа функций
            PlotFunctionCommand = new AsyncRelayCommand(OnPlotFunctionAsync);
            GenerateFunctionTableCommand = new AsyncRelayCommand(OnGenerateFunctionTableAsync);
        }

        #endregion

        #region Calculation Commands

        // Вычисление интеграла
        private async Task OnCalculateIntegralAsync()
        {
            try
            {
                IsCalculating = true;
                StatusMessage = "Вычисляется интеграл...";
                ClearError();

                // Создаем объект интеграла
                var integral = new IntegralCalculation
                {
                    Function = IntegralFunction,
                    LowerLimit = IntegralLowerLimit,
                    UpperLimit = IntegralUpperLimit,
                    Method = IntegralMethod,
                    Steps = IntegralSteps
                };

                // Выполняем вычисление
                var result = await _advancedMathService.IntegrateAsync(integral);

                ProcessCalculationResult(result, "Интеграл");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка вычисления интеграла: {ex.Message}");
            }
            finally
            {
                IsCalculating = false;
            }
        }

        // Вычисление предела
        private async Task OnCalculateLimitAsync()
        {
            try
            {
                IsCalculating = true;
                StatusMessage = "Вычисляется предел...";
                ClearError();

                CalculationResult result;

                if (LimitToInfinity)
                {
                    // Предел в бесконечности
                    result = await _advancedMathService.CalculateLimitAtInfinityAsync(
                        LimitFunction, LimitVariable, LimitApproachingValue >= 0);
                }
                else
                {
                    // Обычный предел
                    result = await _advancedMathService.CalculateLimitAsync(
                        LimitFunction, LimitVariable, LimitApproachingValue, LimitApproachType);
                }

                ProcessCalculationResult(result, "Предел");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка вычисления предела: {ex.Message}");
            }
            finally
            {
                IsCalculating = false;
            }
        }

        // Вычисление суммы ряда
        private async Task OnCalculateSeriesAsync()
        {
            try
            {
                IsCalculating = true;
                StatusMessage = "Вычисляется сумма ряда...";
                ClearError();

                // Определяем конечный индекс
                int? endIndex = SeriesIsInfinite ? null : SeriesEndIndex;

                var result = await _advancedMathService.CalculateSeriesAsync(
                    SeriesFormula, SeriesVariable, SeriesStartIndex, endIndex, SeriesMaxTerms);

                ProcessCalculationResult(result, "Ряд");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка вычисления ряда: {ex.Message}");
            }
            finally
            {
                IsCalculating = false;
            }
        }

        // Вычисление производной
        private async Task OnCalculateDerivativeAsync()
        {
            try
            {
                IsCalculating = true;
                StatusMessage = "Вычисляется производная...";
                ClearError();

                CalculationResult result;

                if (UseSymbolicDerivative)
                {
                    // Символьная производная
                    result = await _advancedMathService.SymbolicDerivativeAsync(
                        DerivativeFunction, DerivativeVariable);
                }
                else
                {
                    // Численная производная в точке
                    result = await _advancedMathService.DerivativeAsync(
                        DerivativeFunction, DerivativeVariable, DerivativePoint);
                }

                ProcessCalculationResult(result, "Производная");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка вычисления производной: {ex.Message}");
            }
            finally
            {
                IsCalculating = false;
            }
        }

        #endregion

        #region Utility Commands

        // Валидация функции перед вычислением
        private async Task OnValidateFunctionAsync(string? function)
        {
            if (string.IsNullOrWhiteSpace(function)) return;

            try
            {
                StatusMessage = "Проверяется функция...";

                // Определяем переменную в зависимости от текущей операции
                string variable = SelectedOperationType switch
                {
                    "Интеграл" => "x",
                    "Предел" => LimitVariable,
                    "Ряд" => SeriesVariable,
                    "Производная" => DerivativeVariable,
                    _ => "x"
                };

                bool isValid = _advancedMathService.IsValidFunction(function, variable);

                if (isValid)
                {
                    StatusMessage = "✓ Функция корректна";
                    ClearError();
                }
                else
                {
                    ShowError("Функция содержит ошибки");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка валидации: {ex.Message}");
            }
        }

        // Очистка истории
        private void OnClearHistory()
        {
            CalculationHistory.Clear();
            StatusMessage = "История очищена";
        }

        // Копирование результата
        private async Task OnCopyResultAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(OperationResult))
                {
                    ShowError("Нет результата для копирования");
                    return;
                }

                string textToCopy = OperationResult;
                if (!string.IsNullOrEmpty(ResultDetails))
                {
                    textToCopy += "\n\nДетали:\n" + ResultDetails;
                }

                await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.SetTextAsync(textToCopy);
                StatusMessage = "Результат скопирован в буфер обмена";
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка копирования: {ex.Message}");
            }
        }

        // Экспорт результата в различные форматы
        private async Task OnExportResultAsync()
        {
            try
            {
                if (CalculationHistory.Count == 0)
                {
                    ShowError("Нет результатов для экспорта");
                    return;
                }

                // Экспортируем последние 10 результатов в формате Markdown
                var recentResults = new CalculationResult[Math.Min(10, CalculationHistory.Count)];
                for (int i = 0; i < recentResults.Length; i++)
                {
                    recentResults[i] = CalculationHistory[i];
                }

                var exportText = await _integrationService.ExportCalculationsAsync(
                    recentResults, ExportFormat.Markdown);

                await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.SetTextAsync(exportText);
                StatusMessage = "Результаты экспортированы в Markdown и скопированы в буфер";
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка экспорта: {ex.Message}");
            }
        }

        // Загрузка примера функции
        private void OnLoadExample(string? example)
        {
            if (string.IsNullOrEmpty(example)) return;

            // Загружаем пример в соответствующее поле в зависимости от текущей операции
            switch (SelectedOperationType)
            {
                case "Интеграл":
                    IntegralFunction = example;
                    break;
                case "Предел":
                    LimitFunction = example;
                    break;
                case "Ряд":
                    SeriesFormula = example;
                    break;
                case "Производная":
                    DerivativeFunction = example;
                    break;
            }

            StatusMessage = $"Загружен пример: {example}";
        }

        // Построение графика функции (заглушка для будущей реализации)
        private async Task OnPlotFunctionAsync()
        {
            try
            {
                StatusMessage = "Построение графика функции...";

                // Определяем функцию для построения
                string function = SelectedOperationType switch
                {
                    "Интеграл" => IntegralFunction,
                    "Предел" => LimitFunction,
                    "Производная" => DerivativeFunction,
                    _ => "x^2"
                };

                // Генерируем таблицу значений для графика
                var result = await _advancedMathService.GenerateFunctionTableAsync(
                    function, "x", -10, 10, 0.5);

                if (result.HasError)
                {
                    ShowError(result.ErrorMessage);
                    return;
                }

                // Здесь должно быть построение графика
                // В реальном приложении можно использовать библиотеки типа OxyPlot или Syncfusion Charts
                StatusMessage = "График построен (функция будет реализована позже)";
                ResultDetails = result.FormattedResult;
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка построения графика: {ex.Message}");
            }
        }

        // Генерация таблицы значений функции
        private async Task OnGenerateFunctionTableAsync()
        {
            try
            {
                IsCalculating = true;
                StatusMessage = "Генерируется таблица значений...";

                string function = SelectedOperationType switch
                {
                    "Интеграл" => IntegralFunction,
                    "Предел" => LimitFunction,
                    "Производная" => DerivativeFunction,
                    _ => "x^2"
                };

                string variable = SelectedOperationType switch
                {
                    "Предел" => LimitVariable,
                    "Производная" => DerivativeVariable,
                    _ => "x"
                };

                // Генерируем таблицу от -5 до 5 с шагом 0.5
                var result = await _advancedMathService.GenerateFunctionTableAsync(
                    function, variable, -5, 5, 0.5);

                ProcessCalculationResult(result, "Таблица функции");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка генерации таблицы: {ex.Message}");
            }
            finally
            {
                IsCalculating = false;
            }
        }

        #endregion

        #region Helper Methods

        // Обработка результата вычисления
        private void ProcessCalculationResult(CalculationResult result, string operationType)
        {
            if (result.HasError)
            {
                ShowError(result.ErrorMessage);
                return;
            }

            // Обновляем отображение результата
            OperationResult = $"{operationType}: {result.FormattedResult}";
            ResultDetails = result.Details;

            // Добавляем в историю
            AddToHistory(result);

            StatusMessage = $"{operationType} вычислен успешно";
            ClearError();

            // Уведомляем сервис интеграции
            _integrationService.NotifyCalculationCompleted(result);
        }

        // Добавление в историю
        private void AddToHistory(CalculationResult result)
        {
            CalculationHistory.Insert(0, result);

            // Ограничиваем размер истории
            while (CalculationHistory.Count > 50)
            {
                CalculationHistory.RemoveAt(CalculationHistory.Count - 1);
            }
        }

        // Показ ошибки
        private void ShowError(string message)
        {
            ErrorMessage = message;
            HasError = true;
            StatusMessage = "Ошибка: " + message;

            // Автоматически скрываем через 5 секунд
            Task.Delay(5000).ContinueWith(_ => ClearError());
        }

        // Очистка ошибки
        private void ClearError()
        {
            ErrorMessage = "";
            HasError = false;
        }

        #endregion

        #region Public Methods

        // Установка функции извне
        public void SetFunction(string function, string operationType = "")
        {
            if (!string.IsNullOrEmpty(operationType))
            {
                SelectedOperationType = operationType;
            }

            switch (SelectedOperationType)
            {
                case "Интеграл":
                    IntegralFunction = function;
                    break;
                case "Предел":
                    LimitFunction = function;
                    break;
                case "Ряд":
                    SeriesFormula = function;
                    break;
                case "Производная":
                    DerivativeFunction = function;
                    break;
            }

            StatusMessage = $"Функция установлена: {function}";
        }

        // Получение текущей функции
        public string GetCurrentFunction()
        {
            return SelectedOperationType switch
            {
                "Интеграл" => IntegralFunction,
                "Предел" => LimitFunction,
                "Ряд" => SeriesFormula,
                "Производная" => DerivativeFunction,
                _ => ""
            };
        }

        // Быстрое вычисление с параметрами по умолчанию
        public async Task<CalculationResult?> QuickCalculateAsync(string function, string operationType)
        {
            try
            {
                SetFunction(function, operationType);

                return operationType switch
                {
                    "Интеграл" => await _advancedMathService.IntegrateAsync(function, 0, 1),
                    "Предел" => await _advancedMathService.CalculateLimitAsync(function, "x", 0),
                    "Ряд" => await _advancedMathService.CalculateSeriesAsync(function, "n", 1, null, 1000),
                    "Производная" => await _advancedMathService.DerivativeAsync(function, "x", 0),
                    _ => null
                };
            }
            catch
            {
                return null;
            }
        }

        // Проверка корректности текущих параметров
        public bool ValidateCurrentParameters()
        {
            switch (SelectedOperationType)
            {
                case "Интеграл":
                    return !string.IsNullOrWhiteSpace(IntegralFunction) &&
                           IntegralLowerLimit < IntegralUpperLimit;

                case "Предел":
                    return !string.IsNullOrWhiteSpace(LimitFunction) &&
                           !string.IsNullOrWhiteSpace(LimitVariable);

                case "Ряд":
                    return !string.IsNullOrWhiteSpace(SeriesFormula) &&
                           !string.IsNullOrWhiteSpace(SeriesVariable) &&
                           (!SeriesEndIndex.HasValue || SeriesEndIndex > SeriesStartIndex);

                case "Производная":
                    return !string.IsNullOrWhiteSpace(DerivativeFunction) &&
                           !string.IsNullOrWhiteSpace(DerivativeVariable);

                default:
                    return false;
            }
        }

        #endregion

        #region Property Change Handlers

        // Автоматическая валидация при изменении типа операции
        partial void OnSelectedOperationTypeChanged(string value)
        {
            // Обновляем статус при смене операции
            StatusMessage = $"Выбрана операция: {value}";

            // Очищаем предыдущие ошибки
            ClearError();

            // Можно добавить специфичную логику для каждого типа операции
            switch (value)
            {
                case "Интеграл":
                    StatusMessage += ". Введите функцию и пределы интегрирования";
                    break;
                case "Предел":
                    StatusMessage += ". Введите функцию и точку приближения";
                    break;
                case "Ряд":
                    StatusMessage += ". Введите формулу общего члена ряда";
                    break;
                case "Производная":
                    StatusMessage += ". Введите функцию для дифференцирования";
                    break;
            }
        }

        // Автоматическая проверка при изменении флага бесконечности ряда
        partial void OnSeriesIsInfiniteChanged(bool value)
        {
            if (value)
            {
                SeriesEndIndex = null;
                StatusMessage = "Ряд установлен как бесконечный";
            }
            else
            {
                SeriesEndIndex = SeriesStartIndex + 100; // Значение по умолчанию
                StatusMessage = "Ряд установлен как конечный";
            }
        }

        // Проверка корректности пределов интегрирования
        partial void OnIntegralLowerLimitChanged(double value)
        {
            if (value >= IntegralUpperLimit)
            {
                ShowError("Нижний предел должен быть меньше верхнего");
            }
            else
            {
                ClearError();
            }
        }

        partial void OnIntegralUpperLimitChanged(double value)
        {
            if (value <= IntegralLowerLimit)
            {
                ShowError("Верхний предел должен быть больше нижнего");
            }
            else
            {
                ClearError();
            }
        }

        #endregion

        #region Events

        // События для уведомления о завершении операций
        public event EventHandler<CalculationResult>? CalculationCompleted;
        public event EventHandler<string>? FunctionValidated;

        #endregion

        #region Cleanup

        public void Dispose()
        {
            // Очистка ресурсов
            CalculationHistory.Clear();
            FunctionExamples.Clear();
        }

        #endregion
    }
}