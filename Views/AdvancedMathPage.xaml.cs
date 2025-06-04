using StudyMateProject.ViewModels;
using System.Linq;

namespace StudyMateProject.Views;

public partial class AdvancedMathPage : ContentPage
{
    #region Private Fields

    private readonly AdvancedMathViewModel _viewModel;

    #endregion

    public AdvancedMathPage(AdvancedMathViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        BindingContext = _viewModel;

        // Инициализация страницы
        InitializePage();
    }

    #region Initialization

    private void InitializePage()
    {
        // Подписываемся на события ViewModel
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        // Настраиваем адаптивность
        SizeChanged += OnPageSizeChanged;

        // Устанавливаем фокус на первое поле при загрузке
        SetInitialFocus();
    }

    private void SetInitialFocus()
    {
        // Устанавливаем фокус на поле функции в зависимости от выбранной операции
        switch (_viewModel.SelectedOperationType)
        {
            case "Интеграл":
                // Фокус на поле интегральной функции
                break;
            case "Предел":
                // Фокус на поле функции предела
                break;
            case "Ряд":
                // Фокус на поле формулы ряда
                break;
            case "Производная":
                // Фокус на поле функции производной
                break;
        }
    }

    #endregion

    #region Event Handlers

    // Обработчик изменений в ViewModel
    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_viewModel.SelectedOperationType):
                HandleOperationTypeChanged();
                break;

            case nameof(_viewModel.HasError):
                HandleErrorStateChanged();
                break;

            case nameof(_viewModel.IsCalculating):
                HandleCalculatingStateChanged();
                break;

            case nameof(_viewModel.OperationResult):
                HandleResultChanged();
                break;
        }
    }

    // Обработчик изменения размера страницы
    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        AdaptToScreenSize();
    }

    // Обработка изменения типа операции
    private void HandleOperationTypeChanged()
    {
        // Анимация перехода между секциями
        AnimateSectionTransition();

        // Устанавливаем фокус на соответствующее поле
        SetInitialFocus();
    }

    // Обработка состояния ошибки
    private void HandleErrorStateChanged()
    {
        if (_viewModel.HasError)
        {
            // Анимация ошибки
            AnimateErrorEffect();
        }
    }

    // Обработка состояния вычисления
    private void HandleCalculatingStateChanged()
    {
        // Добавляем визуальную обратную связь при вычислении
        if (_viewModel.IsCalculating)
        {
            // Можно добавить эффекты загрузки
        }
    }

    // Обработка изменения результата
    private void HandleResultChanged()
    {
        if (!string.IsNullOrEmpty(_viewModel.OperationResult))
        {
            // Анимация появления результата
            AnimateResultAppearance();
        }
    }

    #endregion

    #region Animation Methods

    // Анимация перехода между секциями
    private async void AnimateSectionTransition()
    {
        // Плавная смена видимости секций
        await this.FadeTo(0.7, 150);
        await this.FadeTo(1.0, 150);
    }

    // Анимация ошибки
    private async void AnimateErrorEffect()
    {
        // Легкое встряхивание при ошибке
        await this.TranslateTo(-5, 0, 50);
        await this.TranslateTo(5, 0, 50);
        await this.TranslateTo(-3, 0, 50);
        await this.TranslateTo(0, 0, 50);
    }

    // Анимация появления результата
    private async void AnimateResultAppearance()
    {
        var resultBorder = this.FindByName("ResultBorder");
        if (resultBorder is VisualElement visualElement)
        {
            // Эффект пульсации для привлечения внимания к результату
            await visualElement.ScaleTo(1.05, 200, Easing.CubicOut);
            await visualElement.ScaleTo(1.0, 200, Easing.CubicIn);
        }
    }

    #endregion

    #region Responsive Layout

    // Адаптация к размеру экрана
    private void AdaptToScreenSize()
    {
        var width = Width;
        var height = Height;

        if (width <= 0 || height <= 0) return;

        // Адаптация для разных размеров экрана
        if (width < 600) // Мобильные устройства
        {
            AdaptForMobileLayout();
        }
        else if (width < 1000) // Планшеты
        {
            AdaptForTabletLayout();
        }
        else // Десктоп
        {
            AdaptForDesktopLayout();
        }

        // Адаптация в зависимости от ориентации
        if (width > height) // Альбомная ориентация
        {
            AdaptForLandscapeLayout();
        }
        else // Портретная ориентация
        {
            AdaptForPortraitLayout();
        }
    }

    private void AdaptForMobileLayout()
    {
        // Настройки для мобильных устройств
        // Уменьшаем отступы и размеры элементов

        // Можно программно изменить размеры шрифтов
        var labels = GetAllChildrenOfType<Label>(this);
        foreach (var label in labels)
        {
            if (label.FontSize > 16)
            {
                label.FontSize = Math.Max(14, label.FontSize * 0.9);
            }
        }
    }

    private void AdaptForTabletLayout()
    {
        // Настройки для планшетов
        // Средние размеры элементов
    }

    private void AdaptForDesktopLayout()
    {
        // Настройки для десктопа
        // Полные размеры элементов
    }

    private void AdaptForLandscapeLayout()
    {
        // Альбомная ориентация
        // Можно разместить секции горизонтально
    }

    private void AdaptForPortraitLayout()
    {
        // Портретная ориентация
        // Вертикальное размещение секций
    }

    // Вспомогательный метод для получения всех дочерних элементов определенного типа
    private IEnumerable<T> GetAllChildrenOfType<T>(Element parent) where T : Element
    {
        var result = new List<T>();

        if (parent is T typedParent)
        {
            result.Add(typedParent);
        }

        if (parent is Layout layout)
        {
            foreach (var child in layout.Children.OfType<Element>())
            {
                result.AddRange(GetAllChildrenOfType<T>(child));
            }
        }
        else if (parent is ContentView contentView && contentView.Content is Element contentElement)
        {
            result.AddRange(GetAllChildrenOfType<T>(contentElement));
        }
        else if (parent is ScrollView scrollView && scrollView.Content is Element scrollElement)
        {
            result.AddRange(GetAllChildrenOfType<T>(scrollElement));
        }

        return result;
    }

    #endregion

    #region Validation Methods

    // Валидация введенных данных перед вычислением
    private bool ValidateCurrentInput()
    {
        switch (_viewModel.SelectedOperationType)
        {
            case "Интеграл":
                return ValidateIntegralInput();
            case "Предел":
                return ValidateLimitInput();
            case "Ряд":
                return ValidateSeriesInput();
            case "Производная":
                return ValidateDerivativeInput();
            default:
                return false;
        }
    }

    private bool ValidateIntegralInput()
    {
        // Проверяем, что функция не пустая
        if (string.IsNullOrWhiteSpace(_viewModel.IntegralFunction))
        {
            ShowValidationError("Введите функцию для интегрирования");
            return false;
        }

        // Проверяем корректность пределов
        if (_viewModel.IntegralLowerLimit >= _viewModel.IntegralUpperLimit)
        {
            ShowValidationError("Нижний предел должен быть меньше верхнего");
            return false;
        }

        return true;
    }

    private bool ValidateLimitInput()
    {
        // Проверяем, что функция не пустая
        if (string.IsNullOrWhiteSpace(_viewModel.LimitFunction))
        {
            ShowValidationError("Введите функцию для вычисления предела");
            return false;
        }

        // Проверяем переменную
        if (string.IsNullOrWhiteSpace(_viewModel.LimitVariable))
        {
            ShowValidationError("Укажите переменную");
            return false;
        }

        return true;
    }

    private bool ValidateSeriesInput()
    {
        // Проверяем, что формула не пустая
        if (string.IsNullOrWhiteSpace(_viewModel.SeriesFormula))
        {
            ShowValidationError("Введите формулу общего члена ряда");
            return false;
        }

        // Проверяем переменную
        if (string.IsNullOrWhiteSpace(_viewModel.SeriesVariable))
        {
            ShowValidationError("Укажите переменную ряда");
            return false;
        }

        // Для конечных рядов проверяем корректность индексов
        if (!_viewModel.SeriesIsInfinite && _viewModel.SeriesEndIndex.HasValue)
        {
            if (_viewModel.SeriesEndIndex <= _viewModel.SeriesStartIndex)
            {
                ShowValidationError("Конечный индекс должен быть больше начального");
                return false;
            }
        }

        return true;
    }

    private bool ValidateDerivativeInput()
    {
        // Проверяем, что функция не пустая
        if (string.IsNullOrWhiteSpace(_viewModel.DerivativeFunction))
        {
            ShowValidationError("Введите функцию для дифференцирования");
            return false;
        }

        // Проверяем переменную
        if (string.IsNullOrWhiteSpace(_viewModel.DerivativeVariable))
        {
            ShowValidationError("Укажите переменную дифференцирования");
            return false;
        }

        return true;
    }

    // Показ ошибки валидации
    private async void ShowValidationError(string message)
    {
        // Можно показать Toast или Alert
        await DisplayAlert("Ошибка ввода", message, "OK");
    }

    #endregion

    #region Helper Methods

    // Настройка внешнего вида в зависимости от темы
    private void SetupThemeSpecificStyles()
    {
        var isDarkTheme = Application.Current?.RequestedTheme == AppTheme.Dark;

        // Можно настроить дополнительные стили для темной темы
        if (isDarkTheme)
        {
            // Настройки для темной темы
        }
        else
        {
            // Настройки для светлой темы
        }
    }

    #endregion

    #region Public Methods

    // Установка функции извне
    public void SetFunction(string function, string operationType = "")
    {
        _viewModel.SetFunction(function, operationType);
    }

    // Получение текущей функции
    public string GetCurrentFunction()
    {
        return _viewModel.GetCurrentFunction();
    }

    // Быстрое вычисление с параметрами по умолчанию
    public async Task<string?> QuickCalculateAsync(string function, string operationType)
    {
        try
        {
            var result = await _viewModel.QuickCalculateAsync(function, operationType);
            return result?.FormattedResult;
        }
        catch
        {
            return null;
        }
    }

    // Переключение на определенный тип операции
    public void SwitchToOperationType(string operationType)
    {
        _viewModel.SelectedOperationType = operationType;
    }

    #endregion

    #region Page Lifecycle

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Обновляем размеры при появлении страницы
        AdaptToScreenSize();

        // Настраиваем тему
        SetupThemeSpecificStyles();

        // Устанавливаем фокус
        SetInitialFocus();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Сохраняем состояние при скрытии страницы
        // Можно сохранить введенные функции для восстановления
    }

    protected override bool OnBackButtonPressed()
    {
        // Обрабатываем кнопку "назад"
        if (_viewModel.IsCalculating)
        {
            // Если идет вычисление, можно предложить отменить
            return true; // Предотвращаем закрытие страницы
        }

        return base.OnBackButtonPressed();
    }

    #endregion

    #region Gesture Handlers

    // Можно добавить обработчики жестов для улучшения UX
    private void OnResultTapped(object sender, EventArgs e)
    {
        // При нажатии на результат - копируем его
        _viewModel.CopyResultCommand?.Execute(null);
    }

    private void OnFunctionFieldFocused(object sender, FocusEventArgs e)
    {
        // При фокусе на поле функции - показываем подсказки
        if (e.IsFocused && sender is Entry entry)
        {
            // Можно показать клавиатуру с математическими символами
            // или панель с часто используемыми функциями
        }
    }

    #endregion

    #region Accessibility

    // Настройка доступности для людей с ограниченными возможностями
    private void SetupAccessibility()
    {
        // Устанавливаем семантические описания для элементов
        var integralSection = this.FindByName("IntegralSection") as VisualElement;
        if (integralSection != null)
        {
            SemanticProperties.SetDescription(integralSection, "Раздел для вычисления определенных интегралов");
        }

        var limitSection = this.FindByName("LimitSection") as VisualElement;
        if (limitSection != null)
        {
            SemanticProperties.SetDescription(limitSection, "Раздел для вычисления пределов функций");
        }

        // Добавляем хинты для полей ввода
        var functionEntries = GetAllChildrenOfType<Entry>(this);
        foreach (var entry in functionEntries)
        {
            if (string.IsNullOrEmpty(entry.Placeholder))
            {
                SemanticProperties.SetHint(entry, "Введите математическую функцию");
            }
        }
    }

    #endregion

    #region Cleanup

    protected override void OnParentSet()
    {
        base.OnParentSet();

        // Очистка при удалении страницы
        if (Parent == null)
        {
            SizeChanged -= OnPageSizeChanged;

            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                _viewModel.Dispose();
            }
        }
    }

    #endregion
}