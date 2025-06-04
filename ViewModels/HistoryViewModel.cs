using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudyMateProject.Models;
using StudyMateProject.Services;
using StudyMateProject.Helpers;

namespace StudyMateProject.ViewModels
{
    // ViewModel для управления историей вычислений калькулятора
    public partial class HistoryViewModel : ObservableObject
    {
        #region Private Fields

        // Сервис интеграции для работы с историей
        private readonly ICalculatorIntegrationService _integrationService;

        // Основная модель истории
        private readonly CalculationHistory _history;

        // ID текущей заметки (если история привязана к заметке)
        private string? _currentNoteId;

        #endregion

        #region Observable Properties

        // Отфильтрованные элементы истории для отображения
        [ObservableProperty]
        private ObservableCollection<CalculationResult> filteredItems = new();

        // Текущий поисковый запрос
        [ObservableProperty]
        private string searchQuery = "";

        // Выбранный тип фильтра
        [ObservableProperty]
        private CalculationType? selectedFilterType = null;

        // Выбранный период фильтрации
        [ObservableProperty]
        private HistoryPeriod selectedPeriod = HistoryPeriod.All;

        // Выбранный элемент истории
        [ObservableProperty]
        private CalculationResult? selectedItem = null;

        // Режим отображения (компактный или детальный)
        [ObservableProperty]
        private bool isCompactMode = false;

        // Показывать ли только успешные вычисления
        [ObservableProperty]
        private bool showOnlySuccessful = true;

        // Группировать ли по дням
        [ObservableProperty]
        private bool groupByDate = true;

        // Общее количество элементов в истории
        [ObservableProperty]
        private int totalCount = 0;

        // Количество отфильтрованных элементов
        [ObservableProperty]
        private int filteredCount = 0;

        // Статистика по типам вычислений
        [ObservableProperty]
        private string statisticsText = "";

        #endregion

        #region Collections

        // Доступные типы для фильтрации
        public ObservableCollection<CalculationType?> FilterTypes { get; private set; }

        // Доступные периоды для фильтрации
        public ObservableCollection<HistoryPeriod> FilterPeriods { get; private set; }

        // Группированные элементы по дням (для группированного отображения)
        public ObservableCollection<HistoryGroup> GroupedItems { get; private set; }

        #endregion

        #region Commands

        // Команды поиска и фильтрации
        public ICommand SearchCommand { get; private set; }
        public ICommand ClearSearchCommand { get; private set; }
        public ICommand ApplyFilterCommand { get; private set; }
        public ICommand ClearFiltersCommand { get; private set; }

        // Команды управления элементами
        public ICommand SelectItemCommand { get; private set; }
        public ICommand ReuseItemCommand { get; private set; }
        public ICommand DeleteItemCommand { get; private set; }
        public ICommand CopyItemCommand { get; private set; }
        public ICommand ShareItemCommand { get; private set; }

        // Команды управления историей
        public ICommand ClearHistoryCommand { get; private set; }
        public ICommand ClearOldItemsCommand { get; private set; }
        public ICommand ExportHistoryCommand { get; private set; }
        public ICommand ImportHistoryCommand { get; private set; }

        // Команды отображения
        public ICommand ToggleCompactModeCommand { get; private set; }
        public ICommand ToggleGroupingCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }

        #endregion

        #region Constructor

        public HistoryViewModel(ICalculatorIntegrationService integrationService)
        {
            _integrationService = integrationService ?? throw new ArgumentNullException(nameof(integrationService));
            _history = new CalculationHistory();

            // Инициализируем коллекции
            InitializeCollections();

            // Инициализируем команды
            InitializeCommands();

            // Загружаем историю
            LoadHistoryAsync();

            // Подписываемся на события
            _integrationService.CalculationCompleted += OnCalculationCompleted;
        }

        #endregion

        #region Initialization

        private void InitializeCollections()
        {
            FilterTypes = new ObservableCollection<CalculationType?>
            {
                null, // "Все типы"
                CalculationType.Basic,
                CalculationType.Scientific,
                CalculationType.Matrix,
                CalculationType.Advanced,
                CalculationType.Conversion
            };

            FilterPeriods = new ObservableCollection<HistoryPeriod>
            {
                HistoryPeriod.All,
                HistoryPeriod.Today,
                HistoryPeriod.Yesterday,
                HistoryPeriod.ThisWeek,
                HistoryPeriod.ThisMonth,
                HistoryPeriod.LastMonth
            };

            GroupedItems = new ObservableCollection<HistoryGroup>();
        }

