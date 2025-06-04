using StudyMateProject.ViewModels;

namespace StudyMateProject.Components;

// Перечисление для позиции popup
public enum PopupPosition
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    Center
}

public partial class MiniCalculatorPopup : ContentView
{
    #region Bindable Properties

    // Видимость popup
    public static readonly BindableProperty IsPopupVisibleProperty =
        BindableProperty.Create(nameof(IsPopupVisible), typeof(bool), typeof(MiniCalculatorPopup), false,
            propertyChanged: OnIsPopupVisibleChanged);

    public bool IsPopupVisible
    {
        get => (bool)GetValue(IsPopupVisibleProperty);
        set => SetValue(IsPopupVisibleProperty, value);
    }

    // Позиция popup относительно родительского элемента
    public static readonly BindableProperty PopupPositionProperty =
        BindableProperty.Create(nameof(PopupPosition), typeof(PopupPosition), typeof(MiniCalculatorPopup),
            PopupPosition.BottomRight, propertyChanged: OnPopupPositionChanged);

    public PopupPosition PopupPosition
    {
        get => (PopupPosition)GetValue(PopupPositionProperty);
        set => SetValue(PopupPositionProperty, value);
    }

    // Максимальная ширина popup
    public static readonly BindableProperty MaxPopupWidthProperty =
        BindableProperty.Create(nameof(MaxPopupWidth), typeof(double), typeof(MiniCalculatorPopup), 350.0);

    public double MaxPopupWidth
    {
        get => (double)GetValue(MaxPopupWidthProperty);
        set => SetValue(MaxPopupWidthProperty, value);
    }

    // Максимальная высота popup
    public static readonly BindableProperty MaxPopupHeightProperty =
        BindableProperty.Create(nameof(MaxPopupHeight), typeof(double), typeof(MiniCalculatorPopup), 450.0);

    public double MaxPopupHeight
    {
        get => (double)GetValue(MaxPopupHeightProperty);
        set => SetValue(MaxPopupHeightProperty, value);
    }

    #endregion

    #region Private Fields

    private MiniCalculatorViewModel? _viewModel;
    private string? _attachedNoteId;

    #endregion

    #region Events

    // События для взаимодействия с родительским компонентом
    public event EventHandler? PopupClosed;
    public event EventHandler? ExpandRequested;
    public event EventHandler<string>? ResultInserted;

    #endregion

    public MiniCalculatorPopup()
    {
        InitializeComponent();

        // Начальная настройка
        WidthRequest = MaxPopupWidth;
        HeightRequest = MaxPopupHeight;

        // Скрываем по умолчанию
        IsPopupVisible = false;
    }

    #region Property Changed Handlers

