using System;
using System.Globalization;

namespace UserCRUD
{
    /// <summary>
    /// Simple quadratic equation solver function for Task 3.4
    /// Solves equations of the form ax² + bx + c = 0 and returns roots as string
    /// </summary>
    public static class QuadraticFunction
    {
        /// <summary>
        /// Main function for Task 3.4: Solve quadratic equation and return roots as string
        /// Solves ax² + bx + c = 0
        /// </summary>
        /// <param name="a">Coefficient of x² (cannot be zero for quadratic)</param>
        /// <param name="b">Coefficient of x</param>
        /// <param name="c">Constant term</param>
        /// <returns>String representation of the roots</returns>
        public static string SolveQuadraticEquation(double a, double b, double c)
        {
            // Validate input
            if (a == 0)
            {
                return "Error: Not a quadratic equation (a cannot be zero)";
            }

            if (double.IsNaN(a) || double.IsNaN(b) || double.IsNaN(c))
            {
                return "Error: Invalid coefficients (NaN values not allowed)";
            }

            if (double.IsInfinity(a) || double.IsInfinity(b) || double.IsInfinity(c))
            {
                return "Error: Invalid coefficients (infinite values not allowed)";
            }

            try
            {
                // Calculate discriminant: b² - 4ac
                double discriminant = b * b - 4 * a * c;

                if (discriminant > 0)
                {
                    // Two distinct real roots
                    double sqrtDiscriminant = Math.Sqrt(discriminant);
                    double root1 = (-b + sqrtDiscriminant) / (2 * a);
                    double root2 = (-b - sqrtDiscriminant) / (2 * a);

                    // Format and return roots (order: smaller root first)
                    if (root1 > root2)
                    {
                        (root1, root2) = (root2, root1);
                    }

                    return $"Two real roots: x₁ = {FormatRoot(root1)}, x₂ = {FormatRoot(root2)}";
                }
                else if (discriminant == 0)
                {
                    // One repeated real root
                    double root = -b / (2 * a);
                    return $"One repeated root: x = {FormatRoot(root)}";
                }
                else
                {
                    // Complex conjugate roots
                    double realPart = -b / (2 * a);
                    double imaginaryPart = Math.Sqrt(-discriminant) / (2 * a);
                    
                    return $"Complex roots: x₁ = {FormatRoot(realPart)} + {FormatRoot(imaginaryPart)}i, " +
                           $"x₂ = {FormatRoot(realPart)} - {FormatRoot(imaginaryPart)}i";
                }
            }
            catch (Exception ex)
            {
                return $"Error solving equation: {ex.Message}";
            }
        }

        /// <summary>
        /// Alternative function signature with integer coefficients
        /// </summary>
        /// <param name="a">Coefficient of x²</param>
        /// <param name="b">Coefficient of x</param>
        /// <param name="c">Constant term</param>
        /// <returns>String representation of the roots</returns>
        public static string SolveQuadraticEquation(int a, int b, int c)
        {
            return SolveQuadraticEquation((double)a, (double)b, (double)c);
        }

        /// <summary>
        /// Simplified function that returns just the roots without labels
        /// </summary>
        /// <param name="a">Coefficient of x²</param>
        /// <param name="b">Coefficient of x</param>
        /// <param name="c">Constant term</param>
        /// <returns>Simple string representation of roots</returns>
        public static string GetRoots(double a, double b, double c)
        {
            if (a == 0)
            {
                return "Not quadratic";
            }

            double discriminant = b * b - 4 * a * c;

            if (discriminant > 0)
            {
                double sqrtDiscriminant = Math.Sqrt(discriminant);
                double root1 = (-b + sqrtDiscriminant) / (2 * a);
                double root2 = (-b - sqrtDiscriminant) / (2 * a);

                if (root1 > root2)
                {
                    (root1, root2) = (root2, root1);
                }

                return $"{FormatRoot(root1)}, {FormatRoot(root2)}";
            }
            else if (discriminant == 0)
            {
                double root = -b / (2 * a);
                return FormatRoot(root);
            }
            else
            {
                double realPart = -b / (2 * a);
                double imaginaryPart = Math.Sqrt(-discriminant) / (2 * a);
                return $"{FormatRoot(realPart)} ± {FormatRoot(imaginaryPart)}i";
            }
        }

