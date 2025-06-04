using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using StudyMateProject.Models;
using StudyMateProject.Helpers;
using StudyMateProject.Services;

namespace StudyMateProject.Services
{
    public class MatrixService : IMatrixService
    {
        // Базовые операции с матрицами
        public async Task<CalculationResult> AddMatricesAsync(MatrixModel matrixA, MatrixModel matrixB)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!CanAdd(matrixA, matrixB))
                    {
                        return CreateErrorResult($"{matrixA.Name} + {matrixB.Name}",
                            CalculatorConstants.ERROR_MATRIX_DIMENSION);
                    }

                    var mathNetA = matrixA.ToMathNetMatrix();
                    var mathNetB = matrixB.ToMathNetMatrix();
                    var result = mathNetA + mathNetB;

                    var resultMatrix = new MatrixModel(result, $"{matrixA.Name}+{matrixB.Name}");

                    return new CalculationResult
                    {
                        Expression = $"{matrixA.Name} + {matrixB.Name}",
                        Result = resultMatrix,
                        FormattedResult = MathFormatter.FormatMatrix(resultMatrix),
                        Type = CalculationType.Matrix
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult($"{matrixA.Name} + {matrixB.Name}", ex.Message);
                }
            });
        }

        public async Task<CalculationResult> SubtractMatricesAsync(MatrixModel matrixA, MatrixModel matrixB)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!CanAdd(matrixA, matrixB)) // Условия те же, что для сложения
                    {
                        return CreateErrorResult($"{matrixA.Name} - {matrixB.Name}",
                            CalculatorConstants.ERROR_MATRIX_DIMENSION);
                    }

                    var mathNetA = matrixA.ToMathNetMatrix();
                    var mathNetB = matrixB.ToMathNetMatrix();
                    var result = mathNetA - mathNetB;

                    var resultMatrix = new MatrixModel(result, $"{matrixA.Name}-{matrixB.Name}");

                    return new CalculationResult
                    {
                        Expression = $"{matrixA.Name} - {matrixB.Name}",
                        Result = resultMatrix,
                        FormattedResult = MathFormatter.FormatMatrix(resultMatrix),
                        Type = CalculationType.Matrix
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult($"{matrixA.Name} - {matrixB.Name}", ex.Message);
                }
            });
        }

        public async Task<CalculationResult> MultiplyMatricesAsync(MatrixModel matrixA, MatrixModel matrixB)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!CanMultiply(matrixA, matrixB))
                    {
                        return CreateErrorResult($"{matrixA.Name} × {matrixB.Name}",
                            "Количество столбцов матрицы A должно равняться количеству строк матрицы B");
                    }

                    var mathNetA = matrixA.ToMathNetMatrix();
                    var mathNetB = matrixB.ToMathNetMatrix();
                    var result = mathNetA * mathNetB;

                    var resultMatrix = new MatrixModel(result, $"{matrixA.Name}×{matrixB.Name}");

                    return new CalculationResult
                    {
                        Expression = $"{matrixA.Name} × {matrixB.Name}",
                        Result = resultMatrix,
                        FormattedResult = MathFormatter.FormatMatrix(resultMatrix),
                        Type = CalculationType.Matrix
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult($"{matrixA.Name} × {matrixB.Name}", ex.Message);
                }
            });
        }

        public async Task<CalculationResult> MultiplyByScalarAsync(MatrixModel matrix, double scalar)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var mathNetMatrix = matrix.ToMathNetMatrix();
                    var result = mathNetMatrix * scalar;

                    var resultMatrix = new MatrixModel(result, $"{scalar}×{matrix.Name}");

                    return new CalculationResult
                    {
                        Expression = $"{scalar} × {matrix.Name}",
                        Result = resultMatrix,
                        FormattedResult = MathFormatter.FormatMatrix(resultMatrix),
                        Type = CalculationType.Matrix
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult($"{scalar} × {matrix.Name}", ex.Message);
                }
            });
        }

        // Операции с одной матрицей
        public async Task<CalculationResult> TransposeAsync(MatrixModel matrix)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var mathNetMatrix = matrix.ToMathNetMatrix();
                    var result = mathNetMatrix.Transpose();

                    var resultMatrix = new MatrixModel(result, $"{matrix.Name}ᵀ");

                    return new CalculationResult
                    {
                        Expression = $"{matrix.Name}ᵀ",
                        Result = resultMatrix,
                        FormattedResult = MathFormatter.FormatMatrix(resultMatrix),
                        Type = CalculationType.Matrix
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult($"{matrix.Name}ᵀ", ex.Message);
                }
            });
        }

        public async Task<CalculationResult> InverseAsync(MatrixModel matrix)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!IsSquare(matrix))
                    {
                        return CreateErrorResult($"{matrix.Name}⁻¹", CalculatorConstants.ERROR_NOT_SQUARE_MATRIX);
                    }

                    var mathNetMatrix = matrix.ToMathNetMatrix();

                    if (IsSingular(matrix))
                    {
                        return CreateErrorResult($"{matrix.Name}⁻¹", CalculatorConstants.ERROR_SINGULAR_MATRIX);
                    }

                    var result = mathNetMatrix.Inverse();
                    var resultMatrix = new MatrixModel(result, $"{matrix.Name}⁻¹");

                    return new CalculationResult
                    {
                        Expression = $"{matrix.Name}⁻¹",
                        Result = resultMatrix,
                        FormattedResult = MathFormatter.FormatMatrix(resultMatrix),
                        Type = CalculationType.Matrix
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult($"{matrix.Name}⁻¹", ex.Message);
                }
            });
        }

        public async Task<CalculationResult> DeterminantAsync(MatrixModel matrix)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!IsSquare(matrix))
                    {
                        return CreateErrorResult($"det({matrix.Name})", CalculatorConstants.ERROR_NOT_SQUARE_MATRIX);
                    }

                    var mathNetMatrix = matrix.ToMathNetMatrix();
                    var result = mathNetMatrix.Determinant();

                    return new CalculationResult
                    {
                        Expression = $"det({matrix.Name})",
                        Result = result,
                        FormattedResult = MathFormatter.FormatNumber(result),
                        Type = CalculationType.Matrix
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult($"det({matrix.Name})", ex.Message);
                }
            });
        }

        public async Task<CalculationResult> TraceAsync(MatrixModel matrix)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!IsSquare(matrix))
                    {
                        return CreateErrorResult($"tr({matrix.Name})", CalculatorConstants.ERROR_NOT_SQUARE_MATRIX);
                    }

                    var mathNetMatrix = matrix.ToMathNetMatrix();
                    var result = mathNetMatrix.Trace();

                    return new CalculationResult
                    {
                        Expression = $"tr({matrix.Name})",
                        Result = result,
                        FormattedResult = MathFormatter.FormatNumber(result),
                        Type = CalculationType.Matrix
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult($"tr({matrix.Name})", ex.Message);
                }
            });
        }

        public async Task<CalculationResult> RankAsync(MatrixModel matrix)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var mathNetMatrix = matrix.ToMathNetMatrix();
                    var result = mathNetMatrix.Rank();

                    return new CalculationResult
                    {
                        Expression = $"rank({matrix.Name})",
                        Result = result,
                        FormattedResult = result.ToString(),
                        Type = CalculationType.Matrix
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult($"rank({matrix.Name})", ex.Message);
                }
            });
        }

        // Создание специальных матриц
        public MatrixModel CreateIdentityMatrix(int size)
        {
            var matrix = new MatrixModel(size, size, "I");
            matrix.MakeIdentity();
            return matrix;
        }

        public MatrixModel CreateZeroMatrix(int rows, int columns)
        {
            var matrix = new MatrixModel(rows, columns, "0");
            matrix.Clear();
            return matrix;
        }

        public MatrixModel CreateRandomMatrix(int rows, int columns, double min = -10, double max = 10)
        {
            var matrix = new MatrixModel(rows, columns, "R");
            matrix.FillRandom(min, max);
            return matrix;
        }

        // Проверки свойств матриц
        public bool CanMultiply(MatrixModel matrixA, MatrixModel matrixB)
        {
            return matrixA.Columns == matrixB.Rows;
        }

        public bool CanAdd(MatrixModel matrixA, MatrixModel matrixB)
        {
            return matrixA.Rows == matrixB.Rows && matrixA.Columns == matrixB.Columns;
        }

        public bool IsSquare(MatrixModel matrix)
        {
            return matrix.IsSquare;
        }

        public bool IsSingular(MatrixModel matrix)
        {
            if (!IsSquare(matrix)) return false;

            try
            {
                var mathNetMatrix = matrix.ToMathNetMatrix();
                var det = mathNetMatrix.Determinant();
                return Math.Abs(det) < CalculatorConstants.DEFAULT_TOLERANCE;
            }
            catch
            {
                return true;
            }
        }

        public bool IsSymmetric(MatrixModel matrix)
        {
            if (!IsSquare(matrix)) return false;

            for (int i = 0; i < matrix.Rows; i++)
            {
                for (int j = 0; j < matrix.Columns; j++)
                {
                    if (Math.Abs(matrix[i, j] - matrix[j, i]) > CalculatorConstants.DEFAULT_TOLERANCE)
                        return false;
                }
            }
            return true;
        }

        // Решение систем уравнений
        public async Task<CalculationResult> SolveSystemAsync(MatrixModel coefficients, MatrixModel constants)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!IsSquare(coefficients))
                    {
                        return CreateErrorResult("Solve system", "Матрица коэффициентов должна быть квадратной");
                    }

                    if (coefficients.Rows != constants.Rows)
                    {
                        return CreateErrorResult("Solve system", "Несовместимые размеры матриц");
                    }

                    var mathNetCoeff = coefficients.ToMathNetMatrix();
                    var mathNetConst = constants.ToMathNetMatrix();

                    var result = mathNetCoeff.Solve(mathNetConst);
                    var resultMatrix = new MatrixModel(result, "X");

                    return new CalculationResult
                    {
                        Expression = $"{coefficients.Name} × X = {constants.Name}",
                        Result = resultMatrix,
                        FormattedResult = MathFormatter.FormatMatrix(resultMatrix),
                        Type = CalculationType.Matrix
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult("Solve system", ex.Message);
                }
            });
        }

        // Собственные значения и векторы
        public async Task<CalculationResult> EigenvaluesAsync(MatrixModel matrix)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!IsSquare(matrix))
                    {
                        return CreateErrorResult($"eigenvalues({matrix.Name})", CalculatorConstants.ERROR_NOT_SQUARE_MATRIX);
                    }

                    var mathNetMatrix = matrix.ToMathNetMatrix();
                    var evd = mathNetMatrix.Evd();
                    var eigenvalues = evd.EigenValues;

                    var realParts = eigenvalues.Real();
                    var resultMatrix = new MatrixModel(Matrix<double>.Build.DenseOfColumnVectors(realParts), "λ");

                    return new CalculationResult
                    {
                        Expression = $"eigenvalues({matrix.Name})",
                        Result = resultMatrix,
                        FormattedResult = MathFormatter.FormatMatrix(resultMatrix),
                        Type = CalculationType.Matrix
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult($"eigenvalues({matrix.Name})", ex.Message);
                }
            });
        }

        public async Task<CalculationResult> EigenvectorsAsync(MatrixModel matrix)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!IsSquare(matrix))
                    {
                        return CreateErrorResult($"eigenvectors({matrix.Name})", CalculatorConstants.ERROR_NOT_SQUARE_MATRIX);
                    }

                    var mathNetMatrix = matrix.ToMathNetMatrix();
                    var evd = mathNetMatrix.Evd();
                    var eigenvectors = evd.EigenVectors;

                    var resultMatrix = new MatrixModel(eigenvectors, "V");

                    return new CalculationResult
                    {
                        Expression = $"eigenvectors({matrix.Name})",
                        Result = resultMatrix,
                        FormattedResult = MathFormatter.FormatMatrix(resultMatrix),
                        Type = CalculationType.Matrix
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult($"eigenvectors({matrix.Name})", ex.Message);
                }
            });
        }

        // LU разложение
        public async Task<CalculationResult> LUDecompositionAsync(MatrixModel matrix)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!IsSquare(matrix))
                    {
                        return CreateErrorResult($"LU({matrix.Name})", CalculatorConstants.ERROR_NOT_SQUARE_MATRIX);
                    }

                    var mathNetMatrix = matrix.ToMathNetMatrix();
                    var lu = mathNetMatrix.LU();

                    var details = $"L:\n{MathFormatter.FormatMatrix(new MatrixModel(lu.L, "L"))}\n\n" +
                                 $"U:\n{MathFormatter.FormatMatrix(new MatrixModel(lu.U, "U"))}";

                    return new CalculationResult
                    {
                        Expression = $"LU({matrix.Name})",
                        Result = new { L = new MatrixModel(lu.L, "L"), U = new MatrixModel(lu.U, "U") },
                        FormattedResult = details,
                        Type = CalculationType.Matrix,
                        Details = details
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult($"LU({matrix.Name})", ex.Message);
                }
            });
        }

        // QR разложение
        public async Task<CalculationResult> QRDecompositionAsync(MatrixModel matrix)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var mathNetMatrix = matrix.ToMathNetMatrix();
                    var qr = mathNetMatrix.QR();

                    var details = $"Q:\n{MathFormatter.FormatMatrix(new MatrixModel(qr.Q, "Q"))}\n\n" +
                                 $"R:\n{MathFormatter.FormatMatrix(new MatrixModel(qr.R, "R"))}";

                    return new CalculationResult
                    {
                        Expression = $"QR({matrix.Name})",
                        Result = new { Q = new MatrixModel(qr.Q, "Q"), R = new MatrixModel(qr.R, "R") },
                        FormattedResult = details,
                        Type = CalculationType.Matrix,
                        Details = details
                    };
                }
                catch (Exception ex)
                {
                    return CreateErrorResult($"QR({matrix.Name})", ex.Message);
                }
            });
        }

        // Валидация матрицы
        public bool IsValidMatrix(MatrixModel matrix)
        {
            if (matrix == null) return false;
            if (matrix.Rows <= 0 || matrix.Columns <= 0) return false;
            if (matrix.Values == null) return false;

            // Проверяем на NaN и бесконечность
            for (int i = 0; i < matrix.Rows; i++)
            {
                for (int j = 0; j < matrix.Columns; j++)
                {
                    if (double.IsNaN(matrix[i, j]) || double.IsInfinity(matrix[i, j]))
                        return false;
                }
            }

            return true;
        }

        // Парсинг матрицы из строки
        public MatrixModel ParseMatrixFromString(string matrixString)
        {
            try
            {
                // Формат: [[1,2,3],[4,5,6],[7,8,9]]
                matrixString = matrixString.Trim();
                if (!matrixString.StartsWith("[[") || !matrixString.EndsWith("]]"))
                    throw new ArgumentException("Неверный формат матрицы");

                var rowPattern = @"\[([^\]]+)\]";
                var matches = Regex.Matches(matrixString, rowPattern);

                if (matches.Count == 0)
                    throw new ArgumentException("Матрица не содержит строк");

                var rows = matches.Count;
                var firstRowElements = matches[0].Groups[1].Value.Split(',')
                    .Select(s => double.Parse(s.Trim())).ToArray();
                var columns = firstRowElements.Length;

                var matrix = new MatrixModel(rows, columns);

                for (int i = 0; i < rows; i++)
                {
                    var elements = matches[i].Groups[1].Value.Split(',')
                        .Select(s => double.Parse(s.Trim())).ToArray();

                    if (elements.Length != columns)
                        throw new ArgumentException("Все строки должны иметь одинаковое количество элементов");

                    for (int j = 0; j < columns; j++)
                    {
                        matrix[i, j] = elements[j];
                    }
                }

                return matrix;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Ошибка парсинга матрицы: {ex.Message}");
            }
        }

        // Форматирование матрицы для отображения
        public string FormatMatrix(MatrixModel matrix, int decimalPlaces = 2)
        {
            return MathFormatter.FormatMatrix(matrix, decimalPlaces);
        }

        // Вспомогательный метод для создания результата с ошибкой
        private CalculationResult CreateErrorResult(string expression, string errorMessage)
        {
            return new CalculationResult
            {
                Expression = expression,
                HasError = true,
                ErrorMessage = errorMessage,
                FormattedResult = CalculatorConstants.ERROR_PREFIX + errorMessage,
                Type = CalculationType.Matrix
            };
        }
    }
}