    // Обработчик изменения видимости popup
    private static void OnIsPopupVisibleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MiniCalculatorPopup popup && newValue is bool isVisible)
        {
            popup.HandleVisibilityChanged(isVisible);
        }
    }

    // Обработчик изменения позиции popup
    private static void OnPopupPositionChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MiniCalculatorPopup popup && newValue is PopupPosition position)
        {
            popup.UpdatePopupPosition(position);
        }
    }

    #endregion

    #region Private Methods

    // Обработка изменения видимости
    private async void HandleVisibilityChanged(bool isVisible)
    {
        if (isVisible)
        {
            await ShowPopupWithAnimation();
        }
        else
        {
            await HidePopupWithAnimation();
        }
    }

    // Анимация появления popup
    private async Task ShowPopupWithAnimation()
    {
        // Сначала делаем элемент видимым, но прозрачным
        Opacity = 0;
        Scale = 0.8;
        IsVisible = true;

        // Анимация появления
        await Task.WhenAll(
            this.FadeTo(1, 200, Easing.CubicOut),
            this.ScaleTo(1, 200, Easing.CubicOut)
        );
    }

    // Анимация скрытия popup
    private async Task HidePopupWithAnimation()
    {
        // Анимация исчезновения
        await Task.WhenAll(
            this.FadeTo(0, 150, Easing.CubicIn),
            this.ScaleTo(0.8, 150, Easing.CubicIn)
        );

        // Скрываем элемент
        IsVisible = false;
    }

    // Обновление позиции popup
    private void UpdatePopupPosition(PopupPosition position)
    {
        // Здесь можно настроить расположение popup
        // В зависимости от позиции устанавливаем HorizontalOptions и VerticalOptions
        switch (position)
        {
            case PopupPosition.TopLeft:
                HorizontalOptions = LayoutOptions.Start;
                VerticalOptions = LayoutOptions.Start;
                break;
            case PopupPosition.TopRight:
                HorizontalOptions = LayoutOptions.End;
                VerticalOptions = LayoutOptions.Start;
                break;
            case PopupPosition.BottomLeft:
                HorizontalOptions = LayoutOptions.Start;
                VerticalOptions = LayoutOptions.End;
                break;
            case PopupPosition.BottomRight:
                HorizontalOptions = LayoutOptions.End;
                VerticalOptions = LayoutOptions.End;
                break;
            case PopupPosition.Center:
                HorizontalOptions = LayoutOptions.Center;
                VerticalOptions = LayoutOptions.Center;
                break;
        }
    }

    // Адаптация размера под доступное пространство
    private void AdaptToAvailableSpace(double availableWidth, double availableHeight)
    {
        // Ограничиваем размер popup доступным пространством
        var maxWidth = Math.Min(MaxPopupWidth, availableWidth * 0.9);
        var maxHeight = Math.Min(MaxPopupHeight, availableHeight * 0.8);

        WidthRequest = maxWidth;
        HeightRequest = maxHeight;

        // Уведомляем вложенный калькулятор о новых размерах
        if (_viewModel != null)
        {
            _viewModel.SetSize(maxWidth, maxHeight);
        }
    }

    #endregion

    #region Public Methods

    // Показать popup
    public async Task ShowAsync()
    {
        IsPopupVisible = true;
    }

    // Скрыть popup
    public async Task HideAsync()
    {
        IsPopupVisible = false;
    }

    // Переключить видимость popup
    public async Task ToggleAsync()
    {
        IsPopupVisible = !IsPopupVisible;
    }

    // Установка ViewModel
    public void SetViewModel(MiniCalculatorViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = viewModel;

        // Подписываемся на события ViewModel
        if (_viewModel != null)
        {
            _viewModel.ExpandRequested += OnExpandRequested;
            _viewModel.CloseRequested += OnCloseRequested;
        }
    }

    // Привязка к заметке
    public void AttachToNote(string noteId, int cursorPosition = -1)
    {
        _attachedNoteId = noteId;
        _viewModel?.AttachToNote(noteId, cursorPosition);
    }

    // Отвязка от заметки
    public void DetachFromNote()
    {
        _attachedNoteId = null;
        _viewModel?.DetachFromNote();
    }

    // Установка позиции и размера
    public void SetPositionAndSize(PopupPosition position, double maxWidth, double maxHeight)
    {
        PopupPosition = position;
        MaxPopupWidth = maxWidth;
        MaxPopupHeight = maxHeight;

        UpdatePopupPosition(position);
        AdaptToAvailableSpace(maxWidth, maxHeight);
    }

    // Получение текущего результата
    public string GetCurrentResult()
    {
        return _viewModel?.GetCurrentResult() ?? "0";
    }

    // Вставка текста в калькулятор
    public void InsertText(string text)
    {
        _viewModel?.InsertText(text);
    }

    #endregion

    #region Event Handlers

    // Обработчик запроса на развертывание
    private void OnExpandRequested(object? sender, EventArgs e)
    {
        ExpandRequested?.Invoke(this, EventArgs.Empty);
    }

    // Обработчик запроса на закрытие
    private void OnCloseRequested(object? sender, EventArgs e)
    {
        IsPopupVisible = false;
        PopupClosed?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Cleanup

    protected override void OnParentSet()
    {
        base.OnParentSet();

        // Очистка при удалении из визуального дерева
        if (Parent == null)
        {
            if (_viewModel != null)
            {
                _viewModel.ExpandRequested -= OnExpandRequested;
                _viewModel.CloseRequested -= OnCloseRequested;
                _viewModel.Dispose();
            }
        }
    }

    #endregion
}