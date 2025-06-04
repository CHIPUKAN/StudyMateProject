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
    // ViewModel для калькулятора матриц - специализированный интерфейс для матричных операций
    public partial class MatrixCalculatorViewModel : ObservableObject
    {
        #region Private Fields

        // Сервисы для работы с матрицами и интеграции
        private readonly IMatrixService _matrixService;
        private readonly ICalculatorIntegrationService _integrationService;

        // Текущие рабочие матрицы
        private MatrixModel? _matrixA;
        private MatrixModel? _matrixB;
        private MatrixModel? _resultMatrix;

        #endregion

        #region Observable Properties

        // Отображаемые матрицы для UI
        [ObservableProperty]
        private MatrixModel? matrixA;

        [ObservableProperty]
        private MatrixModel? matrixB;

        [ObservableProperty]
        private MatrixModel? resultMatrix;

        // Размеры для создания новых матриц
        [ObservableProperty]
        private int matrixARows = 3;

        [ObservableProperty]
        private int matrixAColumns = 3;

        [ObservableProperty]
        private int matrixBRows = 3;

        [ObservableProperty]
        private int matrixBColumns = 3;

        // Выбранная операция
        [ObservableProperty]
        private string selectedOperation = "Сложение";

        // Скаляр для умножения на число
        [ObservableProperty]
        private double scalarValue = 1.0;

        // Состояние вычислений
        [ObservableProperty]
        private bool isCalculating = false;

        [ObservableProperty]
        private string statusMessage = "Готов к работе";

        [ObservableProperty]
        private bool hasError = false;

        [ObservableProperty]
        private string errorMessage = "";

        // Результат последней операции в текстовом виде
        [ObservableProperty]
        private string operationResult = "";

        // Детали операции (например, для разложений)
        [ObservableProperty]
        private string operationDetails = "";

        #endregion

        #region Collections

        // Доступные операции для выбора в UI
        public ObservableCollection<string> AvailableOperations { get; private set; }

        // История матричных операций
        public ObservableCollection<CalculationResult> MatrixHistory { get; private set; }

        // Сохраненные матрицы для быстрого доступа
        public ObservableCollection<MatrixModel> SavedMatrices { get; private set; }

        #endregion

        #region Commands

        // Команды для управления матрицами
        public ICommand CreateMatrixACommand { get; private set; }
        public ICommand CreateMatrixBCommand { get; private set; }
        public ICommand FillRandomACommand { get; private set; }
        public ICommand FillRandomBCommand { get; private set; }
        public ICommand ClearMatrixACommand { get; private set; }
        public ICommand ClearMatrixBCommand { get; private set; }
        public ICommand IdentityMatrixACommand { get; private set; }
        public ICommand IdentityMatrixBCommand { get; private set; }

        // Команды для операций
        public ICommand PerformOperationCommand { get; private set; }
        public ICommand AddMatricesCommand { get; private set; }
        public ICommand SubtractMatricesCommand { get; private set; }
        public ICommand MultiplyMatricesCommand { get; private set; }
        public ICommand MultiplyByScalarCommand { get; private set; }

        // Команды для операций с одной матрицей
        public ICommand TransposeACommand { get; private set; }
        public ICommand TransposeBCommand { get; private set; }
        public ICommand InverseACommand { get; private set; }
        public ICommand InverseBCommand { get; private set; }
        public ICommand DeterminantACommand { get; private set; }
        public ICommand DeterminantBCommand { get; private set; }
        public ICommand TraceACommand { get; private set; }
        public ICommand TraceBCommand { get; private set; }

        // Команды для продвинутых операций
        public ICommand LUDecompositionCommand { get; private set; }
        public ICommand QRDecompositionCommand { get; private set; }
        public ICommand EigenvaluesCommand { get; private set; }
        public ICommand EigenvectorsCommand { get; private set; }

        // Команды для сохранения и загрузки
        public ICommand SaveMatrixCommand { get; private set; }
        public ICommand LoadMatrixCommand { get; private set; }
        public ICommand ExportResultCommand { get; private set; }

        // Команды управления
        public ICommand ClearHistoryCommand { get; private set; }
        public ICommand CopyResultCommand { get; private set; }

        #endregion

        #region Constructor

        public MatrixCalculatorViewModel(
            IMatrixService matrixService,
            ICalculatorIntegrationService integrationService)
        {
            _matrixService = matrixService ?? throw new ArgumentNullException(nameof(matrixService));
            _integrationService = integrationService ?? throw new ArgumentNullException(nameof(integrationService));

            // Инициализируем коллекции
            AvailableOperations = new ObservableCollection<string>
            {
                "Сложение", "Вычитание", "Умножение", "Умножение на скаляр",
                "Транспонирование", "Обращение", "Определитель", "След",
                "LU разложение", "QR разложение", "Собственные значения"
            };

            MatrixHistory = new ObservableCollection<CalculationResult>();
            SavedMatrices = new ObservableCollection<MatrixModel>();

            // Инициализируем команды
            InitializeCommands();

            // Создаем начальные матрицы
            CreateInitialMatrices();
        }

        #endregion

        #region Command Initialization

        private void InitializeCommands()
        {
            // Команды создания и управления матрицами
            CreateMatrixACommand = new RelayCommand(OnCreateMatrixA);
            CreateMatrixBCommand = new RelayCommand(OnCreateMatrixB);
            FillRandomACommand = new RelayCommand(OnFillRandomA);
            FillRandomBCommand = new RelayCommand(OnFillRandomB);
            ClearMatrixACommand = new RelayCommand(OnClearMatrixA);
            ClearMatrixBCommand = new RelayCommand(OnClearMatrixB);
            IdentityMatrixACommand = new RelayCommand(OnIdentityMatrixA);
            IdentityMatrixBCommand = new RelayCommand(OnIdentityMatrixB);

            // Операции с двумя матрицами
            PerformOperationCommand = new AsyncRelayCommand(OnPerformOperationAsync);
            AddMatricesCommand = new AsyncRelayCommand(OnAddMatricesAsync);
            SubtractMatricesCommand = new AsyncRelayCommand(OnSubtractMatricesAsync);
            MultiplyMatricesCommand = new AsyncRelayCommand(OnMultiplyMatricesAsync);
            MultiplyByScalarCommand = new AsyncRelayCommand(OnMultiplyByScalarAsync);

            // Операции с одной матрицей
            TransposeACommand = new AsyncRelayCommand(() => OnSingleMatrixOperationAsync("transpose", MatrixA));
            TransposeBCommand = new AsyncRelayCommand(() => OnSingleMatrixOperationAsync("transpose", MatrixB));
            InverseACommand = new AsyncRelayCommand(() => OnSingleMatrixOperationAsync("inverse", MatrixA));
            InverseBCommand = new AsyncRelayCommand(() => OnSingleMatrixOperationAsync("inverse", MatrixB));
            DeterminantACommand = new AsyncRelayCommand(() => OnSingleMatrixOperationAsync("determinant", MatrixA));
            DeterminantBCommand = new AsyncRelayCommand(() => OnSingleMatrixOperationAsync("determinant", MatrixB));
            TraceACommand = new AsyncRelayCommand(() => OnSingleMatrixOperationAsync("trace", MatrixA));
            TraceBCommand = new AsyncRelayCommand(() => OnSingleMatrixOperationAsync("trace", MatrixB));

            // Продвинутые операции
            LUDecompositionCommand = new AsyncRelayCommand(() => OnAdvancedOperationAsync("LU", MatrixA));
            QRDecompositionCommand = new AsyncRelayCommand(() => OnAdvancedOperationAsync("QR", MatrixA));
            EigenvaluesCommand = new AsyncRelayCommand(() => OnAdvancedOperationAsync("eigenvalues", MatrixA));
            EigenvectorsCommand = new AsyncRelayCommand(() => OnAdvancedOperationAsync("eigenvectors", MatrixA));

            // Сохранение и экспорт
            SaveMatrixCommand = new RelayCommand<MatrixModel>(OnSaveMatrix);
            LoadMatrixCommand = new RelayCommand<MatrixModel>(OnLoadMatrix);
            ExportResultCommand = new AsyncRelayCommand(OnExportResultAsync);

            // Управление
            ClearHistoryCommand = new RelayCommand(OnClearHistory);
            CopyResultCommand = new AsyncRelayCommand(OnCopyResultAsync);
        }

        #endregion

        #region Matrix Management Commands

        // Создание новой матрицы A с заданными размерами
        private void OnCreateMatrixA()
        {
            try
            {
                MatrixA = new MatrixModel(MatrixARows, MatrixAColumns, "A");
                StatusMessage = $"Создана матрица A ({MatrixARows}×{MatrixAColumns})";
                ClearError();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка создания матрицы A: {ex.Message}");
            }
        }

        private void OnCreateMatrixB()
        {
            try
            {
                MatrixB = new MatrixModel(MatrixBRows, MatrixBColumns, "B");
                StatusMessage = $"Создана матрица B ({MatrixBRows}×{MatrixBColumns})";
                ClearError();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка создания матрицы B: {ex.Message}");
            }
        }

        // Заполнение случайными числами
        private void OnFillRandomA()
        {
            MatrixA?.FillRandom(-10, 10);
            StatusMessage = "Матрица A заполнена случайными числами";
        }

        private void OnFillRandomB()
        {
            MatrixB?.FillRandom(-10, 10);
            StatusMessage = "Матрица B заполнена случайными числами";
        }

        // Очистка матриц
        private void OnClearMatrixA()
        {
            MatrixA?.Clear();
            StatusMessage = "Матрица A очищена";
        }

        private void OnClearMatrixB()
        {
            MatrixB?.Clear();
            StatusMessage = "Матрица B очищена";
        }

        // Создание единичных матриц
        private void OnIdentityMatrixA()
        {
            if (MatrixA?.IsSquare == true)
            {
                MatrixA.MakeIdentity();
                StatusMessage = "Матрица A преобразована в единичную";
            }
            else
            {
                ShowError("Единичная матрица должна быть квадратной");
            }
        }

        private void OnIdentityMatrixB()
        {
            if (MatrixB?.IsSquare == true)
            {
                MatrixB.MakeIdentity();
                StatusMessage = "Матрица B преобразована в единичную";
            }
            else
            {
                ShowError("Единичная матрица должна быть квадратной");
            }
        }

        #endregion

        #region Matrix Operations

        // Универсальный обработчик операций на основе выбранной операции
        private async Task OnPerformOperationAsync()
        {
            switch (SelectedOperation)
            {
                case "Сложение":
                    await OnAddMatricesAsync();
                    break;
                case "Вычитание":
                    await OnSubtractMatricesAsync();
                    break;
                case "Умножение":
                    await OnMultiplyMatricesAsync();
                    break;
                case "Умножение на скаляр":
                    await OnMultiplyByScalarAsync();
                    break;
                case "Транспонирование":
                    await OnSingleMatrixOperationAsync("transpose", MatrixA);
                    break;
                case "Обращение":
                    await OnSingleMatrixOperationAsync("inverse", MatrixA);
                    break;
                case "Определитель":
                    await OnSingleMatrixOperationAsync("determinant", MatrixA);
                    break;
                case "След":
                    await OnSingleMatrixOperationAsync("trace", MatrixA);
                    break;
                default:
                    ShowError("Неизвестная операция");
                    break;
            }
        }

        // Сложение матриц
        private async Task OnAddMatricesAsync()
        {
            if (!ValidateMatricesForOperation(MatrixA, MatrixB, "addition")) return;

            try
            {
                IsCalculating = true;
                StatusMessage = "Выполняется сложение матриц...";

                var result = await _matrixService.AddMatricesAsync(MatrixA!, MatrixB!);
                ProcessOperationResult(result, "Сложение");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка сложения матриц: {ex.Message}");
            }
            finally
            {
                IsCalculating = false;
            }
        }

        // Вычитание матриц
        private async Task OnSubtractMatricesAsync()
        {
            if (!ValidateMatricesForOperation(MatrixA, MatrixB, "subtraction")) return;

            try
            {
                IsCalculating = true;
                StatusMessage = "Выполняется вычитание матриц...";

                var result = await _matrixService.SubtractMatricesAsync(MatrixA!, MatrixB!);
                ProcessOperationResult(result, "Вычитание");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка вычитания матриц: {ex.Message}");
            }
            finally
            {
                IsCalculating = false;
            }
        }

        // Умножение матриц
        private async Task OnMultiplyMatricesAsync()
        {
            if (!ValidateMatricesForOperation(MatrixA, MatrixB, "multiplication")) return;

            try
            {
                IsCalculating = true;
                StatusMessage = "Выполняется умножение матриц...";

                var result = await _matrixService.MultiplyMatricesAsync(MatrixA!, MatrixB!);
                ProcessOperationResult(result, "Умножение");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка умножения матриц: {ex.Message}");
            }
            finally
            {
                IsCalculating = false;
            }
        }

        // Умножение на скаляр
        private async Task OnMultiplyByScalarAsync()
        {
            if (MatrixA == null)
            {
                ShowError("Матрица A не создана");
                return;
            }

            try
            {
                IsCalculating = true;
                StatusMessage = $"Умножение матрицы на скаляр {ScalarValue}...";

                var result = await _matrixService.MultiplyByScalarAsync(MatrixA, ScalarValue);
                ProcessOperationResult(result, $"Умножение на {ScalarValue}");
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка умножения на скаляр: {ex.Message}");
            }
            finally
            {
                IsCalculating = false;
            }
        }

        // Универсальный обработчик операций с одной матрицей
        private async Task OnSingleMatrixOperationAsync(string operation, MatrixModel? matrix)
        {
            if (matrix == null)
            {
                ShowError("Матрица не выбрана");
                return;
            }

            try
            {
                IsCalculating = true;
                StatusMessage = $"Выполняется операция: {operation}...";

                CalculationResult result = operation switch
                {
                    "transpose" => await _matrixService.TransposeAsync(matrix),
                    "inverse" => await _matrixService.InverseAsync(matrix),
                    "determinant" => await _matrixService.DeterminantAsync(matrix),
                    "trace" => await _matrixService.TraceAsync(matrix),
                    _ => throw new ArgumentException($"Неизвестная операция: {operation}")
                };

                ProcessOperationResult(result, GetOperationDisplayName(operation));
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка выполнения операции {operation}: {ex.Message}");
            }
            finally
            {
                IsCalculating = false;
            }
        }

        // Обработчик продвинутых операций (разложения, собственные значения)
        private async Task OnAdvancedOperationAsync(string operation, MatrixModel? matrix)
        {
            if (matrix == null)
            {
                ShowError("Матрица не выбрана");
                return;
            }

            try
            {
                IsCalculating = true;
                StatusMessage = $"Выполняется {operation} разложение...";

                CalculationResult result = operation switch
                {
                    "LU" => await _matrixService.LUDecompositionAsync(matrix),
                    "QR" => await _matrixService.QRDecompositionAsync(matrix),
                    "eigenvalues" => await _matrixService.EigenvaluesAsync(matrix),
                    "eigenvectors" => await _matrixService.EigenvectorsAsync(matrix),
                    _ => throw new ArgumentException($"Неизвестная продвинутая операция: {operation}")
                };

                ProcessOperationResult(result, GetAdvancedOperationDisplayName(operation));
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка выполнения {operation}: {ex.Message}");
            }
            finally
            {
                IsCalculating = false;
            }
        }

        #endregion

        #region Save/Load/Export Commands

        // Сохранение матрицы в коллекцию
        private void OnSaveMatrix(MatrixModel? matrix)
        {
            if (matrix == null) return;

            try
            {
                // Создаем копию матрицы для сохранения
                var savedMatrix = matrix.Clone();
                savedMatrix.Name = $"Сохранено_{DateTime.Now:HH:mm:ss}";

                SavedMatrices.Add(savedMatrix);
                StatusMessage = $"Матрица {matrix.Name} сохранена";
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка сохранения матрицы: {ex.Message}");
            }
        }

        // Загрузка матрицы из сохраненных
        private void OnLoadMatrix(MatrixModel? savedMatrix)
        {
            if (savedMatrix == null) return;

            try
            {
                // Загружаем в матрицу A по умолчанию
                MatrixA = savedMatrix.Clone();
                MatrixA.Name = "A";

                // Обновляем размеры для UI
                MatrixARows = MatrixA.Rows;
                MatrixAColumns = MatrixA.Columns;

                StatusMessage = $"Матрица загружена в A";
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка загрузки матрицы: {ex.Message}");
            }
        }

        // Экспорт результата
        private async Task OnExportResultAsync()
        {
            if (ResultMatrix == null && string.IsNullOrEmpty(OperationResult))
            {
                ShowError("Нет результата для экспорта");
                return;
            }

            try
            {
                // Формируем текст для экспорта
                var exportText = "";

                if (ResultMatrix != null)
                {
                    exportText = MathFormatter.FormatMatrix(ResultMatrix);
                }
                else
                {
                    exportText = OperationResult;
                }

                if (!string.IsNullOrEmpty(OperationDetails))
                {
                    exportText += "\n\nДетали:\n" + OperationDetails;
                }

                // Копируем в буфер обмена
                await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.SetTextAsync(exportText);
                StatusMessage = "Результат скопирован в буфер обмена";
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка экспорта: {ex.Message}");
            }
        }

        #endregion

        #region Management Commands

        // Очистка истории
        private void OnClearHistory()
        {
            MatrixHistory.Clear();
            StatusMessage = "История очищена";
        }

        // Копирование результата
        private async Task OnCopyResultAsync()
        {
            try
            {
                if (ResultMatrix != null)
                {
                    var matrixText = MathFormatter.FormatMatrix(ResultMatrix);
                    await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.SetTextAsync(matrixText);
                }
                else if (!string.IsNullOrEmpty(OperationResult))
                {
                    await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.SetTextAsync(OperationResult);
                }
                else
                {
                    ShowError("Нет результата для копирования");
                    return;
                }

                StatusMessage = "Результат скопирован";
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка копирования: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        // Создание начальных матриц при запуске
        private void CreateInitialMatrices()
        {
            OnCreateMatrixA();
            OnCreateMatrixB();
        }

        // Валидация матриц перед операцией
        private bool ValidateMatricesForOperation(MatrixModel? matrixA, MatrixModel? matrixB, string operation)
        {
            if (matrixA == null)
            {
                ShowError("Матрица A не создана");
                return false;
            }

            if (matrixB == null)
            {
                ShowError("Матрица B не создана");
                return false;
            }

            // Проверяем совместимость для конкретной операции
            switch (operation)
            {
                case "addition":
                case "subtraction":
                    if (!_matrixService.CanAdd(matrixA, matrixB))
                    {
                        ShowError("Матрицы должны иметь одинаковые размеры для сложения/вычитания");
                        return false;
                    }
                    break;

                case "multiplication":
                    if (!_matrixService.CanMultiply(matrixA, matrixB))
                    {
                        ShowError("Количество столбцов матрицы A должно равняться количеству строк матрицы B");
                        return false;
                    }
                    break;
            }

            return true;
        }

        // Обработка результата операции
        private void ProcessOperationResult(CalculationResult result, string operationName)
        {
            if (result.HasError)
            {
                ShowError(result.ErrorMessage);
                return;
            }

            // Обновляем результат в зависимости от типа
            if (result.Result is MatrixModel matrix)
            {
                ResultMatrix = matrix;
                OperationResult = $"{operationName}: Матрица {MathFormatter.FormatMatrixSize(matrix.Rows, matrix.Columns)}";
            }
            else
            {
                ResultMatrix = null;
                OperationResult = $"{operationName}: {result.FormattedResult}";
            }

            OperationDetails = result.Details;

            // Добавляем в историю
            AddToHistory(result);

            StatusMessage = $"{operationName} выполнено успешно";
            ClearError();
        }

        // Добавление результата в историю
        private void AddToHistory(CalculationResult result)
        {
            MatrixHistory.Insert(0, result);

            // Ограничиваем размер истории
            while (MatrixHistory.Count > 20)
            {
                MatrixHistory.RemoveAt(MatrixHistory.Count - 1);
            }
        }

        // Получение отображаемого имени операции
        private string GetOperationDisplayName(string operation)
        {
            return operation switch
            {
                "transpose" => "Транспонирование",
                "inverse" => "Обращение",
                "determinant" => "Определитель",
                "trace" => "След",
                _ => operation
            };
        }

        private string GetAdvancedOperationDisplayName(string operation)
        {
            return operation switch
            {
                "LU" => "LU разложение",
                "QR" => "QR разложение",
                "eigenvalues" => "Собственные значения",
                "eigenvectors" => "Собственные векторы",
                _ => operation
            };
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

        // Установка матрицы извне (например, из заметок)
        public void SetMatrix(MatrixModel matrix, string target = "A")
        {
            try
            {
                if (target.ToUpper() == "A")
                {
                    MatrixA = matrix.Clone();
                    MatrixA.Name = "A";
                    MatrixARows = MatrixA.Rows;
                    MatrixAColumns = MatrixA.Columns;
                    StatusMessage = "Матрица A установлена";
                }
                else if (target.ToUpper() == "B")
                {
                    MatrixB = matrix.Clone();
                    MatrixB.Name = "B";
                    MatrixBRows = MatrixB.Rows;
                    MatrixBColumns = MatrixB.Columns;
                    StatusMessage = "Матрица B установлена";
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка установки матрицы: {ex.Message}");
            }
        }

        // Получение текущего результата
        public MatrixModel? GetCurrentResult()
        {
            return ResultMatrix;
        }

        // Парсинг матрицы из строки
        public bool TryParseMatrix(string matrixString, string target = "A")
        {
            try
            {
                var matrix = _matrixService.ParseMatrixFromString(matrixString);
                SetMatrix(matrix, target);
                return true;
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка парсинга матрицы: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Events

        // События для уведомления родительских компонентов
        public event EventHandler<CalculationResult>? OperationCompleted;
        public event EventHandler<MatrixModel>? MatrixCreated;

        #endregion

        #region Cleanup

        public void Dispose()
        {
            // Очистка ресурсов при необходимости
            MatrixHistory.Clear();
            SavedMatrices.Clear();
        }

        #endregion
    }
}