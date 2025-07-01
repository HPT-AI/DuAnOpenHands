using System;
using System.Collections.Generic;

namespace UserCRUD
{
    /// <summary>
    /// Test cases for the quadratic equation solver function (Task 3.4)
    /// </summary>
    public static class QuadraticFunctionTests
    {
        /// <summary>
        /// Test case structure
        /// </summary>
        public class TestCase
        {
            public double A { get; set; }
            public double B { get; set; }
            public double C { get; set; }
            public string Description { get; set; } = string.Empty;
            public string ExpectedPattern { get; set; } = string.Empty;
            public bool ShouldContain { get; set; } = true;
        }

        /// <summary>
        /// Runs all test cases for the quadratic function
        /// </summary>
        /// <returns>Test results summary</returns>
        public static string RunAllTests()
        {
            var testCases = GetTestCases();
            int passed = 0;
            int total = testCases.Count;
            var results = new List<string>();

            results.Add("=== Quadratic Function Test Results (Task 3.4) ===\n");

            foreach (var testCase in testCases)
            {
                try
                {
                    string result = QuadraticFunction.SolveQuadraticEquation(testCase.A, testCase.B, testCase.C);
                    bool testPassed = testCase.ShouldContain ? 
                        result.Contains(testCase.ExpectedPattern) : 
                        !result.Contains(testCase.ExpectedPattern);

                    if (testPassed)
                    {
                        passed++;
                        results.Add($"✅ PASS: {testCase.Description}");
                        results.Add($"   Input: a={testCase.A}, b={testCase.B}, c={testCase.C}");
                        results.Add($"   Result: {result}");
                    }
                    else
                    {
                        results.Add($"❌ FAIL: {testCase.Description}");
                        results.Add($"   Input: a={testCase.A}, b={testCase.B}, c={testCase.C}");
                        results.Add($"   Expected pattern: {testCase.ExpectedPattern}");
                        results.Add($"   Actual result: {result}");
                    }
                    results.Add("");
                }
                catch (Exception ex)
                {
                    results.Add($"❌ ERROR: {testCase.Description}");
                    results.Add($"   Exception: {ex.Message}");
                    results.Add("");
                }
            }

            results.Add($"=== Test Summary ===");
            results.Add($"Passed: {passed}/{total} ({(double)passed / total * 100:F1}%)");
            
            if (passed == total)
            {
                results.Add("🎉 All tests passed!");
            }
            else
            {
                results.Add($"⚠️  {total - passed} test(s) failed.");
            }

            return string.Join("\n", results);
        }

