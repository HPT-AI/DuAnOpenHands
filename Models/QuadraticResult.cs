using System;

namespace UserCRUD.Models
{
    public class QuadraticResult
    {
        public int Id { get; set; }
        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }
        public double? Root1 { get; set; }
        public double? Root2 { get; set; }
        public double Discriminant { get; set; }
        public bool HasRealRoots { get; set; }
        public DateTime CalculatedDate { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; } // For display purposes
        public string? Notes { get; set; }

        public QuadraticResult()
        {
            CalculatedDate = DateTime.Now;
        }

        public QuadraticResult(double a, double b, double c, int? userId = null, string? notes = null)
        {
            A = a;
            B = b;
            C = c;
            UserId = userId;
            Notes = notes;
            CalculatedDate = DateTime.Now;
            CalculateRoots();
        }

        public void CalculateRoots()
        {
            // Calculate discriminant: b² - 4ac
            Discriminant = (B * B) - (4 * A * C);

            if (A == 0)
            {
                // Not a quadratic equation, it's linear: bx + c = 0
                HasRealRoots = B != 0;
                if (HasRealRoots)
                {
                    Root1 = -C / B;
                    Root2 = null;
                }
                else
                {
                    Root1 = null;
                    Root2 = null;
                }
            }
            else if (Discriminant >= 0)
            {
                // Real roots exist
                HasRealRoots = true;
                double sqrtDiscriminant = Math.Sqrt(Discriminant);
                Root1 = (-B + sqrtDiscriminant) / (2 * A);
                Root2 = (-B - sqrtDiscriminant) / (2 * A);

                // If discriminant is 0, both roots are the same
                if (Discriminant == 0)
                {
                    Root2 = Root1;
                }
            }
            else
            {
                // Complex roots (no real solutions)
                HasRealRoots = false;
                Root1 = null;
                Root2 = null;
            }
        }

        public string GetEquationString()
        {
            var equation = "";
            
            // Handle coefficient A
            if (A == 1)
                equation += "x²";
            else if (A == -1)
                equation += "-x²";
            else if (A != 0)
                equation += $"{A}x²";

            // Handle coefficient B
            if (B > 0 && !string.IsNullOrEmpty(equation))
                equation += " + ";
            else if (B < 0)
                equation += " - ";

            if (Math.Abs(B) == 1 && B != 0)
                equation += "x";
            else if (B != 0)
                equation += $"{Math.Abs(B)}x";

            // Handle coefficient C
            if (C > 0 && !string.IsNullOrEmpty(equation))
                equation += " + ";
            else if (C < 0)
                equation += " - ";

            if (C != 0)
                equation += Math.Abs(C).ToString();

            // If equation is empty, it means all coefficients are 0
            if (string.IsNullOrEmpty(equation))
                equation = "0";

            return equation + " = 0";
        }

        public string GetRootsString()
        {
            if (!HasRealRoots)
                return "No real roots (complex solutions)";

            if (A == 0)
            {
                if (B == 0)
                    return C == 0 ? "Infinite solutions" : "No solution";
                return $"x = {Root1:F4}";
            }

            if (Discriminant == 0)
                return $"x = {Root1:F4} (double root)";

            return $"x₁ = {Root1:F4}, x₂ = {Root2:F4}";
        }

        public override string ToString()
        {
            return $"Id: {Id}, Equation: {GetEquationString()}, Roots: {GetRootsString()}, Date: {CalculatedDate:yyyy-MM-dd HH:mm}";
        }
    }
}