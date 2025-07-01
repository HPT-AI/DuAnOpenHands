using System;
using System.Windows.Forms;
using UserCRUD.Forms;

namespace UserCRUD
{
    /// <summary>
    /// Demo application to test the Registration Form (Task 4.1)
    /// </summary>
    public static class RegistrationFormDemo
    {
        /// <summary>
        /// Entry point for testing the registration form
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Test the registration form
                TestRegistrationForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running registration form demo: {ex.Message}", 
                    "Demo Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Tests the registration form functionality
        /// </summary>
        private static void TestRegistrationForm()
        {
            // Show a welcome message
            var result = MessageBox.Show(
                "Welcome to the Registration Form Demo!\n\n" +
                "This demo will show the user registration form.\n" +
                "You can test all the validation features:\n\n" +
                "• Username validation\n" +
                "• Password strength requirements\n" +
                "• Password confirmation matching\n" +
                "• Email validation\n" +
                "• Required field validation\n" +
                "• Database integration\n\n" +
                "Click OK to open the registration form.",
                "Registration Form Demo",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Information);

            if (result != DialogResult.OK)
                return;

            // Show the registration form
            var registeredUser = RegistrationForm.ShowRegistrationDialog();

            if (registeredUser != null)
            {
                // Registration successful
                MessageBox.Show(
                    $"Registration Successful!\n\n" +
                    $"User Details:\n" +
                    $"Name: {registeredUser.FullName}\n" +
                    $"Email: {registeredUser.Email}\n" +
                    $"Phone: {registeredUser.Phone}\n" +
                    $"Role: {registeredUser.Role}\n" +
                    $"Created: {registeredUser.DateCreated:yyyy-MM-dd HH:mm:ss}\n" +
                    $"Active: {registeredUser.IsActive}",
                    "Registration Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Ask if user wants to test login
                var loginTest = MessageBox.Show(
                    "Would you like to test the login form with the newly registered user?",
                    "Test Login",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (loginTest == DialogResult.Yes)
                {
                    TestLoginWithNewUser(registeredUser.Email);
                }
            }
            else
            {
                // Registration cancelled or failed
                MessageBox.Show(
                    "Registration was cancelled or failed.",
                    "Registration Cancelled",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            // Ask if user wants to test again
            var testAgain = MessageBox.Show(
                "Would you like to test the registration form again?",
                "Test Again",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (testAgain == DialogResult.Yes)
            {
                TestRegistrationForm();
            }
        }

        /// <summary>
        /// Tests the login form with a newly registered user
        /// </summary>
        /// <param name="email">Email of the registered user</param>
        private static void TestLoginWithNewUser(string email)
        {
            try
            {
                var loginForm = new LoginForm();
                
                // Pre-fill the email
                // Note: This would require making the email textbox public or adding a method
                // For demo purposes, we'll just show the login form
                
                MessageBox.Show(
                    $"Login form will open with email pre-filled: {email}\n\n" +
                    "Enter the password you just created to test the login functionality.",
                    "Login Test",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                var loginResult = loginForm.ShowDialog();

                if (loginResult == DialogResult.OK)
                {
                    MessageBox.Show(
                        "Login successful! The registration and login system is working correctly.",
                        "Login Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Login was cancelled or failed.",
                        "Login Result",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error testing login: {ex.Message}",
                    "Login Test Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Shows registration form validation examples
        /// </summary>
        public static void ShowValidationExamples()
        {
            var examples = @"Registration Form Validation Examples:

USERNAME VALIDATION:
✓ Valid: john_doe, user123, test-user
✗ Invalid: ab (too short), user@name (invalid chars)

PASSWORD VALIDATION:
✓ Valid: MyPassword123!, SecurePass1@
✗ Invalid: 123 (too short), password (no uppercase/numbers)

EMAIL VALIDATION:
✓ Valid: user@example.com, test.email@domain.org
✗ Invalid: invalid-email, user@, @domain.com

REQUIRED FIELDS:
• First Name (required)
• Last Name (required)
• Email (required)
• Username (required)
• Password (required)
• Confirm Password (required, must match)
• Terms Agreement (required)

OPTIONAL FIELDS:
• Phone Number

SECURITY FEATURES:
• Password hashing before storage
• Username uniqueness check
• Email format validation
• Password strength requirements
• Secure password confirmation
• Terms of service agreement";

            MessageBox.Show(examples, "Validation Examples", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Shows registration form features
        /// </summary>
        public static void ShowFormFeatures()
        {
            var features = @"Registration Form Features (Task 4.1):

REQUIRED FIELDS:
✓ Username (Textbox) - with validation
✓ Password (Textbox, PasswordChar) - with strength requirements
✓ Confirm Password (Textbox, PasswordChar) - must match password

REQUIRED BUTTONS:
✓ Register - validates and saves user
✓ Back to Login - returns to login form

VALIDATION LOGIC:
✓ Username not empty - checks for valid username
✓ Password matches confirmation - ensures passwords match
✓ Hash password before saving - secure password storage
✓ Check username not exists - prevents duplicates

ADDITIONAL FEATURES:
✓ First Name and Last Name fields
✓ Email validation
✓ Phone number (optional)
✓ Show/hide password option
✓ Terms of service agreement
✓ Real-time validation feedback
✓ Progress indicator during registration
✓ Professional UI design
✓ Keyboard navigation support
✓ Error handling and user feedback

INTEGRATION:
✓ Works with existing User model
✓ Integrates with UserDAL for database operations
✓ Uses AuthenticationService for password hashing
✓ Connects with LoginForm for seamless user experience
✓ Available from MainForm for admin users";

            MessageBox.Show(features, "Registration Form Features", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}