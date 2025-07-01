using System;
using System.Globalization;
using System.Text;
using UserCRUD.Models;

namespace UserCRUD
{
    /// <summary>
    /// Result of quadratic equation solving
    /// </summary>
    public enum QuadraticSolutionType
    {
        TwoRealRoots,
        OneRealRoot,
        NoRealRoots,
        InfiniteRoots,
        NotQuadratic
    }

    /// <summary>
    /// Comprehensive quadratic equation solver for Task 3.4
    /// Solves equations of the form ax² + bx + c = 0
    /// </summary>
    public static class QuadraticSolver
    {
        #region Core Solver Function (Task 3.4 Main Implementation)

        /// <summary>
        /// Main function to solve quadratic equations - Task 3.4 implementation
        /// Solves ax² + bx + c = 0 and returns roots as a formatted string
        /// </summary>
        /// <param name="a">Coefficient of x²</param>
        /// <param name="b">Coefficient of x</param>
        /// <param name="c">Constant term</param>
        /// <returns>String representation of the roots</returns>
        public static string SolveQuadratic(double a, double b, double c)
        {
            try
            {
                // Input validation
                if (double.IsNaN(a) || double.IsNaN(b) || double.IsNaN(c))
                {
                    return "Error: Invalid input - coefficients cannot be NaN";
                }

                if (double.IsInfinity(a) || double.IsInfinity(b) || double.IsInfinity(c))
                {
                    return "Error: Invalid input - coefficients cannot be infinite";
                }

                // Handle special cases
                if (Math.Abs(a) < 1e-10) // a ≈ 0
                {
                    return SolveLinear(b, c);
                }

                // Calculate discriminant
                double discriminant = b * b - 4 * a * c;

                // Solve based on discriminant
                if (discriminant > 0)
                {
                    // Two distinct real roots
                    double sqrtDiscriminant = Math.Sqrt(discriminant);
                    double root1 = (-b + sqrtDiscriminant) / (2 * a);
                    double root2 = (-b - sqrtDiscriminant) / (2 * a);

                    // Order roots (smaller first)
                    if (root1 > root2)
                    {
                        (root1, root2) = (root2, root1);
                    }

                    return FormatTwoRoots(root1, root2);
                }
                else if (Math.Abs(discriminant) < 1e-10) // discriminant ≈ 0
                {
                    // One repeated real root
                    double root = -b / (2 * a);
                    return FormatOneRoot(root);
                }
                else
                {
                    // Complex roots
                    double realPart = -b / (2 * a);
                    double imaginaryPart = Math.Sqrt(-discriminant) / (2 * a);
                    return FormatComplexRoots(realPart, imaginaryPart);
                }
            }
            catch (Exception ex)
            {
                return $"Error solving quadratic equation: {ex.Message}";
            }
        }

        #endregion

        #region Alternative Function Signatures

