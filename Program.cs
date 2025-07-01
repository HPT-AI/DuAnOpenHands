using System;
using System.Windows.Forms;
using UserCRUD.Forms;

namespace UserCRUD
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Enable visual styles and set compatible text rendering
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Show welcome message for complete application demonstration
                var welcomeResult = MessageBox.Show(
                    "Welcome to the User CRUD Application!\n\n" +
                    "This application demonstrates:\n" +
                    "• Task 4.1: Registration Form ✓\n" +
                    "• Task 4.2: Login Form ✓\n" +
                    "• Task 4.3: User Main Form ✓\n" +
                    "• Complete user management system\n" +
                    "• Quadratic equation calculator\n\n" +
                    "Click OK to start with login (recommended), or Cancel to go directly to admin form.",
                    "User CRUD Application - Complete Demo",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Information);

                if (welcomeResult == DialogResult.OK)
                {
                    // Start with Task 4.2 login form
                    if (LoginFormTask42.ShowLoginAndStartApplication())
                    {
                        // Login successful, main application is running
                        // The login form handles the redirection based on user role
                        Application.Run(); // Keep the application running
                    }
                    else
                    {
                        // Login cancelled - ask if user wants to see main form anyway
                        var fallbackResult = MessageBox.Show(
                            "Login was cancelled.\n\n" +
                            "Would you like to open the main form directly for testing purposes?",
                            "Login Cancelled",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (fallbackResult == DialogResult.Yes)
                        {
                            Application.Run(new MainForm());
                        }
                    }
                }
                else
                {
                    // Go directly to main form (for testing/development)
                    Application.Run(new MainForm());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"An unexpected error occurred: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                    "Application Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}