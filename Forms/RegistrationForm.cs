using System;
using System.Drawing;
using System.Windows.Forms;
using UserCRUD.Models;
using UserCRUD.DAL;
using UserCRUD.Services;

namespace UserCRUD.Forms
{
    public partial class RegistrationForm : Form
    {
        private readonly UserDAL _userDAL;
        private readonly AuthenticationService _authService;

        // Controls
        private TextBox txtUsername;
        private TextBox txtPassword;
        private TextBox txtConfirmPassword;
        private TextBox txtFirstName;
        private TextBox txtLastName;
        private TextBox txtEmail;
        private TextBox txtPhone;
        private Button btnRegister;
        private Button btnBackToLogin;
        private Label lblTitle;
        private Label lblUsername;
        private Label lblPassword;
        private Label lblConfirmPassword;
        private Label lblFirstName;
        private Label lblLastName;
        private Label lblEmail;
        private Label lblPhone;
        private Label lblMessage;
        private CheckBox chkShowPassword;
        private CheckBox chkAgreeTerms;
        private ProgressBar progressBar;

        public bool RegistrationSuccessful { get; private set; }
        public User? RegisteredUser { get; private set; }

        public RegistrationForm()
        {
            _userDAL = new UserDAL();
            _authService = new AuthenticationService();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "User Registration - User Management System";
            this.Size = new Size(450, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            // Title
            lblTitle = new Label
            {
                Text = "Create New Account",
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(50, 20),
                Size = new Size(350, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);

            // First Name
            lblFirstName = new Label
            {
                Text = "First Name:",
                Location = new Point(50, 70),
                Size = new Size(100, 20),
                Font = new Font("Arial", 10)
            };
            this.Controls.Add(lblFirstName);

            txtFirstName = new TextBox
            {
                Location = new Point(50, 95),
                Size = new Size(150, 25),
                Font = new Font("Arial", 10),
                PlaceholderText = "Enter first name"
            };
            txtFirstName.KeyPress += TxtFirstName_KeyPress;
            this.Controls.Add(txtFirstName);

            // Last Name
            lblLastName = new Label
            {
                Text = "Last Name:",
                Location = new Point(220, 70),
                Size = new Size(100, 20),
                Font = new Font("Arial", 10)
            };
            this.Controls.Add(lblLastName);

            txtLastName = new TextBox
            {
                Location = new Point(220, 95),
                Size = new Size(150, 25),
                Font = new Font("Arial", 10),
                PlaceholderText = "Enter last name"
            };
            txtLastName.KeyPress += TxtLastName_KeyPress;
            this.Controls.Add(txtLastName);

            // Email
            lblEmail = new Label
            {
                Text = "Email Address:",
                Location = new Point(50, 130),
                Size = new Size(100, 20),
                Font = new Font("Arial", 10)
            };
            this.Controls.Add(lblEmail);

            txtEmail = new TextBox
            {
                Location = new Point(50, 155),
                Size = new Size(320, 25),
                Font = new Font("Arial", 10),
                PlaceholderText = "Enter email address"
            };
            txtEmail.KeyPress += TxtEmail_KeyPress;
            txtEmail.Leave += TxtEmail_Leave;
            this.Controls.Add(txtEmail);

            // Phone
            lblPhone = new Label
            {
                Text = "Phone Number (Optional):",
                Location = new Point(50, 190),
                Size = new Size(150, 20),
                Font = new Font("Arial", 10)
            };
            this.Controls.Add(lblPhone);

            txtPhone = new TextBox
            {
                Location = new Point(50, 215),
                Size = new Size(320, 25),
                Font = new Font("Arial", 10),
                PlaceholderText = "Enter phone number (optional)"
            };
            txtPhone.KeyPress += TxtPhone_KeyPress;
            this.Controls.Add(txtPhone);

            // Username
            lblUsername = new Label
            {
                Text = "Username:",
                Location = new Point(50, 250),
                Size = new Size(100, 20),
                Font = new Font("Arial", 10)
            };
            this.Controls.Add(lblUsername);

            txtUsername = new TextBox
            {
                Location = new Point(50, 275),
                Size = new Size(320, 25),
                Font = new Font("Arial", 10),
                PlaceholderText = "Choose a username"
            };
            txtUsername.KeyPress += TxtUsername_KeyPress;
            txtUsername.Leave += TxtUsername_Leave;
            this.Controls.Add(txtUsername);

            // Password
            lblPassword = new Label
            {
                Text = "Password:",
                Location = new Point(50, 310),
                Size = new Size(100, 20),
                Font = new Font("Arial", 10)
            };
            this.Controls.Add(lblPassword);

            txtPassword = new TextBox
            {
                Location = new Point(50, 335),
                Size = new Size(320, 25),
                Font = new Font("Arial", 10),
                UseSystemPasswordChar = true,
                PlaceholderText = "Enter password"
            };
            txtPassword.KeyPress += TxtPassword_KeyPress;
            txtPassword.TextChanged += TxtPassword_TextChanged;
            this.Controls.Add(txtPassword);

            // Confirm Password
            lblConfirmPassword = new Label
            {
                Text = "Confirm Password:",
                Location = new Point(50, 370),
                Size = new Size(120, 20),
                Font = new Font("Arial", 10)
            };
            this.Controls.Add(lblConfirmPassword);

            txtConfirmPassword = new TextBox
            {
                Location = new Point(50, 395),
                Size = new Size(320, 25),
                Font = new Font("Arial", 10),
                UseSystemPasswordChar = true,
                PlaceholderText = "Confirm password"
            };
            txtConfirmPassword.KeyPress += TxtConfirmPassword_KeyPress;
            txtConfirmPassword.TextChanged += TxtConfirmPassword_TextChanged;
            this.Controls.Add(txtConfirmPassword);

            // Show password checkbox
            chkShowPassword = new CheckBox
            {
                Text = "Show passwords",
                Location = new Point(50, 430),
                Size = new Size(150, 20),
                Font = new Font("Arial", 9)
            };
            chkShowPassword.CheckedChanged += ChkShowPassword_CheckedChanged;
            this.Controls.Add(chkShowPassword);

            // Terms agreement checkbox
            chkAgreeTerms = new CheckBox
            {
                Text = "I agree to the Terms of Service and Privacy Policy",
                Location = new Point(50, 455),
                Size = new Size(320, 20),
                Font = new Font("Arial", 9)
            };
            chkAgreeTerms.CheckedChanged += ChkAgreeTerms_CheckedChanged;
            this.Controls.Add(chkAgreeTerms);

            // Progress bar
            progressBar = new ProgressBar
            {
                Location = new Point(50, 485),
                Size = new Size(320, 20),
                Style = ProgressBarStyle.Continuous,
                Visible = false
            };
            this.Controls.Add(progressBar);

            // Message label
            lblMessage = new Label
            {
                Location = new Point(50, 510),
                Size = new Size(320, 40),
                Font = new Font("Arial", 9),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblMessage);

            // Register button
            btnRegister = new Button
            {
                Text = "Register",
                Location = new Point(180, 560),
                Size = new Size(90, 35),
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightGreen,
                UseVisualStyleBackColor = false,
                Enabled = false
            };
            btnRegister.Click += BtnRegister_Click;
            this.Controls.Add(btnRegister);

            // Back to Login button
            btnBackToLogin = new Button
            {
                Text = "Back to Login",
                Location = new Point(280, 560),
                Size = new Size(90, 35),
                Font = new Font("Arial", 10),
                BackColor = Color.LightGray,
                UseVisualStyleBackColor = false
            };
            btnBackToLogin.Click += BtnBackToLogin_Click;
            this.Controls.Add(btnBackToLogin);

            // Set tab order
            txtFirstName.TabIndex = 0;
            txtLastName.TabIndex = 1;
            txtEmail.TabIndex = 2;
            txtPhone.TabIndex = 3;
            txtUsername.TabIndex = 4;
            txtPassword.TabIndex = 5;
            txtConfirmPassword.TabIndex = 6;
            chkShowPassword.TabIndex = 7;
            chkAgreeTerms.TabIndex = 8;
            btnRegister.TabIndex = 9;
            btnBackToLogin.TabIndex = 10;

            // Set default button
            this.AcceptButton = btnRegister;

            // Focus on first name textbox
            this.Load += (s, e) => txtFirstName.Focus();
        }

        #region Event Handlers

        private void TxtFirstName_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                txtLastName.Focus();
                e.Handled = true;
            }
        }