        /// <summary>
        /// Gets all test cases for the quadratic function
        /// </summary>
        /// <returns>List of test cases</returns>
        private static List<TestCase> GetTestCases()
        {
            return new List<TestCase>
            {
                // Two real roots test cases
                new TestCase
                {
                    A = 1, B = -5, C = 6,
                    Description = "Standard quadratic with two real roots (x² - 5x + 6 = 0)",
                    ExpectedPattern = "Two real roots"
                },
                new TestCase
                {
                    A = 1, B = -3, C = 2,
                    Description = "Simple quadratic (x² - 3x + 2 = 0)",
                    ExpectedPattern = "Two real roots"
                },
                new TestCase
                {
                    A = 2, B = -4, C = 2,
                    Description = "Quadratic with coefficient 2 (2x² - 4x + 2 = 0)",
                    ExpectedPattern = "One repeated root"
                },

                // One repeated root test cases
                new TestCase
                {
                    A = 1, B = -4, C = 4,
                    Description = "Perfect square (x² - 4x + 4 = 0)",
                    ExpectedPattern = "One repeated root"
                },
                new TestCase
                {
                    A = 1, B = -2, C = 1,
                    Description = "Perfect square (x² - 2x + 1 = 0)",
                    ExpectedPattern = "One repeated root"
                },

                // Complex roots test cases
                new TestCase
                {
                    A = 1, B = 1, C = 1,
                    Description = "Complex roots (x² + x + 1 = 0)",
                    ExpectedPattern = "Complex roots"
                },
                new TestCase
                {
                    A = 1, B = 0, C = 1,
                    Description = "Pure imaginary roots (x² + 1 = 0)",
                    ExpectedPattern = "Complex roots"
                },
                new TestCase
                {
                    A = 1, B = 2, C = 5,
                    Description = "Complex roots with real part (x² + 2x + 5 = 0)",
                    ExpectedPattern = "Complex roots"
                },

                // Edge cases
                new TestCase
                {
                    A = 0, B = 1, C = 1,
                    Description = "Not quadratic (a = 0)",
                    ExpectedPattern = "Error: Not a quadratic equation"
                },
                new TestCase
                {
                    A = 1, B = 0, C = 0,
                    Description = "Simple case (x² = 0)",
                    ExpectedPattern = "One repeated root"
                },
                new TestCase
                {
                    A = 1, B = 0, C = -4,
                    Description = "Difference of squares (x² - 4 = 0)",
                    ExpectedPattern = "Two real roots"
                },

                // Negative coefficient cases
                new TestCase
                {
                    A = -1, B = 5, C = -6,
                    Description = "Negative leading coefficient (-x² + 5x - 6 = 0)",
                    ExpectedPattern = "Two real roots"
                },
                new TestCase
                {
                    A = 1, B = -1, C = -6,
                    Description = "Negative constant (x² - x - 6 = 0)",
                    ExpectedPattern = "Two real roots"
                },

                // Fractional results
                new TestCase
                {
                    A = 2, B = -3, C = 1,
                    Description = "Fractional roots (2x² - 3x + 1 = 0)",
                    ExpectedPattern = "Two real roots"
                },
                new TestCase
                {
                    A = 3, B = 2, C = -1,
                    Description = "Mixed coefficients (3x² + 2x - 1 = 0)",
                    ExpectedPattern = "Two real roots"
                },

                // Large numbers
                new TestCase
                {
                    A = 100, B = -200, C = 100,
                    Description = "Large coefficients (100x² - 200x + 100 = 0)",
                    ExpectedPattern = "One repeated root"
                },

                // Small numbers
                new TestCase
                {
                    A = 0.1, B = -0.3, C = 0.2,
                    Description = "Decimal coefficients (0.1x² - 0.3x + 0.2 = 0)",
                    ExpectedPattern = "Two real roots"
                },

                // Invalid input cases
                new TestCase
                {
                    A = double.NaN, B = 1, C = 1,
                    Description = "NaN coefficient",
                    ExpectedPattern = "Error: Invalid coefficients"
                },
                new TestCase
                {
                    A = double.PositiveInfinity, B = 1, C = 1,
                    Description = "Infinite coefficient",
                    ExpectedPattern = "Error: Invalid coefficients"
                }
            };
        }

        /// <summary>
        /// Runs specific test scenarios and displays results
        /// </summary>
        public static void RunSpecificTests()
        {
            Console.WriteLine("=== Specific Quadratic Function Tests ===\n");

            // Test 1: Classic examples
            Console.WriteLine("Test 1: Classic quadratic equations");
            TestEquation(1, -5, 6, "x² - 5x + 6 = 0 (factors: (x-2)(x-3))");
            TestEquation(1, -7, 12, "x² - 7x + 12 = 0 (factors: (x-3)(x-4))");
            TestEquation(1, 1, -6, "x² + x - 6 = 0 (factors: (x+3)(x-2))");
            Console.WriteLine();

            // Test 2: Perfect squares
            Console.WriteLine("Test 2: Perfect square trinomials");
            TestEquation(1, -4, 4, "(x - 2)² = 0");
            TestEquation(1, 6, 9, "(x + 3)² = 0");
            TestEquation(4, -4, 1, "(2x - 1)² = 0");
            Console.WriteLine();

            // Test 3: No real solutions
            Console.WriteLine("Test 3: Complex solutions");
            TestEquation(1, 1, 1, "x² + x + 1 = 0");
            TestEquation(1, 0, 1, "x² + 1 = 0");
            TestEquation(2, 1, 1, "2x² + x + 1 = 0");
            Console.WriteLine();

            // Test 4: Special cases
            Console.WriteLine("Test 4: Special cases");
            TestEquation(1, 0, 0, "x² = 0");
            TestEquation(1, 0, -4, "x² - 4 = 0");
            TestEquation(0, 2, 4, "2x + 4 = 0 (not quadratic)");
            Console.WriteLine();

            // Test 5: Validation with known results
            Console.WriteLine("Test 5: Validation tests");
            ValidateRoots(1, -5, 6, new double[] { 2, 3 });
            ValidateRoots(1, -4, 4, new double[] { 2 });
            ValidateRoots(1, 0, -4, new double[] { -2, 2 });
        }