        /// <summary>
        /// Solves quadratic equation and returns detailed result object
        /// </summary>
        /// <param name="a">Coefficient of x²</param>
        /// <param name="b">Coefficient of x</param>
        /// <param name="c">Constant term</param>
        /// <returns>QuadraticResult object with detailed information</returns>
        public static QuadraticResult SolveQuadraticDetailed(double a, double b, double c)
        {
            var result = new QuadraticResult
            {
                A = a,
                B = b,
                C = c,
                DateCreated = DateTime.Now
            };

            try
            {
                result.CalculateRoots();
                return result;
            }
            catch (Exception ex)
            {
                result.Notes = $"Error: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Solves quadratic equation from string input
        /// </summary>
        /// <param name="equation">Equation string (e.g., "x^2 + 2x + 1 = 0")</param>
        /// <returns>String representation of the roots</returns>
        public static string SolveQuadraticFromString(string equation)
        {
            try
            {
                var (a, b, c) = ParseEquationString(equation);
                return SolveQuadratic(a, b, c);
            }
            catch (Exception ex)
            {
                return $"Error parsing equation: {ex.Message}";
            }
        }

        /// <summary>
        /// Solves quadratic equation with step-by-step solution
        /// </summary>
        /// <param name="a">Coefficient of x²</param>
        /// <param name="b">Coefficient of x</param>
        /// <param name="c">Constant term</param>
        /// <returns>String with step-by-step solution</returns>
        public static string SolveQuadraticWithSteps(double a, double b, double c)
        {
            var solution = new StringBuilder();
            
            try
            {
                solution.AppendLine($"Solving: {FormatEquation(a, b, c)} = 0");
                solution.AppendLine();

                // Handle special cases
                if (Math.Abs(a) < 1e-10)
                {
                    solution.AppendLine("This is not a quadratic equation (a = 0)");
                    solution.AppendLine(SolveLinear(b, c));
                    return solution.ToString();
                }

                // Calculate discriminant
                double discriminant = b * b - 4 * a * c;
                solution.AppendLine($"Using the quadratic formula: x = (-b ± √(b² - 4ac)) / (2a)");
                solution.AppendLine($"Where a = {a}, b = {b}, c = {c}");
                solution.AppendLine();
                solution.AppendLine($"Discriminant = b² - 4ac = ({b})² - 4({a})({c}) = {discriminant:F6}");
                solution.AppendLine();

                if (discriminant > 0)
                {
                    solution.AppendLine("Since discriminant > 0, there are two distinct real roots:");
                    double sqrtDiscriminant = Math.Sqrt(discriminant);
                    solution.AppendLine($"√discriminant = √{discriminant:F6} = {sqrtDiscriminant:F6}");
                    
                    double root1 = (-b + sqrtDiscriminant) / (2 * a);
                    double root2 = (-b - sqrtDiscriminant) / (2 * a);
                    
                    solution.AppendLine($"x₁ = (-{b} + {sqrtDiscriminant:F6}) / (2 × {a}) = {root1:F6}");
                    solution.AppendLine($"x₂ = (-{b} - {sqrtDiscriminant:F6}) / (2 × {a}) = {root2:F6}");
                    solution.AppendLine();
                    solution.AppendLine($"Roots: {FormatTwoRoots(Math.Min(root1, root2), Math.Max(root1, root2))}");
                }
                else if (Math.Abs(discriminant) < 1e-10)
                {
                    solution.AppendLine("Since discriminant = 0, there is one repeated real root:");
                    double root = -b / (2 * a);
                    solution.AppendLine($"x = -b / (2a) = -({b}) / (2 × {a}) = {root:F6}");
                    solution.AppendLine();
                    solution.AppendLine($"Root: {FormatOneRoot(root)}");
                }
                else
                {
                    solution.AppendLine("Since discriminant < 0, there are two complex conjugate roots:");
                    double realPart = -b / (2 * a);
                    double imaginaryPart = Math.Sqrt(-discriminant) / (2 * a);
                    solution.AppendLine($"Real part = -b / (2a) = -({b}) / (2 × {a}) = {realPart:F6}");
                    solution.AppendLine($"Imaginary part = √(-discriminant) / (2a) = √{-discriminant:F6} / (2 × {a}) = {imaginaryPart:F6}");
                    solution.AppendLine();
                    solution.AppendLine($"Roots: {FormatComplexRoots(realPart, imaginaryPart)}");
                }

                return solution.ToString();
            }
            catch (Exception ex)
            {
                return $"Error generating step-by-step solution: {ex.Message}";
            }
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Solves linear equation bx + c = 0
        /// </summary>
        /// <param name="b">Coefficient of x</param>
        /// <param name="c">Constant term</param>
        /// <returns>String representation of the solution</returns>
        private static string SolveLinear(double b, double c)
        {
            if (Math.Abs(b) < 1e-10) // b ≈ 0
            {
                if (Math.Abs(c) < 1e-10) // c ≈ 0
                {
                    return "Infinite solutions (0 = 0)";
                }
                else
                {
                    return "No solution (contradiction)";
                }
            }
            else
            {
                double root = -c / b;
                return $"Linear equation: x = {FormatNumber(root)}";
            }
        }

        /// <summary>
        /// Formats two real roots
        /// </summary>
        /// <param name="root1">First root</param>
        /// <param name="root2">Second root</param>
        /// <returns>Formatted string</returns>
        private static string FormatTwoRoots(double root1, double root2)
        {
            return $"x₁ = {FormatNumber(root1)}, x₂ = {FormatNumber(root2)}";
        }

        /// <summary>
        /// Formats one real root (repeated)
        /// </summary>
        /// <param name="root">The root value</param>
        /// <returns>Formatted string</returns>
        private static string FormatOneRoot(double root)
        {
            return $"x = {FormatNumber(root)} (repeated root)";
        }

        /// <summary>
        /// Formats complex conjugate roots
        /// </summary>
        /// <param name="realPart">Real part</param>
        /// <param name="imaginaryPart">Imaginary part</param>
        /// <returns>Formatted string</returns>
        private static string FormatComplexRoots(double realPart, double imaginaryPart)
        {
            string real = FormatNumber(realPart);
            string imag = FormatNumber(Math.Abs(imaginaryPart));
            
            if (Math.Abs(realPart) < 1e-10) // Real part ≈ 0
            {
                return $"x₁ = {imag}i, x₂ = -{imag}i";
            }
            else
            {
                return $"x₁ = {real} + {imag}i, x₂ = {real} - {imag}i";
            }
        }

        /// <summary>
        /// Formats a number for display
        /// </summary>
        /// <param name="number">Number to format</param>
        /// <returns>Formatted string</returns>
        private static string FormatNumber(double number)
        {
            // Handle special cases
            if (double.IsNaN(number))
                return "NaN";
            if (double.IsPositiveInfinity(number))
                return "+∞";
            if (double.IsNegativeInfinity(number))
                return "-∞";

            // Check if it's effectively an integer
            if (Math.Abs(number - Math.Round(number)) < 1e-10)
            {
                return Math.Round(number).ToString();
            }

            // Check for common fractions
            string fraction = TryFormatAsFraction(number);
            if (!string.IsNullOrEmpty(fraction))
            {
                return fraction;
            }

            // Format as decimal with appropriate precision
            if (Math.Abs(number) >= 1000 || Math.Abs(number) < 0.001)
            {
                return number.ToString("E6", CultureInfo.InvariantCulture);
            }
            else
            {
                return number.ToString("F6", CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');
            }
        }

        /// <summary>
        /// Attempts to format a decimal as a simple fraction
        /// </summary>
        /// <param name="number">Number to format</param>
        /// <returns>Fraction string or empty if not a simple fraction</returns>
        private static string TryFormatAsFraction(double number)
        {
            // Check for common fractions with denominators up to 12
            for (int denominator = 2; denominator <= 12; denominator++)
            {
                double numerator = number * denominator;
                if (Math.Abs(numerator - Math.Round(numerator)) < 1e-10)
                {
                    int num = (int)Math.Round(numerator);
                    if (num == 0) return "0";
                    if (num == denominator) return "1";
                    if (num == -denominator) return "-1";
                    
                    // Simplify fraction
                    int gcd = GreatestCommonDivisor(Math.Abs(num), denominator);
                    num /= gcd;
                    int den = denominator / gcd;
                    
                    if (den == 1) return num.ToString();
                    return $"{num}/{den}";
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Calculates the greatest common divisor
        /// </summary>
        /// <param name="a">First number</param>
        /// <param name="b">Second number</param>
        /// <returns>GCD</returns>
        private static int GreatestCommonDivisor(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        /// <summary>
        /// Formats the quadratic equation
        /// </summary>
        /// <param name="a">Coefficient of x²</param>
        /// <param name="b">Coefficient of x</param>
        /// <param name="c">Constant term</param>
        /// <returns>Formatted equation string</returns>
        private static string FormatEquation(double a, double b, double c)
        {
            var equation = new StringBuilder();

            // x² term
            if (Math.Abs(a - 1) < 1e-10)
                equation.Append("x²");
            else if (Math.Abs(a + 1) < 1e-10)
                equation.Append("-x²");
            else
                equation.Append($"{FormatNumber(a)}x²");

            // x term
            if (Math.Abs(b) > 1e-10)
            {
                if (b > 0)
                    equation.Append(" + ");
                else
                    equation.Append(" - ");

                double absB = Math.Abs(b);
                if (Math.Abs(absB - 1) < 1e-10)
                    equation.Append("x");
                else
                    equation.Append($"{FormatNumber(absB)}x");
            }

            // Constant term
            if (Math.Abs(c) > 1e-10)
            {
                if (c > 0)
                    equation.Append(" + ");
                else
                    equation.Append(" - ");

                equation.Append(FormatNumber(Math.Abs(c)));
            }

            return equation.ToString();
        }

        /// <summary>
        /// Parses equation string to extract coefficients
        /// </summary>
        /// <param name="equation">Equation string</param>
        /// <returns>Tuple of coefficients (a, b, c)</returns>
        private static (double a, double b, double c) ParseEquationString(string equation)
        {
            // This is a simplified parser - in production, use a proper expression parser
            // For now, we'll handle basic formats like "x^2 + 2x + 1 = 0"
            
            if (string.IsNullOrWhiteSpace(equation))
                throw new ArgumentException("Equation cannot be empty");

            // Remove spaces and convert to lowercase
            equation = equation.Replace(" ", "").ToLower();

            // Remove "= 0" if present
            equation = equation.Replace("=0", "");

            // This is a basic implementation - extend as needed
            // For demonstration, we'll parse simple cases
            double a = 0, b = 0, c = 0;

            // Look for x^2 or x² terms
            if (equation.Contains("x^2") || equation.Contains("x²"))
            {
                a = 1; // Default coefficient
                // Extract coefficient if present
                // This is simplified - implement full parsing as needed
            }

            return (a, b, c);
        }

        #endregion

        #region Validation and Analysis

        /// <summary>
        /// Validates the roots by substituting back into the original equation
        /// </summary>
        /// <param name="a">Coefficient of x²</param>
        /// <param name="b">Coefficient of x</param>
        /// <param name="c">Constant term</param>
        /// <param name="root">Root to validate</param>
        /// <returns>True if root is valid</returns>
        public static bool ValidateRoot(double a, double b, double c, double root)
        {
            double result = a * root * root + b * root + c;
            return Math.Abs(result) < 1e-10;
        }

        /// <summary>
        /// Gets the type of solution for the quadratic equation
        /// </summary>
        /// <param name="a">Coefficient of x²</param>
        /// <param name="b">Coefficient of x</param>
        /// <param name="c">Constant term</param>
        /// <returns>Solution type</returns>
        public static QuadraticSolutionType GetSolutionType(double a, double b, double c)
        {
            if (Math.Abs(a) < 1e-10)
            {
                return QuadraticSolutionType.NotQuadratic;
            }

            double discriminant = b * b - 4 * a * c;

            if (discriminant > 1e-10)
                return QuadraticSolutionType.TwoRealRoots;
            else if (Math.Abs(discriminant) <= 1e-10)
                return QuadraticSolutionType.OneRealRoot;
            else
                return QuadraticSolutionType.NoRealRoots;
        }

        /// <summary>
        /// Gets additional information about the quadratic equation
        /// </summary>
        /// <param name="a">Coefficient of x²</param>
        /// <param name="b">Coefficient of x</param>
        /// <param name="c">Constant term</param>
        /// <returns>Analysis string</returns>
        public static string AnalyzeQuadratic(double a, double b, double c)
        {
            var analysis = new StringBuilder();
            
            analysis.AppendLine($"Equation: {FormatEquation(a, b, c)} = 0");
            analysis.AppendLine($"Coefficients: a = {a}, b = {b}, c = {c}");
            
            if (Math.Abs(a) < 1e-10)
            {
                analysis.AppendLine("Type: Linear equation (not quadratic)");
                return analysis.ToString();
            }

            analysis.AppendLine("Type: Quadratic equation");
            
            double discriminant = b * b - 4 * a * c;
            analysis.AppendLine($"Discriminant: {discriminant:F6}");
            
            var solutionType = GetSolutionType(a, b, c);
            analysis.AppendLine($"Solution type: {solutionType}");
            
            // Vertex information
            double vertexX = -b / (2 * a);
            double vertexY = a * vertexX * vertexX + b * vertexX + c;
            analysis.AppendLine($"Vertex: ({FormatNumber(vertexX)}, {FormatNumber(vertexY)})");
            
            // Parabola direction
            analysis.AppendLine($"Opens: {(a > 0 ? "Upward" : "Downward")}");
            
            return analysis.ToString();
        }

        #endregion

        #region Example Usage Functions

        /// <summary>
        /// Demonstrates various ways to use the quadratic solver
        /// </summary>
        public static void ExampleUsage()
        {
            Console.WriteLine("=== Quadratic Equation Solver Examples ===\n");

            // Example 1: Standard quadratic with two real roots
            Console.WriteLine("Example 1: x² - 5x + 6 = 0");
            Console.WriteLine($"Solution: {SolveQuadratic(1, -5, 6)}");
            Console.WriteLine();

            // Example 2: Perfect square (one repeated root)
            Console.WriteLine("Example 2: x² - 4x + 4 = 0");
            Console.WriteLine($"Solution: {SolveQuadratic(1, -4, 4)}");
            Console.WriteLine();

            // Example 3: No real roots
            Console.WriteLine("Example 3: x² + x + 1 = 0");
            Console.WriteLine($"Solution: {SolveQuadratic(1, 1, 1)}");
            Console.WriteLine();

            // Example 4: Step-by-step solution
            Console.WriteLine("Example 4: Step-by-step solution for 2x² - 4x + 2 = 0");
            Console.WriteLine(SolveQuadraticWithSteps(2, -4, 2));

            // Example 5: Analysis
            Console.WriteLine("Example 5: Analysis of x² - 3x + 2 = 0");
            Console.WriteLine(AnalyzeQuadratic(1, -3, 2));
        }

        #endregion
    }
}