using StudyMateProject.ViewModels;
using StudyMateProject.Services;

namespace StudyMateProject.Views;

public partial class CalculatorPage : ContentPage
{
    #region Private Fields

    private readonly CalculatorViewModel _viewModel;

    #endregion

    public CalculatorPage(CalculatorViewModel viewModel)
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
        // Устанавливаем режим полноэкранного калькулятора
        _viewModel.SetModeAsync(CalculatorMode.FullScreen);

        // Подписываемся на события ViewModel
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        // Настраиваем внешний вид страницы
        SetupPageAppearance();

        // Настраиваем адаптивность
        SetupResponsiveLayout();
    }

    private void SetupPageAppearance()
    {
        // Настройка цветов в зависимости от темы
        if (Application.Current?.RequestedTheme == AppTheme.Dark)
        {
            Shell.SetBackgroundColor(this, Colors.Black);
        }
        else
        {
            Shell.SetBackgroundColor(this, Colors.White);
        }

        // Скрываем навигационную панель по умолчанию, если нужно
        Shell.SetNavBarIsVisible(this, true);
        Shell.SetTabBarIsVisible(this, true);
    }

    private void SetupResponsiveLayout()
    {
        // Подписываемся на изменения размера для адаптивности
        SizeChanged += OnPageSizeChanged;
    }

    #endregion

    #region Event Handlers

    // Обработчик изменений в ViewModel
    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_viewModel.HasError):
                HandleErrorStateChanged();
                break;

            case nameof(_viewModel.ShowHistory):
                HandleHistoryVisibilityChanged();
                break;

            case nameof(_viewModel.IsCalculating):
                HandleCalculatingStateChanged();
                break;
        }
    }

    // Обработчик изменения размера страницы
    private void OnPageSizeChanged(object? sender, EventArgs e)
    {
        AdaptToScreenSize();
    }

    // Обработка состояния ошибки
    private void HandleErrorStateChanged()
    {
        if (_viewModel.HasError)
        {
            // Можно добавить визуальные эффекты для ошибки
            // Например, вибрацию или изменение цвета
            TriggerErrorFeedback();
        }
    }

    // Обработка изменения видимости истории
    private void HandleHistoryVisibilityChanged()
    {
        // Анимация появления/скрытия панели истории
        if (_viewModel.ShowHistory)
        {
            AnimateHistoryPanelIn();
        }
        else
        {
            AnimateHistoryPanelOut();
        }
    }

    // Обработка состояния вычисления
    private void HandleCalculatingStateChanged()
    {
        // Можно добавить дополнительные визуальные эффекты
        if (_viewModel.IsCalculating)
        {
            // Отключаем некоторые взаимодействия во время вычисления
            IsEnabled = true; // Оставляем доступным для отмены
        }
        else
        {
            // Восстанавливаем полную интерактивность
            IsEnabled = true;
        }
    }

    #endregion

    #region Animation Methods

    // Анимация появления панели истории
    private async void AnimateHistoryPanelIn()
    {
        var historyPanel = this.FindByName("HistoryPanel") as Border;
        if (historyPanel != null)
        {
            historyPanel.TranslationX = 300;
            historyPanel.Opacity = 0;

            await Task.WhenAll(
                historyPanel.TranslateTo(0, 0, 250, Easing.CubicOut),
                historyPanel.FadeTo(1, 250)
            );
        }
    }

    // Анимация скрытия панели истории
    private async void AnimateHistoryPanelOut()
    {
        var historyPanel = this.FindByName("HistoryPanel") as Border;
        if (historyPanel != null)
        {
            await Task.WhenAll(
                historyPanel.TranslateTo(300, 0, 200, Easing.CubicIn),
                historyPanel.FadeTo(0, 200)
            );
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
        else if (width < 1200) // Планшеты
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
        // Скрываем историю по умолчанию на мобильных
        if (_viewModel.ShowHistory && Width < 400)
        {
            _viewModel.ShowHistory = false;
        }
    }

    private void AdaptForTabletLayout()
    {
        // Настройки для планшетов
        // Можно показывать историю сбоку
    }

    private void AdaptForDesktopLayout()
    {
        // Настройки для десктопа
        // Показываем все элементы управления
    }

    private void AdaptForLandscapeLayout()
    {
        // Альбомная ориентация - больше места по горизонтали
        // Можем показать историю сбоку
    }

    private void AdaptForPortraitLayout()
    {
        // Портретная ориентация - экономим горизонтальное место
    }

    #endregion

    #region Feedback Methods

    // Тактильная обратная связь при ошибке
    private void TriggerErrorFeedback()
    {
        try
        {
            // Вибрация при ошибке (если поддерживается)
            if (Microsoft.Maui.Authentication.WebAuthenticatorResult.Properties.ContainsKey("HapticFeedback"))
            {
                // Здесь можно добавить вибрацию
                // HapticFeedback.Perform(HapticFeedbackType.Error);
            }

            // Визуальный эффект ошибки
            AnimateErrorEffect();
        }
        catch
        {
            // Игнорируем ошибки обратной связи
        }
    }

    // Анимация ошибки
    private async void AnimateErrorEffect()
    {
        var calculator = this.FindByName("CalculatorComponent");
        if (calculator != null)
        {
            // Легкое "тряхнуть" калькулятор при ошибке
            await calculator.TranslateTo(-10, 0, 50);
            await calculator.TranslateTo(10, 0, 50);
            await calculator.TranslateTo(-5, 0, 50);
            await calculator.TranslateTo(0, 0, 50);
        }
    }

    #endregion

    #region Public Methods

    // Программная установка режима калькулятора
    public async Task SetCalculatorModeAsync(CalculatorMode mode)
    {
        await _viewModel.SetModeAsync(mode);
    }

    // Вставка текста в калькулятор (для интеграции с другими частями приложения)
    public void InsertText(string text)
    {
        _viewModel.InsertText(text);
    }

    // Получение текущего результата
    public string GetCurrentResult()
    {
        return _viewModel.GetCurrentResult();
    }

    #endregion

    #region Page Lifecycle

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Обновляем размеры при появлении страницы
        AdaptToScreenSize();

        // Устанавливаем фокус на калькулятор
        Focus();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Сохраняем состояние при скрытии страницы
        // Можно сохранить историю или настройки
    }

    protected override bool OnBackButtonPressed()
    {
        // Обрабатываем кнопку "назад"
        if (_viewModel.ShowHistory)
        {
            // Сначала скрываем историю
            _viewModel.ShowHistory = false;
            return true; // Предотвращаем закрытие страницы
        }

        return base.OnBackButtonPressed();
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