        /// <summary>
        /// Function that returns roots with equation information
        /// </summary>
        /// <param name="a">Coefficient of x²</param>
        /// <param name="b">Coefficient of x</param>
        /// <param name="c">Constant term</param>
        /// <returns>String with equation and roots</returns>
        public static string SolveWithEquation(double a, double b, double c)
        {
            string equation = FormatEquation(a, b, c);
            string roots = SolveQuadraticEquation(a, b, c);
            return $"Equation: {equation} = 0\nSolution: {roots}";
        }

        /// <summary>
        /// Formats a root value for display
        /// </summary>
        /// <param name="value">Root value</param>
        /// <returns>Formatted string</returns>
        private static string FormatRoot(double value)
        {
            // Handle special cases
            if (double.IsNaN(value))
                return "NaN";
            if (double.IsPositiveInfinity(value))
                return "∞";
            if (double.IsNegativeInfinity(value))
                return "-∞";

            // Check if it's effectively zero
            if (Math.Abs(value) < 1e-10)
                return "0";

            // Check if it's effectively an integer
            if (Math.Abs(value - Math.Round(value)) < 1e-10)
            {
                return Math.Round(value).ToString();
            }

            // Check for simple fractions
            for (int denominator = 2; denominator <= 10; denominator++)
            {
                double numerator = value * denominator;
                if (Math.Abs(numerator - Math.Round(numerator)) < 1e-10)
                {
                    int num = (int)Math.Round(numerator);
                    if (num != 0)
                    {
                        // Simplify fraction
                        int gcd = GreatestCommonDivisor(Math.Abs(num), denominator);
                        num /= gcd;
                        int den = denominator / gcd;
                        
                        if (den == 1)
                            return num.ToString();
                        else
                            return $"{num}/{den}";
                    }
                }
            }

            // Format as decimal
            return value.ToString("F6", CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');
        }

        /// <summary>
        /// Formats the quadratic equation
        /// </summary>
        /// <param name="a">Coefficient of x²</param>
        /// <param name="b">Coefficient of x</param>
        /// <param name="c">Constant term</param>
        /// <returns>Formatted equation</returns>
        private static string FormatEquation(double a, double b, double c)
        {
            string result = "";

            // x² term
            if (a == 1)
                result = "x²";
            else if (a == -1)
                result = "-x²";
            else
                result = $"{FormatRoot(a)}x²";

            // x term
            if (b != 0)
            {
                if (b > 0 && result != "")
                    result += " + ";
                else if (b < 0)
                    result += " - ";

                double absB = Math.Abs(b);
                if (absB == 1)
                    result += "x";
                else
                    result += $"{FormatRoot(absB)}x";
            }

            // Constant term
            if (c != 0)
            {
                if (c > 0 && result != "")
                    result += " + ";
                else if (c < 0)
                    result += " - ";

                result += FormatRoot(Math.Abs(c));
            }

            return result == "" ? "0" : result;
        }

        /// <summary>
        /// Calculates greatest common divisor
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
        /// Example usage of the quadratic solver function
        /// </summary>
        public static void ExampleUsage()
        {
            Console.WriteLine("=== Quadratic Function Examples (Task 3.4) ===\n");

            // Example 1: Two real roots
            Console.WriteLine("Example 1: x² - 5x + 6 = 0");
            Console.WriteLine(SolveQuadraticEquation(1, -5, 6));
            Console.WriteLine();

            // Example 2: One repeated root
            Console.WriteLine("Example 2: x² - 4x + 4 = 0");
            Console.WriteLine(SolveQuadraticEquation(1, -4, 4));
            Console.WriteLine();

            // Example 3: Complex roots
            Console.WriteLine("Example 3: x² + x + 1 = 0");
            Console.WriteLine(SolveQuadraticEquation(1, 1, 1));
            Console.WriteLine();

            // Example 4: Simple roots format
            Console.WriteLine("Example 4: Simple roots for 2x² - 8x + 6 = 0");
            Console.WriteLine($"Roots: {GetRoots(2, -8, 6)}");
            Console.WriteLine();

            // Example 5: With equation display
            Console.WriteLine("Example 5: Complete solution");
            Console.WriteLine(SolveWithEquation(1, -3, 2));
        }
    }
}