        private void InitializeCommands()
        {
            // Поиск и фильтрация
            SearchCommand = new AsyncRelayCommand(OnSearchAsync);
            ClearSearchCommand = new RelayCommand(OnClearSearch);
            ApplyFilterCommand = new AsyncRelayCommand(OnApplyFilterAsync);
            ClearFiltersCommand = new RelayCommand(OnClearFilters);

            // Управление элементами
            SelectItemCommand = new RelayCommand<CalculationResult>(OnSelectItem);
            ReuseItemCommand = new RelayCommand<CalculationResult>(OnReuseItem);
            DeleteItemCommand = new AsyncRelayCommand<CalculationResult>(OnDeleteItemAsync);
            CopyItemCommand = new AsyncRelayCommand<CalculationResult>(OnCopyItemAsync);
            ShareItemCommand = new AsyncRelayCommand<CalculationResult>(OnShareItemAsync);

            // Управление историей
            ClearHistoryCommand = new AsyncRelayCommand(OnClearHistoryAsync);
            ClearOldItemsCommand = new AsyncRelayCommand(OnClearOldItemsAsync);
            ExportHistoryCommand = new AsyncRelayCommand(OnExportHistoryAsync);
            ImportHistoryCommand = new AsyncRelayCommand(OnImportHistoryAsync);

            // Отображение
            ToggleCompactModeCommand = new RelayCommand(OnToggleCompactMode);
            ToggleGroupingCommand = new RelayCommand(OnToggleGrouping);
            RefreshCommand = new AsyncRelayCommand(OnRefreshAsync);
        }

        #endregion

        #region Search and Filter Commands

