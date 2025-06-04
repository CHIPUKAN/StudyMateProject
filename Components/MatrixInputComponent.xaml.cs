using StudyMateProject.Models;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace StudyMateProject.Components;

public partial class MatrixInputComponent : ContentView
{
    #region Bindable Properties

    // Матрица для отображения и редактирования
    public static readonly BindableProperty MatrixProperty =
        BindableProperty.Create(nameof(Matrix), typeof(MatrixModel), typeof(MatrixInputComponent), null,
            propertyChanged: OnMatrixChanged);

    public MatrixModel? Matrix
    {
        get => (MatrixModel?)GetValue(MatrixProperty);
        set => SetValue(MatrixProperty, value);
    }

    // Имя матрицы для отображения в заголовке
    public static readonly BindableProperty MatrixNameProperty =
        BindableProperty.Create(nameof(MatrixName), typeof(string), typeof(MatrixInputComponent), "A");

    public string MatrixName
    {
        get => (string)GetValue(MatrixNameProperty);
        set => SetValue(MatrixNameProperty, value);
    }

    // Размер матрицы для отображения
    public static readonly BindableProperty MatrixSizeProperty =
        BindableProperty.Create(nameof(MatrixSize), typeof(string), typeof(MatrixInputComponent), "");

    public string MatrixSize
    {
        get => (string)GetValue(MatrixSizeProperty);
        set => SetValue(MatrixSizeProperty, value);
    }

    // Является ли матрица квадратной
    public static readonly BindableProperty IsSquareMatrixProperty =
        BindableProperty.Create(nameof(IsSquareMatrix), typeof(bool), typeof(MatrixInputComponent), false);

    public bool IsSquareMatrix
    {
        get => (bool)GetValue(IsSquareMatrixProperty);
        set => SetValue(IsSquareMatrixProperty, value);
    }

    // Размер ячеек матрицы
    public static readonly BindableProperty CellSizeProperty =
        BindableProperty.Create(nameof(CellSize), typeof(double), typeof(MatrixInputComponent), 60.0);

    public double CellSize
    {
        get => (double)GetValue(CellSizeProperty);
        set => SetValue(CellSizeProperty, value);
    }

    #endregion

    #region Commands

    // Команды для кнопок управления
    public ICommand ShowSizeDialogCommand { get; }
    public ICommand ShowMatrixMenuCommand { get; }
    public ICommand ClearMatrixCommand { get; }
    public ICommand FillRandomCommand { get; }
    public ICommand MakeIdentityCommand { get; }

    #endregion

    #region Events

    // События для уведомления об изменениях
    public event EventHandler<MatrixModel>? MatrixChanged;
    public event EventHandler? SizeChangeRequested;
    public event EventHandler? MenuRequested;

    #endregion

    #region Private Fields

    private Entry[,]? _entryGrid;

    #endregion

    public MatrixInputComponent()
    {
        InitializeComponent();

        // Инициализируем команды
        ShowSizeDialogCommand = new Command(OnShowSizeDialog);
        ShowMatrixMenuCommand = new Command(OnShowMatrixMenu);
        ClearMatrixCommand = new Command(OnClearMatrix);
        FillRandomCommand = new Command(OnFillRandom);
        MakeIdentityCommand = new Command(OnMakeIdentity);

        // Устанавливаем контекст привязки на себя для команд
        BindingContext = this;
    }

    #region Property Changed Handlers

