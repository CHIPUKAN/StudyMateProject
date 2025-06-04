using StudyMateProject.ViewModels;
using StudyMateProject.Models;

namespace StudyMateProject.Views;

public partial class MatrixCalculatorPage : ContentPage
{
    #region Private Fields

    private readonly MatrixCalculatorViewModel _viewModel;

    #endregion

    public MatrixCalculatorPage(MatrixCalculatorViewModel viewModel)
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

        // Обновляем отображение результата при загрузке
        UpdateResultDisplay();
    }

    #endregion

    #region Event Handlers

    // Обработчик изменений в ViewModel
    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_viewModel.ResultMatrix):
                UpdateResultMatrixDisplay();
                break;

            case nameof(_viewModel.HasError):
                HandleErrorStateChanged();
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
            // Анимация ошибки
            AnimateErrorEffect();
        }
    }

    // Обработка состояния вычисления
    private void HandleCalculatingStateChanged()
    {
        // Можно добавить дополнительные визуальные эффекты
    }

    #endregion

    #region Result Display Methods

    // Обновление общего отображения результата
    private void UpdateResultDisplay()
    {
        UpdateResultMatrixDisplay();
    }

    // Обновление отображения матричного результата
    private void UpdateResultMatrixDisplay()
    {
        if (_viewModel.ResultMatrix == null)
        {
            ResultMatrixGrid.Children.Clear();
            ResultMatrixGrid.RowDefinitions.Clear();
            ResultMatrixGrid.ColumnDefinitions.Clear();
            return;
        }

        var matrix = _viewModel.ResultMatrix;
        CreateResultMatrixGrid(matrix);
    }

    // Создание сетки для отображения результирующей матрицы
    private void CreateResultMatrixGrid(MatrixModel matrix)
    {
        ResultMatrixGrid.Children.Clear();
        ResultMatrixGrid.RowDefinitions.Clear();
        ResultMatrixGrid.ColumnDefinitions.Clear();

        // Создаем определения строк и столбцов
        for (int i = 0; i < matrix.Rows; i++)
        {
            ResultMatrixGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        for (int j = 0; j < matrix.Columns; j++)
        {
            ResultMatrixGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        }

        // Создаем элементы для отображения значений
        for (int i = 0; i < matrix.Rows; i++)
        {
            for (int j = 0; j < matrix.Columns; j++)
            {
                var label = CreateResultLabel(matrix[i, j]);

                Grid.SetRow(label, i);
                Grid.SetColumn(label, j);
                ResultMatrixGrid.Children.Add(label);
            }
        }
    }

    // Создание Label для элемента результирующей матрицы
    private Label CreateResultLabel(double value)
    {
        return new Label
        {
            Text = value.ToString("F2"),
            FontSize = 14,
            FontFamily = "Consolas",
            TextColor = Application.Current?.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            Padding = new Thickness(8, 4),
            MinimumWidthRequest = 60,
            MinimumHeightRequest = 30
        };
    }

    #endregion

    #region Animation Methods

    // Анимация ошибки
    private async void AnimateErrorEffect()
    {
        // Легкое встряхивание страницы при ошибке
        await this.TranslateTo(-5, 0, 50);
        await this.TranslateTo(5, 0, 50);
        await this.TranslateTo(-3, 0, 50);
        await this.TranslateTo(0, 0, 50);
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
        if (width < 800) // Мобильные устройства и маленькие планшеты
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
        // Уменьшаем размер ячеек матрицы
        var matrixA = this.FindByName("MatrixInputComponentA") as Components.MatrixInputComponent;
        var matrixB = this.FindByName("MatrixInputComponentB") as Components.MatrixInputComponent;

        if (matrixA != null) matrixA.CellSize = 40;
        if (matrixB != null) matrixB.CellSize = 40;
    }

    private void AdaptForTabletLayout()
    {
        // Настройки для планшетов
        var matrixA = this.FindByName("MatrixInputComponentA") as Components.MatrixInputComponent;
        var matrixB = this.FindByName("MatrixInputComponentB") as Components.MatrixInputComponent;

        if (matrixA != null) matrixA.CellSize = 50;
        if (matrixB != null) matrixB.CellSize = 50;
    }

    private void AdaptForDesktopLayout()
    {
        // Настройки для десктопа
        var matrixA = this.FindByName("MatrixInputComponentA") as Components.MatrixInputComponent;
        var matrixB = this.FindByName("MatrixInputComponentB") as Components.MatrixInputComponent;

        if (matrixA != null) matrixA.CellSize = 60;
        if (matrixB != null) matrixB.CellSize = 60;
    }

    private void AdaptForLandscapeLayout()
    {
        // Альбомная ориентация - можем разместить матрицы горизонтально
    }

    private void AdaptForPortraitLayout()
    {
        // Портретная ориентация - возможно, стоит размещать матрицы вертикально
    }

    #endregion

    #region Public Methods

    // Установка матрицы извне
    public void SetMatrix(MatrixModel matrix, string target = "A")
    {
        _viewModel.SetMatrix(matrix, target);
    }

    // Получение результата
    public MatrixModel? GetResult()
    {
        return _viewModel.GetCurrentResult();
    }

    // Парсинг матрицы из строки
    public bool TryParseMatrix(string matrixString, string target = "A")
    {
        return _viewModel.TryParseMatrix(matrixString, target);
    }

    #endregion

    #region Page Lifecycle

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Обновляем размеры при появлении страницы
        AdaptToScreenSize();

        // Обновляем отображение результата
        UpdateResultDisplay();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Сохраняем состояние при скрытии страницы
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