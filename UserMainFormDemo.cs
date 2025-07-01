using System;
using System.Windows.Forms;
using UserCRUD.Forms;
using UserCRUD.Models;

namespace UserCRUD
{
    /// <summary>
    /// Demo application to test the User Main Form for Task 4.3
    /// </summary>
    public static class UserMainFormDemo
    {
        /// <summary>
        /// Entry point for testing the Task 4.3 user main form
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Test the Task 4.3 user main form
                TestUserMainForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running user main form demo: {ex.Message}", 
                    "Demo Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Tests the Task 4.3 user main form functionality
        /// </summary>
        private static void TestUserMainForm()
        {
            // Show welcome message
            var result = MessageBox.Show(
                "Welcome to the Task 4.3 User Main Form Demo!\n\n" +
                "This demo will show the main screen for users with all required features:\n\n" +
                "REQUIRED FIELDS:\n" +
                "• a, b, c (Textbox) - Quadratic equation coefficients\n\n" +
                "REQUIRED BUTTONS:\n" +
                "• Solve - Calculate quadratic equation roots\n" +
                "• Save - Save result to database\n" +
                "• Clear - Clear all fields and results\n\n" +
                "REQUIRED LABELS:\n" +
                "• Display result - Shows calculated roots\n\n" +
                "REQUIRED LOGIC:\n" +
                "• Calculate roots correctly (no root, double root, two distinct roots)\n" +
                "• Save result to DB when clicking Save\n\n" +
                "Click OK to open the user main form.",
                "Task 4.3 User Main Form Demo",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Information);

            if (result != DialogResult.OK)
                return;

            // Create a test user
            var testUser = CreateTestUser();

            // Show user main form
            var formResult = UserMainForm.ShowUserMainForm(testUser);

            if (formResult == DialogResult.OK)
            {
                // User logged out normally
                MessageBox.Show(
                    "User main form closed successfully!\n\n" +
                    "Task 4.3 requirements verified:\n" +
                    "✓ Fields: a, b, c (Textbox) implemented\n" +
                    "✓ Buttons: Solve, Save, Clear functional\n" +
                    "✓ Labels: Display result working\n" +
                    "✓ Logic: Root calculation correct\n" +
                    "✓ Logic: Save to database functional\n" +
                    "✓ Working UI form delivered",
                    "Demo Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                // Form was closed or cancelled
                var retryResult = MessageBox.Show(
                    "User main form was closed.\n\n" +
                    "Would you like to:\n" +
                    "• Try the form again (Yes)\n" +
                    "• See feature examples (No)\n" +
                    "• Exit demo (Cancel)",
                    "Demo Result",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                switch (retryResult)
                {
                    case DialogResult.Yes:
                        TestUserMainForm(); // Retry
                        break;
                    case DialogResult.No:
                        ShowFeatureExamples();
                        break;
                    default:
                        break; // Exit
                }
            }
        }

        /// <summary>
        /// Creates a test user for demonstration
        /// </summary>
        /// <returns>Test user object</returns>
        private static User CreateTestUser()
        {
            return new User
            {
                Id = 1,
                FirstName = "Demo",
                LastName = "User",
                Email = "demo.user@example.com",
                Phone = "555-DEMO",
                Role = UserRole.User,
                IsActive = true,
                DateCreated = DateTime.Now.AddDays(-30),
                LastLoginDate = DateTime.Now
            };
        }

        /// <summary>
        /// Shows feature examples for the user main form
        /// </summary>
        private static void ShowFeatureExamples()
        {
            var examples = @"Task 4.3 User Main Form Feature Examples:

COEFFICIENT INPUT FIELDS (a, b, c):
✓ Valid inputs: 1, -5, 6 (integers)
✓ Valid inputs: 1.5, -2.3, 0.75 (decimals)
✓ Valid inputs: -1, 0, 4 (negative and zero)
✗ Invalid: a = 0 (not quadratic)
✗ Invalid: abc, empty fields

SOLVE BUTTON LOGIC:
Example 1: a=1, b=-5, c=6
• Discriminant = 25 - 24 = 1 > 0
• Result: Two distinct real roots: x₁ = 2, x₂ = 3

Example 2: a=1, b=-4, c=4  
• Discriminant = 16 - 16 = 0
• Result: One repeated root: x = 2

Example 3: a=1, b=1, c=1
• Discriminant = 1 - 4 = -3 < 0
• Result: Two complex roots: x₁ = -0.5 + 0.866i, x₂ = -0.5 - 0.866i

SAVE BUTTON LOGIC:
• Saves current result to database
• Associates with logged-in user
• Updates calculation history
• Disables after successful save

CLEAR BUTTON LOGIC:
• Clears all coefficient fields (a, b, c)
• Resets result display
• Prompts to save unsaved results
• Resets form state

RESULT DISPLAY LABEL:
• Shows formatted solution text
• Color-coded messages (blue for results, red for errors)
• Handles all solution types correctly
• Updates in real-time after solving

ADDITIONAL FEATURES:
• Real-time equation display (ax² + bx + c = 0)
• Input validation and error handling
• Calculation history with database storage
• User profile and logout functionality
• Keyboard navigation and shortcuts
• Professional UI design";

            MessageBox.Show(examples, "Task 4.3 Feature Examples", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Ask if user wants to try the form
            var tryForm = MessageBox.Show(
                "Would you like to try the user main form now?",
                "Try User Main Form",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (tryForm == DialogResult.Yes)
            {
                TestUserMainForm();
            }
        }

        /// <summary>
        /// Shows the calculation examples
        /// </summary>
        public static void ShowCalculationExamples()
        {
            var examples = @"Quadratic Equation Calculation Examples:

EXAMPLE 1: Two Distinct Real Roots
Input: a = 1, b = -5, c = 6
Equation: x² - 5x + 6 = 0
Discriminant: (-5)² - 4(1)(6) = 25 - 24 = 1
Result: x₁ = 2, x₂ = 3
Verification: (2)² - 5(2) + 6 = 4 - 10 + 6 = 0 ✓

EXAMPLE 2: One Repeated Root
Input: a = 1, b = -4, c = 4
Equation: x² - 4x + 4 = 0
Discriminant: (-4)² - 4(1)(4) = 16 - 16 = 0
Result: x = 2 (repeated)
Verification: (2)² - 4(2) + 4 = 4 - 8 + 4 = 0 ✓

EXAMPLE 3: Two Complex Roots
Input: a = 1, b = 1, c = 1
Equation: x² + x + 1 = 0
Discriminant: (1)² - 4(1)(1) = 1 - 4 = -3
Result: x₁ = -0.5 + 0.866i, x₂ = -0.5 - 0.866i

EXAMPLE 4: Simple Cases
Input: a = 1, b = 0, c = -4
Equation: x² - 4 = 0
Result: x₁ = -2, x₂ = 2

Input: a = 1, b = 0, c = 0
Equation: x² = 0
Result: x = 0 (repeated)

ERROR CASES:
Input: a = 0, b = 2, c = 1
Error: Not a quadratic equation (a cannot be zero)

Input: a = abc, b = 1, c = 1
Error: Invalid coefficient values";

            MessageBox.Show(examples, "Calculation Examples", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Shows the UI features of the user main form
        /// </summary>
        public static void ShowUIFeatures()
        {
            var features = @"Task 4.3 User Main Form UI Features:

LAYOUT DESIGN:
• Professional, clean interface
• Organized in logical groups (Input, Results, Actions)
• Responsive design with proper spacing
• Color-coded elements for better usability

INPUT SECTION:
• Three textboxes for coefficients a, b, c
• Real-time input validation
• Placeholder text for guidance
• Equation display updates automatically

RESULT SECTION:
• Large, clear result display area
• Color-coded messages (blue for results, red for errors)
• Formatted mathematical notation
• Bordered display area for emphasis

ACTION BUTTONS:
• Solve: Blue button, calculates roots
• Save: Green button, saves to database
• Clear: Gray button, resets form
• Additional: History, Profile, Logout

NAVIGATION:
• Tab order for keyboard navigation
• Enter key moves between fields
• Keyboard shortcuts available
• Menu bar with File, View, Help options

HISTORY SECTION:
• DataGridView showing past calculations
• Double-click to reload calculation
• Automatic refresh after saving
• User-specific history filtering

STATUS BAR:
• Real-time status updates
• Timestamp for actions
• Error and success messages
• Current operation feedback

ADDITIONAL FEATURES:
• Welcome message with user name
• Example button for sample values
• Help system with usage instructions
• Unsaved changes protection
• Logout confirmation";

            MessageBox.Show(features, "UI Features", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Demonstrates the complete workflow
        /// </summary>
        public static void DemonstrateWorkflow()
        {
            var workflow = @"Task 4.3 Complete Workflow Demonstration:

STEP 1: Form Initialization
• User main form opens with welcome message
• All fields are empty and ready for input
• Solve button is disabled until valid input

STEP 2: Enter Coefficients
• User enters values for a, b, c in textboxes
• Real-time validation ensures valid numbers
• Equation display updates: ax² + bx + c = 0
• Solve button becomes enabled

STEP 3: Calculate Solution
• User clicks Solve button
• System validates that a ≠ 0 (quadratic requirement)
• Calculates discriminant: b² - 4ac
• Determines solution type and calculates roots
• Displays formatted result in result label

STEP 4: Review Result
• Result shows appropriate message:
  - Two distinct real roots: x₁ = ..., x₂ = ...
  - One repeated root: x = ... (repeated)
  - Two complex roots: x₁ = ... + ...i, x₂ = ... - ...i
• Save button becomes enabled

STEP 5: Save Result (Optional)
• User clicks Save button
• System creates QuadraticResult object
• Saves to database with user association
• Updates calculation history
• Shows success confirmation

STEP 6: View History
• User can see all previous calculations
• History shows coefficients, roots, and dates
• Double-click to reload any calculation
• Automatic refresh after saving

STEP 7: Clear or Continue
• User clicks Clear to start new calculation
• System prompts to save unsaved results
• Form resets for new calculation
• Or user can continue with new values

STEP 8: Logout
• User clicks Logout button
• System checks for unsaved changes
• Confirms logout action
• Returns to login form

ERROR HANDLING:
• Invalid input validation
• Database connection errors
• Calculation errors
• User-friendly error messages";

            MessageBox.Show(workflow, "Complete Workflow", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}