        private void TxtLastName_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                txtEmail.Focus();
                e.Handled = true;
            }
        }

        private void TxtEmail_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                txtPhone.Focus();
                e.Handled = true;
            }
        }

        private void TxtEmail_Leave(object? sender, EventArgs e)
        {
            ValidateEmail();
        }

        private void TxtPhone_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                txtUsername.Focus();
                e.Handled = true;
            }
        }

        private void TxtUsername_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                txtPassword.Focus();
                e.Handled = true;
            }
        }

        private void TxtUsername_Leave(object? sender, EventArgs e)
        {
            ValidateUsername();
        }

        private void TxtPassword_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                txtConfirmPassword.Focus();
                e.Handled = true;
            }
        }

        private void TxtPassword_TextChanged(object? sender, EventArgs e)
        {
            ValidatePasswords();
            ValidateForm();
        }

        private void TxtConfirmPassword_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (btnRegister.Enabled)
                {
                    BtnRegister_Click(sender, e);
                }
                e.Handled = true;
            }
        }

        private void TxtConfirmPassword_TextChanged(object? sender, EventArgs e)
        {
            ValidatePasswords();
            ValidateForm();
        }

        private void ChkShowPassword_CheckedChanged(object? sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
            txtConfirmPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
        }

        private void ChkAgreeTerms_CheckedChanged(object? sender, EventArgs e)
        {
            ValidateForm();
        }

        private async void BtnRegister_Click(object? sender, EventArgs e)
        {
            await PerformRegistration();
        }

        private void BtnBackToLogin_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #endregion

        #region Validation Methods

        private bool ValidateEmail()
        {
            string email = txtEmail.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(email))
            {
                ShowFieldError("Email is required.");
                return false;
            }

            if (!IsValidEmail(email))
            {
                ShowFieldError("Please enter a valid email address.");
                return false;
            }

            ClearFieldError();
            return true;
        }

        private bool ValidateUsername()
        {
            string username = txtUsername.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(username))
            {
                ShowFieldError("Username is required.");
                return false;
            }

            if (username.Length < 3)
            {
                ShowFieldError("Username must be at least 3 characters long.");
                return false;
            }

            if (username.Length > 50)
            {
                ShowFieldError("Username cannot exceed 50 characters.");
                return false;
            }

            // Check for valid characters (alphanumeric, underscore, hyphen)
            foreach (char c in username)
            {
                if (!char.IsLetterOrDigit(c) && c != '_' && c != '-')
                {
                    ShowFieldError("Username can only contain letters, numbers, underscores, and hyphens.");
                    return false;
                }
            }

            ClearFieldError();
            return true;
        }

        private bool ValidatePasswords()
        {
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;

            // Validate password strength
            if (!string.IsNullOrEmpty(password))
            {
                if (!AuthenticationService.ValidatePasswordStrength(password))
                {
                    ShowFieldError("Password does not meet strength requirements.");
                    return false;
                }
            }

            // Validate password confirmation
            if (!string.IsNullOrEmpty(confirmPassword))
            {
                if (password != confirmPassword)
                {
                    ShowFieldError("Passwords do not match.");
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(confirmPassword) && password == confirmPassword)
            {
                ClearFieldError();
                return true;
            }

            return false;
        }

        private void ValidateForm()
        {
            bool isValid = !string.IsNullOrWhiteSpace(txtFirstName.Text) &&
                          !string.IsNullOrWhiteSpace(txtLastName.Text) &&
                          !string.IsNullOrWhiteSpace(txtEmail.Text) &&
                          !string.IsNullOrWhiteSpace(txtUsername.Text) &&
                          !string.IsNullOrWhiteSpace(txtPassword.Text) &&
                          !string.IsNullOrWhiteSpace(txtConfirmPassword.Text) &&
                          txtPassword.Text == txtConfirmPassword.Text &&
                          chkAgreeTerms.Checked;

            btnRegister.Enabled = isValid;
        }

        #endregion

        #region Registration Logic

        private async System.Threading.Tasks.Task PerformRegistration()
        {
            // Clear previous messages
            ClearFieldError();

            // Validate all fields
            if (!ValidateAllFields())
            {
                return;
            }

            // Disable controls during registration
            SetControlsEnabled(false);
            ShowProgress("Registering user...");

            try
            {
                // Check if username (email) already exists
                var existingUser = await System.Threading.Tasks.Task.Run(() => 
                    _userDAL.GetUserByEmail(txtEmail.Text.Trim()));

                if (existingUser != null)
                {
                    ShowFieldError("An account with this email address already exists.");
                    return;
                }

                // Create new user
                var newUser = new User
                {
                    FirstName = txtFirstName.Text.Trim(),
                    LastName = txtLastName.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Phone = txtPhone.Text.Trim(),
                    Role = UserRole.User, // Default role for new registrations
                    IsActive = true
                };

                // Set password (this will hash it automatically)
                newUser.SetPassword(txtPassword.Text);

                // Save user to database
                UpdateProgress(50, "Saving user information...");
                int userId = await System.Threading.Tasks.Task.Run(() => 
                    _userDAL.CreateUser(newUser));

                if (userId > 0)
                {
                    newUser.Id = userId;
                    RegisteredUser = newUser;
                    RegistrationSuccessful = true;

                    UpdateProgress(100, "Registration completed successfully!");
                    ShowFieldError("Registration successful! You can now log in.", Color.Green);

                    // Close form after short delay
                    await System.Threading.Tasks.Task.Delay(1500);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    ShowFieldError("Registration failed. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowFieldError($"Registration error: {ex.Message}");
            }
            finally
            {
                SetControlsEnabled(true);
                HideProgress();
            }
        }

        private bool ValidateAllFields()
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                ShowFieldError("First name is required.");
                txtFirstName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                ShowFieldError("Last name is required.");
                txtLastName.Focus();
                return false;
            }

            if (!ValidateEmail())
            {
                txtEmail.Focus();
                return false;
            }

            if (!ValidateUsername())
            {
                txtUsername.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                ShowFieldError("Password is required.");
                txtPassword.Focus();
                return false;
            }

            if (!AuthenticationService.ValidatePasswordStrength(txtPassword.Text))
            {
                ShowFieldError($"Password does not meet requirements:\n{AuthenticationService.GetPasswordRequirements()}");
                txtPassword.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtConfirmPassword.Text))
            {
                ShowFieldError("Please confirm your password.");
                txtConfirmPassword.Focus();
                return false;
            }

            if (txtPassword.Text != txtConfirmPassword.Text)
            {
                ShowFieldError("Passwords do not match.");
                txtConfirmPassword.Focus();
                return false;
            }

            if (!chkAgreeTerms.Checked)
            {
                ShowFieldError("You must agree to the Terms of Service and Privacy Policy.");
                chkAgreeTerms.Focus();
                return false;
            }

            return true;
        }

        #endregion

        #region Helper Methods

        private void ShowFieldError(string message, Color? color = null)
        {
            lblMessage.Text = message;
            lblMessage.ForeColor = color ?? Color.Red;
        }

        private void ClearFieldError()
        {
            lblMessage.Text = "";
        }

        private void SetControlsEnabled(bool enabled)
        {
            txtFirstName.Enabled = enabled;
            txtLastName.Enabled = enabled;
            txtEmail.Enabled = enabled;
            txtPhone.Enabled = enabled;
            txtUsername.Enabled = enabled;
            txtPassword.Enabled = enabled;
            txtConfirmPassword.Enabled = enabled;
            chkShowPassword.Enabled = enabled;
            chkAgreeTerms.Enabled = enabled;
            btnRegister.Enabled = enabled && ValidateAllFieldsQuick();
            btnBackToLogin.Enabled = enabled;
        }

        private bool ValidateAllFieldsQuick()
        {
            return !string.IsNullOrWhiteSpace(txtFirstName.Text) &&
                   !string.IsNullOrWhiteSpace(txtLastName.Text) &&
                   !string.IsNullOrWhiteSpace(txtEmail.Text) &&
                   !string.IsNullOrWhiteSpace(txtUsername.Text) &&
                   !string.IsNullOrWhiteSpace(txtPassword.Text) &&
                   !string.IsNullOrWhiteSpace(txtConfirmPassword.Text) &&
                   txtPassword.Text == txtConfirmPassword.Text &&
                   chkAgreeTerms.Checked;
        }

        private void ShowProgress(string message)
        {
            progressBar.Visible = true;
            progressBar.Value = 0;
            ShowFieldError(message, Color.Blue);
        }

        private void UpdateProgress(int value, string message)
        {
            progressBar.Value = Math.Min(value, 100);
            ShowFieldError(message, Color.Blue);
        }

        private void HideProgress()
        {
            progressBar.Visible = false;
        }

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

        #endregion

        #region Static Methods

        /// <summary>
        /// Shows the registration form as a dialog
        /// </summary>
        /// <param name="parent">Parent window</param>
        /// <returns>Registered user if successful, null otherwise</returns>
        public static User? ShowRegistrationDialog(IWin32Window? parent = null)
        {
            using (var registrationForm = new RegistrationForm())
            {
                var result = registrationForm.ShowDialog(parent);
                
                if (result == DialogResult.OK && registrationForm.RegistrationSuccessful)
                {
                    return registrationForm.RegisteredUser;
                }
                
                return null;
            }
        }

        /// <summary>
        /// Shows the registration form and returns to login form
        /// </summary>
        /// <param name="parent">Parent window</param>
        /// <returns>True if user should return to login, false to exit</returns>
        public static bool ShowRegistrationWithReturn(IWin32Window? parent = null)
        {
            using (var registrationForm = new RegistrationForm())
            {
                var result = registrationForm.ShowDialog(parent);
                return result == DialogResult.OK || result == DialogResult.Cancel;
            }
        }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            // Test database connection
            if (!_authService.TestConnection())
            {
                ShowFieldError("Warning: Cannot connect to database. Please check your connection.", Color.Orange);
            }
        }
    }
}