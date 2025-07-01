using System;
using System.Drawing;
using System.Windows.Forms;
using UserCRUD.Services;
using UserCRUD.Models;

namespace UserCRUD.Forms
{
    public partial class LoginForm : Form
    {
        private readonly AuthenticationService _authService;
        private User? _authenticatedUser;

        // Controls
        private TextBox txtEmail;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnCancel;
        private Label lblTitle;
        private Label lblEmail;
        private Label lblPassword;
        private Label lblMessage;
        private CheckBox chkShowPassword;
        private LinkLabel lnkForgotPassword;

        public User? AuthenticatedUser => _authenticatedUser;
        public bool LoginSuccessful { get; private set; }

        public LoginForm()
        {
            _authService = new AuthenticationService();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Login - User Management System";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            // Title
            lblTitle = new Label
            {
                Text = "User Management System",
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(50, 20),
                Size = new Size(300, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);

            // Email label and textbox
            lblEmail = new Label
            {
                Text = "Email:",
                Location = new Point(50, 70),
                Size = new Size(80, 20),
                Font = new Font("Arial", 10)
            };
            this.Controls.Add(lblEmail);

            txtEmail = new TextBox
            {
                Location = new Point(50, 95),
                Size = new Size(300, 25),
                Font = new Font("Arial", 10),
                PlaceholderText = "Enter your email address"
            };
            txtEmail.KeyPress += TxtEmail_KeyPress;
            this.Controls.Add(txtEmail);

            // Password label and textbox
            lblPassword = new Label
            {
                Text = "Password:",
                Location = new Point(50, 130),
                Size = new Size(80, 20),
                Font = new Font("Arial", 10)
            };
            this.Controls.Add(lblPassword);

            txtPassword = new TextBox
            {
                Location = new Point(50, 155),
                Size = new Size(300, 25),
                Font = new Font("Arial", 10),
                UseSystemPasswordChar = true,
                PlaceholderText = "Enter your password"
            };
            txtPassword.KeyPress += TxtPassword_KeyPress;
            this.Controls.Add(txtPassword);

            // Show password checkbox
            chkShowPassword = new CheckBox
            {
                Text = "Show password",
                Location = new Point(50, 185),
                Size = new Size(120, 20),
                Font = new Font("Arial", 9)
            };
            chkShowPassword.CheckedChanged += ChkShowPassword_CheckedChanged;
            this.Controls.Add(chkShowPassword);

            // Message label
            lblMessage = new Label
            {
                Location = new Point(50, 210),
                Size = new Size(300, 40),
                Font = new Font("Arial", 9),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblMessage);

            // Login button
            btnLogin = new Button
            {
                Text = "Login",
                Location = new Point(180, 260),
                Size = new Size(80, 30),
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightBlue,
                UseVisualStyleBackColor = false
            };
            btnLogin.Click += BtnLogin_Click;
            this.Controls.Add(btnLogin);

            // Cancel button
            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(270, 260),
                Size = new Size(80, 30),
                Font = new Font("Arial", 10),
                DialogResult = DialogResult.Cancel
            };
            btnCancel.Click += BtnCancel_Click;
            this.Controls.Add(btnCancel);

            // Forgot password link
            lnkForgotPassword = new LinkLabel
            {
                Text = "Forgot Password?",
                Location = new Point(50, 265),
                Size = new Size(120, 20),
                Font = new Font("Arial", 9),
                LinkColor = Color.Blue
            };
            lnkForgotPassword.LinkClicked += LnkForgotPassword_LinkClicked;
            this.Controls.Add(lnkForgotPassword);

            // Register link
            var lnkRegister = new LinkLabel
            {
                Text = "Create New Account",
                Location = new Point(50, 285),
                Size = new Size(150, 20),
                Font = new Font("Arial", 9),
                LinkColor = Color.Green
            };
            lnkRegister.LinkClicked += LnkRegister_LinkClicked;
            this.Controls.Add(lnkRegister);

            // Set default button and cancel button
            this.AcceptButton = btnLogin;
            this.CancelButton = btnCancel;

            // Focus on email textbox
            this.Load += (s, e) => txtEmail.Focus();
        }

