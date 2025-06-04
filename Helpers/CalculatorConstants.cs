using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudyMateProject.Helpers
{
    public class CalculatorConstants
    {
        // Математические константы
        public const double PI = Math.PI;
        public const double E = Math.E;
        public const double GOLDEN_RATIO = 1.618033988749895;
        public const double EULER_GAMMA = 0.5772156649015329;

        // Точность вычислений
        public const double DEFAULT_TOLERANCE = 1e-10;
        public const double INTEGRATION_TOLERANCE = 1e-8;
        public const double LIMIT_TOLERANCE = 1e-12;

        // Настройки отображения
        public const int DEFAULT_DECIMAL_PLACES = 10;
        public const int MATRIX_DECIMAL_PLACES = 4;
        public const int SCIENTIFIC_DECIMAL_PLACES = 8;

        // Лимиты вычислений
        public const int MAX_FACTORIAL = 170; // После 170! получается бесконечность
        public const int MAX_INTEGRATION_STEPS = 100000;
        public const int MAX_SERIES_TERMS = 50000;
        public const int MAX_MATRIX_SIZE = 100;

        // Константы для UI
        public const string INFINITY_SYMBOL = "∞";
        public const string PI_SYMBOL = "π";
        public const string DEGREES_SYMBOL = "°";
        public const string ERROR_PREFIX = "Error: ";

        // Кнопки калькулятора
        public static readonly string[] NUMBER_BUTTONS = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        public static readonly string[] OPERATOR_BUTTONS = { "+", "-", "×", "÷", "=" };
        public static readonly string[] FUNCTION_BUTTONS = { "sin", "cos", "tan", "log", "ln", "√", "x²", "xʸ" };
        public static readonly string[] SPECIAL_BUTTONS = { "C", "CE", "±", "%", "π", "e", "(", ")", ".", "⌫" };

        // Цвета для кнопок (в формате hex)
        public static readonly Dictionary<string, string> BUTTON_COLORS = new()
        {
            ["Numbers"] = "#2196F3",      // Синий
            ["Operators"] = "#FF9800",    // Оранжевый
            ["Functions"] = "#9C27B0",    // Фиолетовый
            ["Special"] = "#4CAF50",      // Зеленый
            ["Clear"] = "#F44336",        // Красный
            ["Equals"] = "#FF5722"        // Красно-оранжевый
        };

        // Размеры для разных режимов
        public static readonly Dictionary<string, double> BUTTON_SIZES = new()
        {
            ["FullScreen"] = 60.0,
            ["Mini"] = 40.0,
            ["Tablet"] = 80.0
        };

        // Сообщения об ошибках
        public const string ERROR_DIVISION_BY_ZERO = "Деление на ноль";
        public const string ERROR_INVALID_EXPRESSION = "Неверное выражение";
        public const string ERROR_DOMAIN_ERROR = "Ошибка области определения";
        public const string ERROR_OVERFLOW = "Переполнение";
        public const string ERROR_MATRIX_DIMENSION = "Несовместимые размеры матриц";
        public const string ERROR_SINGULAR_MATRIX = "Матрица вырожденная";
        public const string ERROR_NOT_SQUARE_MATRIX = "Матрица не квадратная";
        public const string ERROR_CONVERGENCE = "Ряд не сходится";
        public const string ERROR_INVALID_FUNCTION = "Неверная функция";

        // Форматы чисел
        public const string STANDARD_FORMAT = "F{0}";
        public const string SCIENTIFIC_FORMAT = "E{0}";
        public const string PERCENTAGE_FORMAT = "P{0}";

        // Регулярные выражения для валидации
        public const string NUMBER_PATTERN = @"^-?\d*\.?\d+([eE][+-]?\d+)?$";
        public const string VARIABLE_PATTERN = @"^[a-zA-Z][a-zA-Z0-9]*$";
        public const string FUNCTION_PATTERN = @"^[a-zA-Z][a-zA-Z0-9]*\([^)]*\)$";

        // Настройки истории
        public const int DEFAULT_HISTORY_SIZE = 100;
        public const int MINI_HISTORY_SIZE = 20;

        // Названия функций для DynamicExpresso
        public static readonly Dictionary<string, string> FUNCTION_MAPPINGS = new()
        {
            ["sin"] = "Sin",
            ["cos"] = "Cos",
            ["tan"] = "Tan",
            ["asin"] = "Asin",
            ["acos"] = "Acos",
            ["atan"] = "Atan",
            ["log"] = "Log10",
            ["ln"] = "Log",
            ["sqrt"] = "Sqrt",
            ["abs"] = "Abs",
            ["exp"] = "Exp",
            ["floor"] = "Floor",
            ["ceil"] = "Ceiling",
            ["round"] = "Round"
        };

        // Единицы измерения для конвертера
        public static readonly Dictionary<string, Dictionary<string, double>> CONVERSION_FACTORS = new()
        {
            ["Length"] = new()
            {
                ["m"] = 1.0,
                ["cm"] = 0.01,
                ["mm"] = 0.001,
                ["km"] = 1000.0,
                ["in"] = 0.0254,
                ["ft"] = 0.3048,
                ["yd"] = 0.9144,
                ["mi"] = 1609.34
            },
            ["Weight"] = new()
            {
                ["kg"] = 1.0,
                ["g"] = 0.001,
                ["lb"] = 0.453592,
                ["oz"] = 0.0283495
            },
            ["Temperature"] = new()
            {
                // Специальная обработка для температуры
                ["C"] = 1.0,
                ["F"] = 1.0,
                ["K"] = 1.0
            }
        };
    }
}
