using DynamicExpresso;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudyMateProject.Helpers
{
    public class MathExpressionParser
    {
        private readonly Interpreter _interpreter;
        private readonly Dictionary<string, double> _variables;
        private readonly Dictionary<string, Func<double, double>> _functions;

        public MathExpressionParser()
        {
            _interpreter = new Interpreter();
            _variables = new Dictionary<string, double>();
            _functions = new Dictionary<string, Func<double, double>>();

            InitializeStandardFunctions();
            InitializeConstants();
        }

        // Инициализирует стандартные математические функции
        private void InitializeStandardFunctions()
        {
            // Тригонометрические функции (в градусах)
            _interpreter.SetFunction("sin", new Func<double, double>(x => Math.Sin(x * Math.PI / 180)));
            _interpreter.SetFunction("cos", new Func<double, double>(x => Math.Cos(x * Math.PI / 180)));
            _interpreter.SetFunction("tan", new Func<double, double>(x => Math.Tan(x * Math.PI / 180)));

            // Тригонометрические функции (в радианах)
            _interpreter.SetFunction("sinr", new Func<double, double>(Math.Sin));
            _interpreter.SetFunction("cosr", new Func<double, double>(Math.Cos));
            _interpreter.SetFunction("tanr", new Func<double, double>(Math.Tan));

            // Обратные тригонометрические функции
            _interpreter.SetFunction("asin", new Func<double, double>(x => Math.Asin(x) * 180 / Math.PI));
            _interpreter.SetFunction("acos", new Func<double, double>(x => Math.Acos(x) * 180 / Math.PI));
            _interpreter.SetFunction("atan", new Func<double, double>(x => Math.Atan(x) * 180 / Math.PI));

            // Логарифмические функции
            _interpreter.SetFunction("log", new Func<double, double>(Math.Log10));
            _interpreter.SetFunction("ln", new Func<double, double>(Math.Log));
            _interpreter.SetFunction("log2", new Func<double, double>(x => Math.Log(x, 2)));

            // Степенные функции
            _interpreter.SetFunction("sqrt", new Func<double, double>(Math.Sqrt));
            _interpreter.SetFunction("cbrt", new Func<double, double>(x => Math.Pow(x, 1.0 / 3.0)));
            _interpreter.SetFunction("exp", new Func<double, double>(Math.Exp));
            _interpreter.SetFunction("pow", new Func<double, double, double>(Math.Pow));

            // Другие функции
            _interpreter.SetFunction("abs", new Func<double, double>(Math.Abs));
            _interpreter.SetFunction("floor", new Func<double, double>(Math.Floor));
            _interpreter.SetFunction("ceil", new Func<double, double>(Math.Ceiling));
            _interpreter.SetFunction("round", new Func<double, double>(Math.Round));
            _interpreter.SetFunction("sign", new Func<double, double>(x => Math.Sign(x)));

            // Факториал
            _interpreter.SetFunction("fact", new Func<double, double>(Factorial));

            // Гиперболические функции
            _interpreter.SetFunction("sinh", new Func<double, double>(Math.Sinh));
            _interpreter.SetFunction("cosh", new Func<double, double>(Math.Cosh));
            _interpreter.SetFunction("tanh", new Func<double, double>(Math.Tanh));
        }

        // Инициализирует математические константы
        private void InitializeConstants()
        {
            _interpreter.SetVariable("pi", Math.PI);
            _interpreter.SetVariable("e", Math.E);
            _interpreter.SetVariable("phi", CalculatorConstants.GOLDEN_RATIO);
            _interpreter.SetVariable("gamma", CalculatorConstants.EULER_GAMMA);
        }

        // Вычисляет математическое выражение
        public double Evaluate(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException("Выражение не может быть пустым");

            try
            {
                // Предварительная обработка выражения
                expression = PreprocessExpression(expression);

                // Вычисление
                var result = _interpreter.Eval(expression);

                return Convert.ToDouble(result);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка вычисления: {ex.Message}", ex);
            }
        }

        // Предварительная обработка выражения
        private string PreprocessExpression(string expression)
        {
            // Удаляем пробелы
            expression = expression.Replace(" ", "");

            // Заменяем символы
            expression = expression.Replace("×", "*");
            expression = expression.Replace("÷", "/");
            expression = expression.Replace("π", "pi");
            expression = expression.Replace("°", "*pi/180");

            // Обрабатываем степени
            expression = Regex.Replace(expression, @"(\d+|\))²", "$1^2");
            expression = Regex.Replace(expression, @"(\d+|\))³", "$1^3");

            // Добавляем знаки умножения там, где они пропущены
            expression = Regex.Replace(expression, @"(\d)(\()", "$1*$2");
            expression = Regex.Replace(expression, @"(\))(\d)", "$1*$2");
            expression = Regex.Replace(expression, @"(\))\(", "$1*$2");

            // Обрабатываем корни
            expression = Regex.Replace(expression, @"√(\d+|\([^)]+\))", "sqrt($1)");

            return expression;
        }

        // Проверяет валидность выражения
        public bool IsValidExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            try
            {
                expression = PreprocessExpression(expression);
                _interpreter.Parse(expression);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Устанавливает переменную
        public void SetVariable(string name, double value)
        {
            _variables[name] = value;
            _interpreter.SetVariable(name, value);
        }

        // Получает значение переменной
        public double? GetVariable(string name)
        {
            return _variables.ContainsKey(name) ? _variables[name] : null;
        }

        // Устанавливает пользовательскую функцию
        public void SetFunction(string name, Func<double, double> function)
        {
            _functions[name] = function;
            _interpreter.SetFunction(name, function);
        }

        // Вычисляет функцию в точке
        public double EvaluateFunction(string function, string variable, double value)
        {
            SetVariable(variable, value);
            return Evaluate(function);
        }

        // Получает список всех доступных функций
        public List<string> GetAvailableFunctions()
        {
            var functions = new List<string>();
            functions.AddRange(CalculatorConstants.FUNCTION_MAPPINGS.Keys);
            functions.AddRange(_functions.Keys);
            return functions;
        }

        // Получает список всех переменных
        public Dictionary<string, double> GetVariables()
        {
            return new Dictionary<string, double>(_variables);
        }

        // Очищает все переменные
        public void ClearVariables()
        {
            _variables.Clear();
            // Переинициализируем константы
            InitializeConstants();
        }

        // Парсит выражение и возвращает информацию о нем
        public ExpressionInfo ParseExpressionInfo(string expression)
        {
            var info = new ExpressionInfo
            {
                OriginalExpression = expression,
                ProcessedExpression = PreprocessExpression(expression),
                IsValid = IsValidExpression(expression)
            };

            if (!info.IsValid)
                return info;

            // Анализируем выражение
            info.ContainsTrigonometry = ContainsTrigonometry(expression);
            info.ContainsLogarithms = ContainsLogarithms(expression);
            info.ContainsRoots = ContainsRoots(expression);
            info.Variables = ExtractVariables(expression);

            return info;
        }

        // Вспомогательные методы для анализа выражений
        private bool ContainsTrigonometry(string expression)
        {
            return Regex.IsMatch(expression, @"\b(sin|cos|tan|asin|acos|atan|sinh|cosh|tanh)\b");
        }

        private bool ContainsLogarithms(string expression)
        {
            return Regex.IsMatch(expression, @"\b(log|ln|log2)\b");
        }

        private bool ContainsRoots(string expression)
        {
            return expression.Contains("sqrt") || expression.Contains("cbrt") || expression.Contains("√");
        }

        private List<string> ExtractVariables(string expression)
        {
            var variables = new HashSet<string>();
            var matches = Regex.Matches(expression, @"\b[a-zA-Z][a-zA-Z0-9]*\b");

            foreach (Match match in matches)
            {
                var word = match.Value;
                if (!CalculatorConstants.FUNCTION_MAPPINGS.ContainsKey(word) &&
                    !_functions.ContainsKey(word) &&
                    word != "pi" && word != "e" && word != "phi" && word != "gamma")
                {
                    variables.Add(word);
                }
            }

            return new List<string>(variables);
        }

        // Вычисляет факториал
        private double Factorial(double n)
        {
            if (n < 0 || n != Math.Floor(n))
                throw new ArgumentException("Факториал определен только для неотрицательных целых чисел");

            if (n > CalculatorConstants.MAX_FACTORIAL)
                return double.PositiveInfinity;

            double result = 1;
            for (int i = 2; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }
    }

    // Информация о математическом выражении
    public class ExpressionInfo
    {
        public string OriginalExpression { get; set; } = string.Empty;
        public string ProcessedExpression { get; set; } = string.Empty;
        public bool IsValid { get; set; } = false;
        public bool ContainsTrigonometry { get; set; } = false;
        public bool ContainsLogarithms { get; set; } = false;
        public bool ContainsRoots { get; set; } = false;
        public List<string> Variables { get; set; } = new List<string>();
    }
}
