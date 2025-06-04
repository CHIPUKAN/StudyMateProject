using System;
using System.Collections.Generic;
using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace StudyMateProject.Models
{
    // Модель для работы с матрицами
    public class MatrixModel
    {
        // Уникальный идентификатор матрицы
        public Guid Id { get; set; } = Guid.NewGuid();

        // Имя матрицы (A, B, C и т.д.)
        public string Name { get; set; } = string.Empty;

        // Количество строк
        public int Rows { get; set; }

        // Количество столбцов
        public int Columns { get; set; }

        // Двумерный массив значений матрицы
        public double[,] Values { get; set; }

        // Конструктор пустой матрицы
        public MatrixModel(int rows, int columns, string name = "")
        {
            Rows = rows;
            Columns = columns;
            Name = name;
            Values = new double[rows, columns];
        }

        // Конструктор из двумерного массива
        public MatrixModel(double[,] values, string name = "")
        {
            Values = values;
            Rows = values.GetLength(0);
            Columns = values.GetLength(1);
            Name = name;
        }

        // Конструктор из MathNet матрицы
        public MatrixModel(Matrix<double> matrix, string name = "")
        {
            Name = name;
            Rows = matrix.RowCount;
            Columns = matrix.ColumnCount;
            Values = new double[Rows, Columns];

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    Values[i, j] = matrix[i, j];
                }
            }
        }

        // Преобразует в MathNet матрицу для вычислений
        public Matrix<double> ToMathNetMatrix()
        {
            return Matrix<double>.Build.DenseOfArray(Values);
        }

        // Получает или устанавливает элемент матрицы
        public double this[int row, int column]
        {
            get => Values[row, column];
            set => Values[row, column] = value;
        }

        // Проверяет, является ли матрица квадратной
        public bool IsSquare => Rows == Columns;

        // Заполняет матрицу случайными числами
        public void FillRandom(double min = -10, double max = 10)
        {
            var random = new Random();
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    Values[i, j] = Math.Round(random.NextDouble() * (max - min) + min, 2);
                }
            }
        }

        // Заполняет матрицу нулями
        public void Clear()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    Values[i, j] = 0;
                }
            }
        }

        // Создает единичную матрицу
        public void MakeIdentity()
        {
            if (!IsSquare) return;

            Clear();
            for (int i = 0; i < Rows; i++)
            {
                Values[i, i] = 1;
            }
        }

        // Возвращает строковое представление матрицы
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Matrix {Name} ({Rows}x{Columns}):");

            for (int i = 0; i < Rows; i++)
            {
                sb.Append("│");
                for (int j = 0; j < Columns; j++)
                {
                    sb.Append($"{Values[i, j],8:F2}");
                    if (j < Columns - 1) sb.Append(" ");
                }
                sb.AppendLine("│");
            }

            return sb.ToString();
        }

        // Создает копию матрицы
        public MatrixModel Clone()
        {
            var clonedValues = new double[Rows, Columns];
            Array.Copy(Values, clonedValues, Values.Length);
            return new MatrixModel(clonedValues, Name + "_copy");
        }

        // Получает список всех элементов как строк для UI
        public List<List<string>> GetDisplayValues()
        {
            var result = new List<List<string>>();
            for (int i = 0; i < Rows; i++)
            {
                var row = new List<string>();
                for (int j = 0; j < Columns; j++)
                {
                    row.Add(Values[i, j].ToString("F2"));
                }
                result.Add(row);
            }
            return result;
        }
    }
}