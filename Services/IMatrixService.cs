using System;
using System.Threading.Tasks;
using StudyMateProject.Models;

namespace StudyMateProject.Services
{
    // Интерфейс для операций с матрицами
    public interface IMatrixService
    {
        // Базовые операции с матрицами
        Task<CalculationResult> AddMatricesAsync(MatrixModel matrixA, MatrixModel matrixB);
        Task<CalculationResult> SubtractMatricesAsync(MatrixModel matrixA, MatrixModel matrixB);
        Task<CalculationResult> MultiplyMatricesAsync(MatrixModel matrixA, MatrixModel matrixB);
        Task<CalculationResult> MultiplyByScalarAsync(MatrixModel matrix, double scalar);

        // Операции с одной матрицей
        Task<CalculationResult> TransposeAsync(MatrixModel matrix);
        Task<CalculationResult> InverseAsync(MatrixModel matrix);
        Task<CalculationResult> DeterminantAsync(MatrixModel matrix);
        Task<CalculationResult> TraceAsync(MatrixModel matrix);
        Task<CalculationResult> RankAsync(MatrixModel matrix);

        // Создание специальных матриц
        MatrixModel CreateIdentityMatrix(int size);
        MatrixModel CreateZeroMatrix(int rows, int columns);
        MatrixModel CreateRandomMatrix(int rows, int columns, double min = -10, double max = 10);

        // Проверки свойств матриц
        bool CanMultiply(MatrixModel matrixA, MatrixModel matrixB);
        bool CanAdd(MatrixModel matrixA, MatrixModel matrixB);
        bool IsSquare(MatrixModel matrix);
        bool IsSingular(MatrixModel matrix);
        bool IsSymmetric(MatrixModel matrix);

        // Решение систем уравнений
        Task<CalculationResult> SolveSystemAsync(MatrixModel coefficients, MatrixModel constants);

        // Собственные значения и векторы (для продвинутых операций)
        Task<CalculationResult> EigenvaluesAsync(MatrixModel matrix);
        Task<CalculationResult> EigenvectorsAsync(MatrixModel matrix);

        // LU разложение
        Task<CalculationResult> LUDecompositionAsync(MatrixModel matrix);

        // QR разложение
        Task<CalculationResult> QRDecompositionAsync(MatrixModel matrix);

        // Валидация матрицы
        bool IsValidMatrix(MatrixModel matrix);

        // Парсинг матрицы из строки
        MatrixModel ParseMatrixFromString(string matrixString);

        // Форматирование матрицы для отображения
        string FormatMatrix(MatrixModel matrix, int decimalPlaces = 2);
    }
}