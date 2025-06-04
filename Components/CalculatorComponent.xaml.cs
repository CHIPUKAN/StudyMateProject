using StudyMateProject.ViewModels;

namespace StudyMateProject.Components;

public partial class CalculatorComponent : ContentView
{
    #region Bindable Properties

    // Размер шрифта для дисплея калькулятора
    public static readonly BindableProperty FontSizeProperty =
        BindableProperty.Create(nameof(FontSize), typeof(double), typeof(CalculatorComponent), 28.0);

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    // Режим мини-калькулятора
    public static readonly BindableProperty IsMiniModeProperty =
        BindableProperty.Create(nameof(IsMiniMode), typeof(bool), typeof(CalculatorComponent), false,
            propertyChanged: OnIsMiniModeChanged);

    public bool IsMiniMode
    {
        get => (bool)GetValue(IsMiniModeProperty);
        set => SetValue(IsMiniModeProperty, value);
    }

    // Показывать ли кнопку вставки
    public static readonly BindableProperty ShowInsertButtonProperty =
        BindableProperty.Create(nameof(ShowInsertButton), typeof(bool), typeof(CalculatorComponent), false);

    public bool ShowInsertButton
    {
        get => (bool)GetValue(ShowInsertButtonProperty);
        set => SetValue(ShowInsertButtonProperty, value);
    }

    // Показывать ли кнопку развертывания
    public static readonly BindableProperty ShowExpandButtonProperty =
        BindableProperty.Create(nameof(ShowExpandButton), typeof(bool), typeof(CalculatorComponent), false);

    public bool ShowExpandButton
    {
        get => (bool)GetValue(ShowExpandButtonProperty);
        set => SetValue(ShowExpandButtonProperty, value);
    }

    #endregion

    public CalculatorComponent()
    {
        InitializeComponent();

        // Подписываемся на изменение размера для адаптивности
        SizeChanged += OnSizeChanged;
    }

    #region Property Changed Handlers

    // Обработчик изменения режима мини-калькулятора
    private static void OnIsMiniModeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CalculatorComponent component && newValue is bool isMiniMode)
        {
            component.AdaptToMiniMode(isMiniMode);
        }
    }

    #endregion

    #region Private Methods

    // Адаптация интерфейса под мини-режим
    private void AdaptToMiniMode(bool isMiniMode)
    {
        if (isMiniMode)
        {
            // Уменьшаем размеры для мини-режима
            FontSize = 20.0;

            // Настраиваем размеры кнопок
            AdaptButtonSizes(40.0);
        }
        else
        {
            // Восстанавливаем обычные размеры
            FontSize = 28.0;
            AdaptButtonSizes(60.0);
        }
    }

    // Адаптация размеров кнопок
    private void AdaptButtonSizes(double buttonSize)
    {
        // Устанавливаем размеры для всех кнопок в сетке
        if (MainButtonsGrid != null)
        {
            SetButtonSizesInGrid(MainButtonsGrid, buttonSize);
        }

        if (ScientificButtonsGrid != null)
        {
            SetButtonSizesInGrid(ScientificButtonsGrid, buttonSize * 0.8); // Научные кнопки чуть меньше
        }
    }

    // Установка размеров кнопок в сетке
    private void SetButtonSizesInGrid(Grid grid, double size)
    {
        foreach (var child in grid.Children)
        {
            if (child is Button button)
            {
                button.HeightRequest = size;
                // Ширина автоматически подстраивается под колонки сетки
            }
        }
    }

    // Обработчик изменения размера компонента
    private void OnSizeChanged(object? sender, EventArgs e)
    {
        AdaptToAvailableSpace();
    }

    // Адаптация к доступному пространству
    private void AdaptToAvailableSpace()
    {
        if (Width <= 0 || Height <= 0) return;

        // Автоматически определяем оптимальные размеры
        var availableWidth = Width - 32; // Учитываем отступы
        var availableHeight = Height - 100; // Учитываем дисплей

        // Рассчитываем оптимальный размер кнопок
        var optimalButtonSize = CalculateOptimalButtonSize(availableWidth, availableHeight);

        // Применяем размеры
        AdaptButtonSizes(optimalButtonSize);

        // Адаптируем размер шрифта дисплея
        AdaptDisplayFontSize(availableWidth);
    }

    // Расчет оптимального размера кнопок
    private double CalculateOptimalButtonSize(double availableWidth, double availableHeight)
    {
        // Основная сетка 4x5, научная добавляет 2 строки
        var rows = 5 + (ScientificButtonsGrid?.IsVisible == true ? 2 : 0);
        var columns = 4;

        // Учитываем отступы между кнопками
        var spacing = 8 * (columns - 1); // Горизонтальные отступы
        var verticalSpacing = 8 * (rows - 1); // Вертикальные отступы

        // Расчет размера по ширине и высоте
        var widthBasedSize = (availableWidth - spacing) / columns;
        var heightBasedSize = (availableHeight - verticalSpacing) / rows;

        // Берем меньший размер и ограничиваем диапазон
        var optimalSize = Math.Min(widthBasedSize, heightBasedSize);
        return Math.Max(35, Math.Min(80, optimalSize));
    }

    // Адаптация размера шрифта дисплея
    private void AdaptDisplayFontSize(double availableWidth)
    {
        if (availableWidth < 300)
        {
            FontSize = 18.0;
        }
        else if (availableWidth < 400)
        {
            FontSize = 22.0;
        }
        else if (availableWidth < 500)
        {
            FontSize = 26.0;
        }
        else
        {
            FontSize = IsMiniMode ? 20.0 : 28.0;
        }
    }

    #endregion

    #region Public Methods

    // Установка ViewModel программно
    public void SetViewModel(CalculatorViewModel viewModel)
    {
        BindingContext = viewModel;
    }

    // Получение текущей ViewModel
    public CalculatorViewModel? GetViewModel()
    {
        return BindingContext as CalculatorViewModel;
    }

    // Принудительная адаптация размеров
    public void ForceAdaptation()
    {
        AdaptToAvailableSpace();
    }

    // Установка размеров для конкретного режима
    public void SetModeSpecificSizes(double displayFontSize, double buttonSize)
    {
        FontSize = displayFontSize;
        AdaptButtonSizes(buttonSize);
    }

    #endregion

    #region Cleanup

    protected override void OnParentSet()
    {
        base.OnParentSet();

        // Отписываемся от событий при удалении из визуального дерева
        if (Parent == null)
        {
            SizeChanged -= OnSizeChanged;
        }
    }

    #endregion
}