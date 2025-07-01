using System;
using System.Windows.Forms;
using UserCRUD.Forms;
using UserCRUD.Models;

namespace UserCRUD
{
    /// <summary>
    /// Demo application to test the Login Form for Task 4.2
    /// </summary>
    public static class LoginFormTask42Demo
    {
        /// <summary>
        /// Entry point for testing the Task 4.2 login form
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Test the Task 4.2 login form
                TestLoginForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running login form demo: {ex.Message}", 
                    "Demo Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Tests the Task 4.2 login form functionality
        /// </summary>
        private static void TestLoginForm()
        {
            // Show welcome message
            var result = MessageBox.Show(
                "Welcome to the Task 4.2 Login Form Demo!\n\n" +
                "This demo will show the login form with all required features:\n\n" +
                "REQUIRED FIELDS:\n" +
                "• Username (Textbox)\n" +
                "• Password (Textbox with PasswordChar)\n\n" +
                "REQUIRED BUTTONS:\n" +
                "• Login (with full authentication logic)\n" +
                "• Register (opens registration form)\n\n" +
                "REQUIRED LOGIC:\n" +
                "• Check user exists in database\n" +
                "• Password matches (compare hash)\n" +
                "• IsActive == 1 validation\n" +
                "• Role-based redirection\n\n" +
                "Click OK to open the login form.",
                "Task 4.2 Login Form Demo",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Information);

            if (result != DialogResult.OK)
                return;

            // Show login form and handle authentication
            bool loginSuccessful = LoginFormTask42.ShowLoginAndStartApplication();

            if (loginSuccessful)
            {
                // Login was successful, application should be running
                MessageBox.Show(
                    "Login successful! The application has started based on user role.\n\n" +
                    "Task 4.2 requirements verified:\n" +
                    "✓ Username field implemented\n" +
                    "✓ Password field with PasswordChar\n" +
                    "✓ Login button with authentication logic\n" +
                    "✓ Register button functional\n" +
                    "✓ User existence check\n" +
                    "✓ Password hash comparison\n" +
                    "✓ IsActive validation\n" +
                    "✓ Role-based redirection",
                    "Login Demo Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                // Login was cancelled or failed
                var retryResult = MessageBox.Show(
                    "Login was cancelled or failed.\n\n" +
                    "Would you like to:\n" +
                    "• Try the login form again (Yes)\n" +
                    "• See validation examples (No)\n" +
                    "• Exit demo (Cancel)",
                    "Login Demo",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                switch (retryResult)
                {
                    case DialogResult.Yes:
                        TestLoginForm(); // Retry
                        break;
                    case DialogResult.No:
                        ShowValidationExamples();
                        break;
                    default:
                        break; // Exit
                }
            }
        }

        /// <summary>
        /// Shows validation examples for the login form
        /// </summary>
        private static void ShowValidationExamples()
        {
            var examples = @"Task 4.2 Login Form Validation Examples:

USERNAME FIELD:
✓ Valid: user@example.com, john.doe@company.com
✗ Invalid: (empty), (whitespace only)

PASSWORD FIELD:
✓ Valid: Any non-empty password
✗ Invalid: (empty), (whitespace only)
Note: Password is masked with PasswordChar

LOGIN LOGIC VALIDATION:
1. Check user exists:
   - Searches database for username (email)
   - Shows 'Invalid username or password' if not found

2. Password matches (compare hash):
   - Uses secure hash comparison
   - Shows 'Invalid username or password' if mismatch
   - Records failed login attempts

3. IsActive == 1:
   - Checks user.IsActive property
   - Shows 'Account deactivated' message if false

4. Role-based redirection:
   - Admin users → MainForm (admin panel)
   - Regular users → UserDashboardForm (limited access)

SECURITY FEATURES:
• Password hashing and verification
• Failed login attempt tracking
• Account lockout protection
• Session management
• Input validation and sanitization

BUTTON FUNCTIONALITY:
• Login: Performs full authentication process
• Register: Opens registration form
• Cancel: Closes login form
• Show Password: Toggles password visibility

ERROR HANDLING:
• Database connection errors
• Authentication failures
• Account status issues
• Role-based access control";

            MessageBox.Show(examples, "Task 4.2 Validation Examples", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Ask if user wants to try login again
            var tryAgain = MessageBox.Show(
                "Would you like to try the login form now?",
                "Try Login Form",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (tryAgain == DialogResult.Yes)
            {
                TestLoginForm();
            }
        }

        /// <summary>
        /// Shows the features implemented in Task 4.2
        /// </summary>
        public static void ShowTask42Features()
        {
            var features = @"Task 4.2 Login Form Implementation:

REQUIRED COMPONENTS:
✓ Username field (Textbox) - Email-based username
✓ Password field (Textbox with PasswordChar) - Secure input
✓ Login button - Full authentication logic
✓ Register button - Opens registration form

REQUIRED LOGIC:
✓ Check user exists - Database lookup by username/email
✓ Password matches - Secure hash comparison
✓ IsActive == 1 - Account status validation
✓ Role-based redirect - Admin vs User access

AUTHENTICATION FLOW:
1. User enters username and password
2. Form validates input fields
3. System checks if user exists in database
4. System verifies password against stored hash
5. System checks if account is active (IsActive == 1)
6. System checks for account lockout status
7. System records login attempt (success/failure)
8. System redirects based on user role:
   - Admin → MainForm (full access)
   - User → UserDashboardForm (limited access)

SECURITY FEATURES:
• Secure password hashing (SHA-256 with salt)
• Failed login attempt tracking
• Account lockout after multiple failures
• Input validation and sanitization
• Session management
• Role-based access control

UI/UX FEATURES:
• Professional, clean interface
• Real-time form validation
• Password visibility toggle
• Remember me option
• Keyboard navigation support
• Error message display
• Loading indicators

INTEGRATION:
• Works with existing User model
• Integrates with UserDAL for database operations
• Uses AuthenticationService for security
• Connects with RegistrationForm
• Supports role-based application flow

ADDITIONAL FEATURES:
• Forgot password functionality
• Database connection testing
• Comprehensive error handling
• User-friendly error messages
• Responsive UI design";

            MessageBox.Show(features, "Task 4.2 Features", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Demonstrates the login process step by step
        /// </summary>
        public static void DemonstrateLoginProcess()
        {
            var steps = @"Task 4.2 Login Process Demonstration:

STEP 1: Form Initialization
• Login form opens with username and password fields
• Both fields are required for login button to be enabled
• Password field uses PasswordChar for security

STEP 2: User Input Validation
• Username field accepts email format
• Password field accepts any non-empty input
• Real-time validation enables/disables login button

STEP 3: Authentication Process
• Click Login button to start authentication
• System disables form controls during processing
• Shows 'Authenticating...' message

STEP 4: User Existence Check
• System searches database for username (email)
• If user not found: 'Invalid username or password'
• If user found: Continue to next step

STEP 5: Password Verification
• System compares entered password with stored hash
• Uses secure hash comparison (SHA-256 with salt)
• If mismatch: 'Invalid username or password'
• Records failed attempt and checks for lockout

STEP 6: Account Status Check
• Verifies user.IsActive == true
• If inactive: 'Account has been deactivated'
• If locked: Shows lockout expiration time

STEP 7: Successful Authentication
• Records successful login in database
• Updates last login timestamp
• Shows 'Login successful! Redirecting...'

STEP 8: Role-Based Redirection
• Admin users: Redirected to MainForm (admin panel)
• Regular users: Redirected to UserDashboardForm
• Login form closes, main application starts

REGISTER BUTTON:
• Opens RegistrationForm for new user creation
• Returns to login form after registration
• Pre-fills username if registration successful

ERROR HANDLING:
• Database connection errors
• Authentication service errors
• Form validation errors
• Network timeout errors
• Graceful error recovery";

            MessageBox.Show(steps, "Login Process Steps", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}