        private void TxtEmail_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                txtPassword.Focus();
                e.Handled = true;
            }
        }

        private void TxtPassword_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                BtnLogin_Click(sender, e);
                e.Handled = true;
            }
        }

        private void ChkShowPassword_CheckedChanged(object? sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
        }

        private async void BtnLogin_Click(object? sender, EventArgs e)
        {
            await PerformLogin();
        }

        private async System.Threading.Tasks.Task PerformLogin()
        {
            // Clear previous messages
            lblMessage.Text = "";
            lblMessage.ForeColor = Color.Red;

            // Validate input
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                ShowMessage("Please enter your email address.", Color.Red);
                txtEmail.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                ShowMessage("Please enter your password.", Color.Red);
                txtPassword.Focus();
                return;
            }

            // Disable controls during login
            SetControlsEnabled(false);
            ShowMessage("Logging in...", Color.Blue);

            try
            {
                // Perform login
                var loginResponse = await System.Threading.Tasks.Task.Run(() => 
                    _authService.Login(txtEmail.Text.Trim(), txtPassword.Text));

                // Handle login result
                switch (loginResponse.Result)
                {
                    case LoginResult.Success:
                        _authenticatedUser = loginResponse.User;
                        LoginSuccessful = true;
                        ShowMessage("Login successful!", Color.Green);
                        
                        // Close form after short delay
                        await System.Threading.Tasks.Task.Delay(500);
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                        break;

                    case LoginResult.InvalidCredentials:
                        ShowMessage(loginResponse.Message, Color.Red);
                        txtPassword.Clear();
                        txtPassword.Focus();
                        
                        if (loginResponse.RemainingAttempts == 0)
                        {
                            ShowMessage($"{loginResponse.Message}\nAccount locked until: {loginResponse.LockoutExpiry:yyyy-MM-dd HH:mm}", Color.Red);
                        }
                        break;

                    case LoginResult.UserNotFound:
                        ShowMessage("Invalid email or password.", Color.Red);
                        txtPassword.Clear();
                        txtEmail.Focus();
                        break;

                    case LoginResult.UserInactive:
                        ShowMessage(loginResponse.Message, Color.Orange);
                        break;

                    case LoginResult.UserLocked:
                        ShowMessage(loginResponse.Message, Color.Orange);
                        break;

                    case LoginResult.DatabaseError:
                        ShowMessage("Database connection error. Please try again later.", Color.Red);
                        break;

                    case LoginResult.InvalidInput:
                        ShowMessage(loginResponse.Message, Color.Red);
                        break;

                    default:
                        ShowMessage("An unexpected error occurred. Please try again.", Color.Red);
                        break;
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"An error occurred during login: {ex.Message}", Color.Red);
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        private void ShowMessage(string message, Color color)
        {
            lblMessage.Text = message;
            lblMessage.ForeColor = color;
        }

        private void SetControlsEnabled(bool enabled)
        {
            txtEmail.Enabled = enabled;
            txtPassword.Enabled = enabled;
            btnLogin.Enabled = enabled;
            chkShowPassword.Enabled = enabled;
            
            if (enabled)
            {
                btnLogin.Text = "Login";
            }
            else
            {
                btnLogin.Text = "Logging in...";
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void LnkForgotPassword_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show(
                "To reset your password, please contact your system administrator.\n\n" +
                "For security reasons, password resets must be performed by an administrator.",
                "Password Reset",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void LnkRegister_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            // Hide login form temporarily
            this.Hide();
            
            try
            {
                // Show registration form
                var registeredUser = RegistrationForm.ShowRegistrationDialog(this);
                
                if (registeredUser != null)
                {
                    // Registration successful - pre-fill email and show success message
                    txtEmail.Text = registeredUser.Email;
                    txtPassword.Clear();
                    ShowMessage($"Registration successful! Welcome {registeredUser.FullName}. Please log in.", Color.Green);
                    txtPassword.Focus();
                }
                else
                {
                    // Registration cancelled or failed
                    ShowMessage("Registration cancelled.", Color.Orange);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Error opening registration form: {ex.Message}", Color.Red);
            }
            finally
            {
                // Show login form again
                this.Show();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            // Test database connection
            if (!_authService.TestConnection())
            {
                ShowMessage("Warning: Cannot connect to database. Please check your connection.", Color.Orange);
            }
        }

        /// <summary>
        /// Shows the login form and returns the authenticated user
        /// </summary>
        /// <param name="parent">Parent form</param>
        /// <returns>Authenticated user if login successful, null otherwise</returns>
        public static User? ShowLoginDialog(IWin32Window? parent = null)
        {
            using (var loginForm = new LoginForm())
            {
                var result = loginForm.ShowDialog(parent);
                
                if (result == DialogResult.OK && loginForm.LoginSuccessful)
                {
                    return loginForm.AuthenticatedUser;
                }
                
                return null;
            }
        }

        /// <summary>
        /// Validates email format
        /// </summary>
        /// <param name="email">Email to validate</param>
        /// <returns>True if email format is valid</returns>
        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}