    // Обработчик изменения матрицы
    private static void OnMatrixChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MatrixInputComponent component && newValue is MatrixModel matrix)
        {
            component.UpdateMatrixDisplay(matrix);
        }
    }

    #endregion

    #region Private Methods

    // Обновление отображения матрицы
    private void UpdateMatrixDisplay(MatrixModel matrix)
    {
        if (matrix == null) return;

        // Обновляем свойства
        MatrixSize = $"{matrix.Rows} × {matrix.Columns}";
        IsSquareMatrix = matrix.IsSquare;

        // Создаем сетку ввода
        CreateInputGrid(matrix.Rows, matrix.Columns);

        // Заполняем значениями
        FillGridWithValues(matrix);
    }

    // Создание сетки для ввода элементов матрицы
    private void CreateInputGrid(int rows, int columns)
    {
        var matrixGrid = this.FindByName("MatrixGrid") as Grid;
        if (matrixGrid == null) return;

        matrixGrid.Children.Clear();
        matrixGrid.RowDefinitions.Clear();
        matrixGrid.ColumnDefinitions.Clear();

        // Создаем определения строк и столбцов
        for (int i = 0; i < rows; i++)
        {
            matrixGrid.RowDefinitions.Add(new RowDefinition { Height = CellSize });
        }

        for (int j = 0; j < columns; j++)
        {
            matrixGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = CellSize });
        }

        // Создаем массив Entry для элементов матрицы
        _entryGrid = new Entry[rows, columns];

        // Создаем Entry элементы
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                var entry = CreateMatrixEntry(i, j);
                _entryGrid[i, j] = entry;

                Grid.SetRow(entry, i);
                Grid.SetColumn(entry, j);
                matrixGrid.Children.Add(entry);
            }
        }
    }

    // Создание Entry для элемента матрицы
    private Entry CreateMatrixEntry(int row, int col)
    {
        var entry = new Entry
        {
            Text = "0",
            Keyboard = Keyboard.Numeric,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            FontSize = 14,
            HeightRequest = CellSize - 8,
            WidthRequest = CellSize - 8,
            BackgroundColor = Colors.White,
            TextColor = Colors.Black
        };

        // Подписываемся на изменения текста
        entry.TextChanged += (sender, e) => OnMatrixElementChanged(row, col, e.NewTextValue);

        // Стиль рамки
        entry.SetValue(Border.StrokeProperty, Colors.Gray);
        entry.SetValue(Border.StrokeThicknessProperty, 1);

        return entry;
    }

    // Заполнение сетки значениями из матрицы
    private void FillGridWithValues(MatrixModel matrix)
    {
        if (_entryGrid == null) return;

        for (int i = 0; i < matrix.Rows && i < _entryGrid.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.Columns && j < _entryGrid.GetLength(1); j++)
            {
                _entryGrid[i, j].Text = matrix[i, j].ToString("F2");
            }
        }
    }

    // Обработчик изменения элемента матрицы
    private void OnMatrixElementChanged(int row, int col, string newValue)
    {
        if (Matrix == null) return;

        // Пытаемся преобразовать введенное значение в число
        if (double.TryParse(newValue, out double value))
        {
            Matrix[row, col] = value;

            // Уведомляем об изменении матрицы
            MatrixChanged?.Invoke(this, Matrix);
        }
    }

    #endregion

    #region Command Handlers

    // Показ диалога изменения размера
    private void OnShowSizeDialog()
    {
        SizeChangeRequested?.Invoke(this, EventArgs.Empty);
    }

    // Показ меню матрицы
    private void OnShowMatrixMenu()
    {
        MenuRequested?.Invoke(this, EventArgs.Empty);
    }

    // Очистка матрицы
    private void OnClearMatrix()
    {
        Matrix?.Clear();
        FillGridWithValues(Matrix);
        MatrixChanged?.Invoke(this, Matrix);
    }

    // Заполнение случайными числами
    private void OnFillRandom()
    {
        Matrix?.FillRandom(-10, 10);
        FillGridWithValues(Matrix);
        MatrixChanged?.Invoke(this, Matrix);
    }

    // Создание единичной матрицы
    private void OnMakeIdentity()
    {
        if (Matrix?.IsSquare == true)
        {
            Matrix.MakeIdentity();
            FillGridWithValues(Matrix);
            MatrixChanged?.Invoke(this, Matrix);
        }
    }

    #endregion

    #region Public Methods

    // Установка новой матрицы
    public void SetMatrix(MatrixModel matrix)
    {
        Matrix = matrix;
    }

    // Получение текущей матрицы
    public MatrixModel? GetMatrix()
    {
        return Matrix;
    }

    // Изменение размера матрицы
    public void ResizeMatrix(int newRows, int newColumns)
    {
        if (Matrix != null)
        {
            var oldMatrix = Matrix;
            Matrix = new MatrixModel(newRows, newColumns, Matrix.Name);

            // Копируем существующие значения
            int minRows = Math.Min(oldMatrix.Rows, newRows);
            int minCols = Math.Min(oldMatrix.Columns, newColumns);

            for (int i = 0; i < minRows; i++)
            {
                for (int j = 0; j < minCols; j++)
                {
                    Matrix[i, j] = oldMatrix[i, j];
                }
            }

            UpdateMatrixDisplay(Matrix);
            MatrixChanged?.Invoke(this, Matrix);
        }
    }

    // Валидация всех введенных значений
    public bool ValidateInput()
    {
        if (_entryGrid == null || Matrix == null) return false;

        for (int i = 0; i < Matrix.Rows; i++)
        {
            for (int j = 0; j < Matrix.Columns; j++)
            {
                if (!double.TryParse(_entryGrid[i, j].Text, out _))
                {
                    // Подсвечиваем неверную ячейку
                    _entryGrid[i, j].BackgroundColor = Colors.LightPink;
                    return false;
                }
                else
                {
                    // Возвращаем нормальный цвет
                    _entryGrid[i, j].BackgroundColor = Colors.White;
                }
            }
        }

        return true;
    }

    // Применение всех изменений из Entry в матрицу
    public void ApplyChanges()
    {
        if (_entryGrid == null || Matrix == null) return;

        for (int i = 0; i < Matrix.Rows; i++)
        {
            for (int j = 0; j < Matrix.Columns; j++)
            {
                if (double.TryParse(_entryGrid[i, j].Text, out double value))
                {
                    Matrix[i, j] = value;
                }
            }
        }

        MatrixChanged?.Invoke(this, Matrix);
    }

    #endregion
}