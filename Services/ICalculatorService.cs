namespace StudyMateProject.Services
{
    public interface ICalculatorService
    {
        // Базовые операции
        double Calculate(string expression);
        bool IsValidExpression(string expression);
        string FormatResult(double result);

        // Математические функции
        double CalculateSquareRoot(double value);
        double CalculatePower(double baseValue, double exponent);

        // Тригонометрические функции
        double CalculateSin(double value, bool isRadianMode = true);
        double CalculateCos(double value, bool isRadianMode = true);
        double CalculateTan(double value, bool isRadianMode = true);

        // Логарифмические функции
        double CalculateNaturalLog(double value);
        double CalculateLog10(double value);
    }
}