        /// <summary>
        /// Tests a specific equation and displays the result
        /// </summary>
        /// <param name="a">Coefficient of x²</param>
        /// <param name="b">Coefficient of x</param>
        /// <param name="c">Constant term</param>
        /// <param name="description">Description of the equation</param>
        private static void TestEquation(double a, double b, double c, string description)
        {
            string result = QuadraticFunction.SolveQuadraticEquation(a, b, c);
            Console.WriteLine($"  {description}");
            Console.WriteLine($"  Result: {result}");
        }

        /// <summary>
        /// Validates that the function produces expected roots
        /// </summary>
        /// <param name="a">Coefficient of x²</param>
        /// <param name="b">Coefficient of x</param>
        /// <param name="c">Constant term</param>
        /// <param name="expectedRoots">Expected root values</param>
        private static void ValidateRoots(double a, double b, double c, double[] expectedRoots)
        {
            string result = QuadraticFunction.SolveQuadraticEquation(a, b, c);
            Console.WriteLine($"  Equation: {a}x² + {b}x + {c} = 0");
            Console.WriteLine($"  Expected roots: {string.Join(", ", expectedRoots)}");
            Console.WriteLine($"  Function result: {result}");
            
            // Verify by substitution
            bool allValid = true;
            foreach (double root in expectedRoots)
            {
                double substitution = a * root * root + b * root + c;
                bool isValid = Math.Abs(substitution) < 1e-10;
                Console.WriteLine($"  Verification for x = {root}: {a}({root})² + {b}({root}) + {c} = {substitution:F10} {(isValid ? "✓" : "✗")}");
                allValid &= isValid;
            }
            
            Console.WriteLine($"  Overall validation: {(allValid ? "✅ PASS" : "❌ FAIL")}");
        }

        /// <summary>
        /// Performance test for the quadratic function
        /// </summary>
        /// <param name="iterations">Number of iterations to run</param>
        /// <returns>Performance results</returns>
        public static string PerformanceTest(int iterations = 10000)
        {
            var random = new Random(42); // Fixed seed for reproducible results
            var startTime = DateTime.Now;

            for (int i = 0; i < iterations; i++)
            {
                double a = random.NextDouble() * 10 + 0.1; // Avoid zero
                double b = random.NextDouble() * 20 - 10;  // -10 to 10
                double c = random.NextDouble() * 20 - 10;  // -10 to 10

                QuadraticFunction.SolveQuadraticEquation(a, b, c);
            }

            var endTime = DateTime.Now;
            var duration = endTime - startTime;

            return $"Performance Test Results:\n" +
                   $"Iterations: {iterations:N0}\n" +
                   $"Total time: {duration.TotalMilliseconds:F2} ms\n" +
                   $"Average time per call: {duration.TotalMilliseconds / iterations:F6} ms\n" +
                   $"Calls per second: {iterations / duration.TotalSeconds:F0}";
        }
    }
}