        // Поиск по истории
        private async Task OnSearchAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(SearchQuery))
                    {
                        // Если запрос пустой, показываем все элементы с учетом фильтров
                        ApplyCurrentFilters();
                        return;
                    }

                    // Выполняем поиск
                    var searchResults = _history.Search(SearchQuery);

                    // Применяем дополнительные фильтры к результатам поиска
                    var filteredResults = ApplyFiltersToCollection(searchResults);

                    // Обновляем UI в главном потоке
                    Microsoft.Maui.Controls.Application.Current?.Dispatcher.Dispatch(() =>
                    {
                        FilteredItems.Clear();
                        foreach (var item in filteredResults)
                        {
                            FilteredItems.Add(item);
                        }

                        UpdateCounts();
                        UpdateGroupedItems();
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Ошибка поиска: {ex.Message}");
                }
            });
        }

        // Очистка поиска
        private void OnClearSearch()
        {
            SearchQuery = "";
            ApplyCurrentFilters();
        }

        // Применение фильтров
        private async Task OnApplyFilterAsync()
        {
            await Task.Run(() => ApplyCurrentFilters());
        }

        // Очистка всех фильтров
        private void OnClearFilters()
        {
            SearchQuery = "";
            SelectedFilterType = null;
            SelectedPeriod = HistoryPeriod.All;
            ShowOnlySuccessful = true;

            ApplyCurrentFilters();
        }

        #endregion

        #region Item Management Commands

        // Выбор элемента истории
        private void OnSelectItem(CalculationResult? item)
        {
            SelectedItem = item;

            // Уведомляем о выборе элемента
            ItemSelected?.Invoke(this, new ItemSelectedEventArgs(item));
        }

        // Повторное использование результата
        private void OnReuseItem(CalculationResult? item)
        {
            if (item == null) return;

            // Уведомляем о желании переиспользовать результат
            ItemReuseRequested?.Invoke(this, new ItemReuseEventArgs(item));
        }

        // Удаление элемента из истории
        private async Task OnDeleteItemAsync(CalculationResult? item)
        {
            if (item == null) return;

            try
            {
                // Удаляем из модели истории
                _history.Remove(item);

                // Удаляем из отфильтрованной коллекции
                FilteredItems.Remove(item);

                // Если привязаны к заметке, обновляем историю заметки
                if (!string.IsNullOrEmpty(_currentNoteId))
                {
                    await SaveHistoryToNoteAsync();
                }

                UpdateCounts();
                UpdateGroupedItems();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления элемента: {ex.Message}");
            }
        }

        // Копирование элемента в буфер обмена
        private async Task OnCopyItemAsync(CalculationResult? item)
        {
            if (item == null) return;

            try
            {
                var textToCopy = MathFormatter.FormatHistoryEntry(item);
                await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.SetTextAsync(textToCopy);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка копирования: {ex.Message}");
            }
        }

        // Отправка элемента
        private async Task OnShareItemAsync(CalculationResult? item)
        {
            if (item == null) return;

            try
            {
                await _integrationService.ShareCalculationAsync(item, ShareTarget.ShareSheet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка отправки: {ex.Message}");
            }
        }

        #endregion

        #region History Management Commands

        // Полная очистка истории
        private async Task OnClearHistoryAsync()
        {
            try
            {
                _history.Clear();
                FilteredItems.Clear();
                GroupedItems.Clear();
                SelectedItem = null;

                // Если привязаны к заметке, очищаем историю заметки
                if (!string.IsNullOrEmpty(_currentNoteId))
                {
                    await _integrationService.ClearNoteHistoryAsync(_currentNoteId);
                }

                UpdateCounts();
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка очистки истории: {ex.Message}");
            }
        }

        // Очистка старых элементов (старше месяца)
        private async Task OnClearOldItemsAsync()
        {
            try
            {
                var cutoffDate = DateTime.Now.AddMonths(-1);
                var itemsToRemove = _history.Calculations
                    .Where(item => item.Timestamp < cutoffDate)
                    .ToList();

                foreach (var item in itemsToRemove)
                {
                    _history.Remove(item);
                    FilteredItems.Remove(item);
                }

                if (!string.IsNullOrEmpty(_currentNoteId))
                {
                    await SaveHistoryToNoteAsync();
                }

                UpdateCounts();
                UpdateGroupedItems();
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка очистки старых элементов: {ex.Message}");
            }
        }

        // Экспорт истории
        private async Task OnExportHistoryAsync()
        {
            try
            {
                var itemsToExport = FilteredItems.ToArray();
                if (itemsToExport.Length == 0)
                {
                    return;
                }

                // Экспортируем в Markdown формате
                var exportedText = await _integrationService.ExportCalculationsAsync(
                    itemsToExport, ExportFormat.Markdown);

                await Microsoft.Maui.ApplicationModel.DataTransfer.Clipboard.SetTextAsync(exportedText);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка экспорта: {ex.Message}");
            }
        }

        // Импорт истории (заглушка)
        private async Task OnImportHistoryAsync()
        {
            // В будущем здесь можно реализовать импорт из файла
            await Task.Delay(100);
        }

        #endregion

        #region Display Commands

        // Переключение компактного режима
        private void OnToggleCompactMode()
        {
            IsCompactMode = !IsCompactMode;
        }

        // Переключение группировки по дням
        private void OnToggleGrouping()
        {
            GroupByDate = !GroupByDate;
            UpdateGroupedItems();
        }

        // Обновление истории
        private async Task OnRefreshAsync()
        {
            await LoadHistoryAsync();
        }

        #endregion

        #region Helper Methods

        // Загрузка истории
        private async Task LoadHistoryAsync()
        {
            try
            {
                // Если привязаны к заметке, загружаем историю заметки
                if (!string.IsNullOrEmpty(_currentNoteId))
                {
                    var noteHistory = await _integrationService.GetNoteCalculationHistoryAsync(_currentNoteId);
                    _history.Calculations.Clear();

                    foreach (var item in noteHistory.Calculations)
                    {
                        _history.Calculations.Add(item);
                    }
                }

                ApplyCurrentFilters();
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки истории: {ex.Message}");
            }
        }

        // Сохранение истории в заметку
        private async Task SaveHistoryToNoteAsync()
        {
            if (string.IsNullOrEmpty(_currentNoteId)) return;

            try
            {
                foreach (var item in _history.Calculations)
                {
                    await _integrationService.SaveCalculationToNoteAsync(_currentNoteId, item);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения истории: {ex.Message}");
            }
        }

        // Применение текущих фильтров
        private void ApplyCurrentFilters()
        {
            var allItems = _history.Calculations.ToList();
            var filteredResults = ApplyFiltersToCollection(allItems);

            Microsoft.Maui.Controls.Application.Current?.Dispatcher.Dispatch(() =>
            {
                FilteredItems.Clear();
                foreach (var item in filteredResults.OrderByDescending(x => x.Timestamp))
                {
                    FilteredItems.Add(item);
                }

                UpdateCounts();
                UpdateGroupedItems();
            });
        }

        // Применение фильтров к коллекции
        private System.Collections.Generic.List<CalculationResult> ApplyFiltersToCollection(
            System.Collections.Generic.IEnumerable<CalculationResult> items)
        {
            var filtered = items.AsEnumerable();

            // Фильтр по типу
            if (SelectedFilterType.HasValue)
            {
                filtered = filtered.Where(item => item.Type == SelectedFilterType.Value);
            }

            // Фильтр по периоду
            filtered = filtered.Where(item => IsInSelectedPeriod(item.Timestamp));

            // Фильтр только успешных
            if (ShowOnlySuccessful)
            {
                filtered = filtered.Where(item => !item.HasError);
            }

            return filtered.ToList();
        }

        // Проверка принадлежности к выбранному периоду
        private bool IsInSelectedPeriod(DateTime timestamp)
        {
            var now = DateTime.Now;

            return SelectedPeriod switch
            {
                HistoryPeriod.Today => timestamp.Date == now.Date,
                HistoryPeriod.Yesterday => timestamp.Date == now.Date.AddDays(-1),
                HistoryPeriod.ThisWeek => timestamp >= now.Date.AddDays(-(int)now.DayOfWeek),
                HistoryPeriod.ThisMonth => timestamp.Year == now.Year && timestamp.Month == now.Month,
                HistoryPeriod.LastMonth => timestamp.Year == now.Year && timestamp.Month == now.Month - 1,
                _ => true // All
            };
        }

        // Обновление счетчиков
        private void UpdateCounts()
        {
            TotalCount = _history.Calculations.Count;
            FilteredCount = FilteredItems.Count;
        }

        // Обновление группированных элементов
        private void UpdateGroupedItems()
        {
            GroupedItems.Clear();

            if (!GroupByDate)
                return;

            try
            {
                var groupedByDate = FilteredItems
                    .GroupBy(item => item.Timestamp.Date)
                    .OrderByDescending(group => group.Key);

                foreach (var group in groupedByDate)
                {
                    var historyGroup = new HistoryGroup
                    {
                        Date = group.Key,
                        Items = new ObservableCollection<CalculationResult>(
                            group.OrderByDescending(item => item.Timestamp))
                    };

                    GroupedItems.Add(historyGroup);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка группировки: {ex.Message}");
            }
        }

        // Обновление статистики
        private void UpdateStatistics()
        {
            try
            {
                var stats = _history.GetStatistics();
                var totalCalculations = stats.Values.Sum();

                if (totalCalculations == 0)
                {
                    StatisticsText = "История пуста";
                    return;
                }

                var statsText = $"Всего вычислений: {totalCalculations}\n";

                foreach (var stat in stats.OrderByDescending(s => s.Value))
                {
                    var percentage = (stat.Value * 100.0) / totalCalculations;
                    var typeName = GetCalculationTypeDisplayName(stat.Key);
                    statsText += $"{typeName}: {stat.Value} ({percentage:F1}%)\n";
                }

                StatisticsText = statsText.TrimEnd('\n');
            }
            catch (Exception ex)
            {
                StatisticsText = "Ошибка расчета статистики";
                System.Diagnostics.Debug.WriteLine($"Ошибка статистики: {ex.Message}");
            }
        }

        // Получение отображаемого названия типа вычисления
        private string GetCalculationTypeDisplayName(CalculationType type)
        {
            return type switch
            {
                CalculationType.Basic => "Базовые",
                CalculationType.Scientific => "Научные",
                CalculationType.Matrix => "Матрицы",
                CalculationType.Advanced => "Продвинутые",
                CalculationType.Conversion => "Конвертация",
                _ => "Неизвестно"
            };
        }

        // Обработчик события завершения вычисления
        private void OnCalculationCompleted(object? sender, CalculationCompletedEventArgs e)
        {
            // Добавляем новое вычисление в историю
            _history.AddCalculation(e.Result);

            // Если элемент проходит текущие фильтры, добавляем его в отображение
            var filteredResult = ApplyFiltersToCollection(new[] { e.Result });
            if (filteredResult.Any())
            {
                Microsoft.Maui.Controls.Application.Current?.Dispatcher.Dispatch(() =>
                {
                    FilteredItems.Insert(0, e.Result);
                    UpdateCounts();
                    UpdateGroupedItems();
                    UpdateStatistics();
                });
            }

            // Сохраняем в заметку если привязаны
            if (!string.IsNullOrEmpty(_currentNoteId))
            {
                Task.Run(() => SaveHistoryToNoteAsync());
            }
        }

        #endregion

        #region Public Methods

        // Привязка к заметке
        public async Task AttachToNoteAsync(string noteId)
        {
            _currentNoteId = noteId;
            await LoadHistoryAsync();
        }

        // Отвязка от заметки
        public void DetachFromNote()
        {
            _currentNoteId = null;
        }

        // Добавление элемента в историю извне
        public void AddCalculation(CalculationResult result)
        {
            _history.AddCalculation(result);

            // Применяем фильтры и обновляем отображение
            var filteredResult = ApplyFiltersToCollection(new[] { result });
            if (filteredResult.Any())
            {
                FilteredItems.Insert(0, result);
                UpdateCounts();
                UpdateGroupedItems();
                UpdateStatistics();
            }
        }

        // Получение всех элементов истории
        public System.Collections.Generic.IEnumerable<CalculationResult> GetAllItems()
        {
            return _history.Calculations.AsEnumerable();
        }

        // Получение отфильтрованных элементов
        public System.Collections.Generic.IEnumerable<CalculationResult> GetFilteredItems()
        {
            return FilteredItems.AsEnumerable();
        }

        // Поиск по конкретному запросу
        public async Task<System.Collections.Generic.IEnumerable<CalculationResult>> SearchAsync(string query)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(query))
                    return Enumerable.Empty<CalculationResult>();

                return _history.Search(query);
            });
        }

        // Получение элементов определенного типа
        public System.Collections.Generic.IEnumerable<CalculationResult> GetByType(CalculationType type, int count = 10)
        {
            return _history.GetByType(type, count);
        }

        // Получение недавних успешных вычислений
        public System.Collections.Generic.IEnumerable<CalculationResult> GetRecentSuccessful(int count = 10)
        {
            return _history.GetSuccessful(count);
        }

        // Установка фильтров программно
        public void SetFilters(CalculationType? type = null, HistoryPeriod period = HistoryPeriod.All, bool onlySuccessful = true)
        {
            SelectedFilterType = type;
            SelectedPeriod = period;
            ShowOnlySuccessful = onlySuccessful;

            ApplyCurrentFilters();
        }

        // Быстрое применение поиска
        public async Task QuickSearchAsync(string query)
        {
            SearchQuery = query;
            await OnSearchAsync();
        }

        #endregion

        #region Property Change Handlers

        // Автоматическое применение фильтров при изменении свойств
        partial void OnSelectedFilterTypeChanged(CalculationType? value)
        {
            ApplyCurrentFilters();
        }

        partial void OnSelectedPeriodChanged(HistoryPeriod value)
        {
            ApplyCurrentFilters();
        }

        partial void OnShowOnlySuccessfulChanged(bool value)
        {
            ApplyCurrentFilters();
        }

        partial void OnGroupByDateChanged(bool value)
        {
            UpdateGroupedItems();
        }

        #endregion

        #region Events

        // События для взаимодействия с родительскими компонентами
        public event EventHandler<ItemSelectedEventArgs>? ItemSelected;
        public event EventHandler<ItemReuseEventArgs>? ItemReuseRequested;
        public event EventHandler<HistoryUpdatedEventArgs>? HistoryUpdated;

        #endregion

        #region Cleanup

        public void Dispose()
        {
            // Отписываемся от событий
            if (_integrationService != null)
            {
                _integrationService.CalculationCompleted -= OnCalculationCompleted;
            }

            // Очищаем коллекции
            FilteredItems.Clear();
            GroupedItems.Clear();
            FilterTypes.Clear();
            FilterPeriods.Clear();
        }

        #endregion
    }

    #region Supporting Classes and Enums

    // Периоды для фильтрации истории
    public enum HistoryPeriod
    {
        All,
        Today,
        Yesterday,
        ThisWeek,
        ThisMonth,
        LastMonth
    }

    // Группа элементов истории по дате
    public class HistoryGroup
    {
        public DateTime Date { get; set; }
        public string DateDisplayText => Date.Date == DateTime.Today ? "Сегодня" :
                                       Date.Date == DateTime.Today.AddDays(-1) ? "Вчера" :
                                       Date.ToString("dd MMMM yyyy");
        public ObservableCollection<CalculationResult> Items { get; set; } = new();
        public int Count => Items.Count;
    }

    // Аргументы события выбора элемента
    public class ItemSelectedEventArgs : EventArgs
    {
        public CalculationResult? SelectedItem { get; }

        public ItemSelectedEventArgs(CalculationResult? item)
        {
            SelectedItem = item;
        }
    }

    // Аргументы события переиспользования элемента
    public class ItemReuseEventArgs : EventArgs
    {
        public CalculationResult ReusedItem { get; }

        public ItemReuseEventArgs(CalculationResult item)
        {
            ReusedItem = item ?? throw new ArgumentNullException(nameof(item));
        }
    }

    // Аргументы события обновления истории
    public class HistoryUpdatedEventArgs : EventArgs
    {
        public int TotalCount { get; }
        public int NewItemsCount { get; }

        public HistoryUpdatedEventArgs(int totalCount, int newItemsCount = 0)
        {
            TotalCount = totalCount;
            NewItemsCount = newItemsCount;
        }
    }

    #endregion
}