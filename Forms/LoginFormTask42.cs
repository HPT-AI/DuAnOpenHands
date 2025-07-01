using System;
using System.Drawing;
using System.Windows.Forms;
using UserCRUD.Services;
using UserCRUD.Models;
using UserCRUD.DAL;

namespace UserCRUD.Forms
{
    /// <summary>
    /// Login Form implementation for Task 4.2
    /// Provides user authentication with username/password and role-based redirection
    /// </summary>
    public partial class LoginFormTask42 : Form
    {
        private readonly AuthenticationService _authService;
        private readonly UserDAL _userDAL;
        private User? _authenticatedUser;

        // Controls - Task 4.2 Requirements
        private TextBox txtUsername;      // Username field (required)
        private TextBox txtPassword;      // Password field with PasswordChar (required)
        private Button btnLogin;          // Login button (required)
        private Button btnRegister;       // Register button (required)
        
        // Additional UI Controls
        private Label lblTitle;
        private Label lblUsername;
        private Label lblPassword;
        private Label lblMessage;
        private CheckBox chkShowPassword;
        private CheckBox chkRememberMe;
        private LinkLabel lnkForgotPassword;
        private PictureBox picLogo;
        private Panel pnlMain;

        // Properties
        public User? AuthenticatedUser => _authenticatedUser;
        public bool LoginSuccessful { get; private set; }

        public LoginFormTask42()
        {
            _authService = new AuthenticationService();
            _userDAL = new UserDAL();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Form properties
            this.Text = "Login - User Management System";
            this.Size = new Size(450, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(240, 248, 255); // Light blue background
            this.Icon = SystemIcons.Shield; // Security icon

            // Main panel
            pnlMain = new Panel
            {
                Location = new Point(50, 30),
                Size = new Size(350, 420),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(pnlMain);

            // Logo/Icon
            picLogo = new PictureBox
            {
                Location = new Point(150, 20),
                Size = new Size(50, 50),
                Image = SystemIcons.Shield.ToBitmap(),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            pnlMain.Controls.Add(picLogo);

            // Title
            lblTitle = new Label
            {
                Text = "User Login",
                Font = new Font("Arial", 18, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(50, 80),
                Size = new Size(250, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlMain.Controls.Add(lblTitle);

            // Username label
            lblUsername = new Label
            {
                Text = "Username:",
                Location = new Point(30, 130),
                Size = new Size(80, 20),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            pnlMain.Controls.Add(lblUsername);

            // Username textbox (Task 4.2 requirement)
            txtUsername = new TextBox
            {
                Location = new Point(30, 155),
                Size = new Size(290, 25),
                Font = new Font("Arial", 11),
                PlaceholderText = "Enter your username"
            };
            txtUsername.KeyPress += TxtUsername_KeyPress;
            txtUsername.TextChanged += ValidateForm;
            pnlMain.Controls.Add(txtUsername);

            // Password label
            lblPassword = new Label
            {
                Text = "Password:",
                Location = new Point(30, 190),
                Size = new Size(80, 20),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            pnlMain.Controls.Add(lblPassword);

            // Password textbox (Task 4.2 requirement with PasswordChar)
            txtPassword = new TextBox
            {
                Location = new Point(30, 215),
                Size = new Size(290, 25),
                Font = new Font("Arial", 11),
                UseSystemPasswordChar = true, // PasswordChar requirement
                PlaceholderText = "Enter your password"
            };
            txtPassword.KeyPress += TxtPassword_KeyPress;
            txtPassword.TextChanged += ValidateForm;
            pnlMain.Controls.Add(txtPassword);

            // Show password checkbox
            chkShowPassword = new CheckBox
            {
                Text = "Show password",
                Location = new Point(30, 250),
                Size = new Size(120, 20),
                Font = new Font("Arial", 9)
            };
            chkShowPassword.CheckedChanged += ChkShowPassword_CheckedChanged;
            pnlMain.Controls.Add(chkShowPassword);

            // Remember me checkbox
            chkRememberMe = new CheckBox
            {
                Text = "Remember me",
                Location = new Point(200, 250),
                Size = new Size(120, 20),
                Font = new Font("Arial", 9)
            };
            pnlMain.Controls.Add(chkRememberMe);

            // Message label
            lblMessage = new Label
            {
                Location = new Point(30, 280),
                Size = new Size(290, 40),
                Font = new Font("Arial", 9),
                ForeColor = Color.Red,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlMain.Controls.Add(lblMessage);

            // Login button (Task 4.2 requirement)
            btnLogin = new Button
            {
                Text = "Login",
                Location = new Point(30, 330),
                Size = new Size(100, 35),
                Font = new Font("Arial", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 123, 255), // Blue
                ForeColor = Color.White,
                UseVisualStyleBackColor = false,
                Enabled = false
            };
            btnLogin.Click += BtnLogin_Click;
            pnlMain.Controls.Add(btnLogin);

            // Register button (Task 4.2 requirement)
            btnRegister = new Button
            {
                Text = "Register",
                Location = new Point(140, 330),
                Size = new Size(100, 35),
                Font = new Font("Arial", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(40, 167, 69), // Green
                ForeColor = Color.White,
                UseVisualStyleBackColor = false
            };
            btnRegister.Click += BtnRegister_Click;
            pnlMain.Controls.Add(btnRegister);

            // Cancel button
            var btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(250, 330),
                Size = new Size(70, 35),
                Font = new Font("Arial", 10),
                BackColor = Color.FromArgb(108, 117, 125), // Gray
                ForeColor = Color.White,
                UseVisualStyleBackColor = false
            };
            btnCancel.Click += BtnCancel_Click;
            pnlMain.Controls.Add(btnCancel);

            // Forgot password link
            lnkForgotPassword = new LinkLabel
            {
                Text = "Forgot Password?",
                Location = new Point(30, 380),
                Size = new Size(120, 20),
                Font = new Font("Arial", 9),
                LinkColor = Color.Blue
            };
            lnkForgotPassword.LinkClicked += LnkForgotPassword_LinkClicked;
            pnlMain.Controls.Add(lnkForgotPassword);

            // Version info
            var lblVersion = new Label
            {
                Text = "Version 1.0 - Task 4.2 Implementation",
                Location = new Point(150, 380),
                Size = new Size(170, 20),
                Font = new Font("Arial", 8),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleRight
            };
            pnlMain.Controls.Add(lblVersion);

            // Set tab order
            txtUsername.TabIndex = 0;
            txtPassword.TabIndex = 1;
            chkShowPassword.TabIndex = 2;
            chkRememberMe.TabIndex = 3;
            btnLogin.TabIndex = 4;
            btnRegister.TabIndex = 5;

            // Set default and cancel buttons
            this.AcceptButton = btnLogin;
            this.CancelButton = btnCancel;

            // Focus on username field when form loads
            this.Load += (s, e) => txtUsername.Focus();
        }

        #region Event Handlers

        private void TxtUsername_KeyPress(object? sender, KeyPressEventArgs e)
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
                if (btnLogin.Enabled)
                {
                    BtnLogin_Click(sender, e);
                }
                e.Handled = true;
            }
        }

        private void ValidateForm(object? sender, EventArgs e)
        {
            bool isValid = !string.IsNullOrWhiteSpace(txtUsername.Text) && 
                          !string.IsNullOrWhiteSpace(txtPassword.Text);
            btnLogin.Enabled = isValid;
        }

        private void ChkShowPassword_CheckedChanged(object? sender, EventArgs e)
        {
            txtPassword.UseSystemPasswordChar = !chkShowPassword.Checked;
        }

        // Task 4.2 Login Logic Implementation
        private async void BtnLogin_Click(object? sender, EventArgs e)
        {
            await PerformLogin();
        }

        // Task 4.2 Register Button Implementation
        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            try
            {
                // Hide login form temporarily
                this.Hide();
                
                // Show registration form
                var registeredUser = RegistrationForm.ShowRegistrationDialog(this);
                
                if (registeredUser != null)
                {
                    // Registration successful - pre-fill username and show success message
                    txtUsername.Text = registeredUser.Email; // Use email as username
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

        #endregion

        #region Task 4.2 Login Logic Implementation

        /// <summary>
        /// Main login logic implementation for Task 4.2
        /// Implements all required validation: user exists, password matches, IsActive == 1, role-based redirect
        /// </summary>
        private async System.Threading.Tasks.Task PerformLogin()
        {
            // Clear previous messages
            ClearMessage();

            // Get input values
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            // Basic validation
            if (string.IsNullOrWhiteSpace(username))
            {
                ShowMessage("Username is required.", Color.Red);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowMessage("Password is required.", Color.Red);
                txtPassword.Focus();
                return;
            }

            // Disable controls during login
            SetControlsEnabled(false);
            ShowMessage("Authenticating...", Color.Blue);

            try
            {
                // Task 4.2 Requirement: Check user exists
                var user = await System.Threading.Tasks.Task.Run(() => 
                    _userDAL.GetUserByEmail(username)); // Using email as username

                if (user == null)
                {
                    ShowMessage("Invalid username or password.", Color.Red);
                    return;
                }

                // Task 4.2 Requirement: Check IsActive == 1
                if (!user.IsActive)
                {
                    ShowMessage("Your account has been deactivated. Please contact an administrator.", Color.Red);
                    return;
                }

                // Task 4.2 Requirement: Password matches (compare hash)
                if (!user.VerifyPassword(password))
                {
                    ShowMessage("Invalid username or password.", Color.Red);
                    
                    // Update failed login attempts
                    await System.Threading.Tasks.Task.Run(() => 
                        _authService.RecordFailedLoginAttempt(user.Id));
                    
                    return;
                }

                // Check if account is locked
                if (user.IsLocked)
                {
                    ShowMessage($"Account is locked until {user.LockedUntil:yyyy-MM-dd HH:mm:ss}. Please try again later.", Color.Red);
                    return;
                }

                // Login successful - update last login
                await System.Threading.Tasks.Task.Run(() => 
                    _authService.RecordSuccessfulLogin(user.Id));

                // Set authenticated user
                _authenticatedUser = user;
                LoginSuccessful = true;

                ShowMessage("Login successful! Redirecting...", Color.Green);

                // Task 4.2 Requirement: Check role for redirect
                await System.Threading.Tasks.Task.Delay(1000); // Brief delay to show success message

                // Role-based redirection
                RedirectBasedOnRole(user);

            }
            catch (Exception ex)
            {
                ShowMessage($"Login error: {ex.Message}", Color.Red);
            }
            finally
            {
                SetControlsEnabled(true);
            }
        }

        /// <summary>
        /// Task 4.2 Requirement: Role-based redirection logic
        /// </summary>
        /// <param name="user">Authenticated user</param>
        private void RedirectBasedOnRole(User user)
        {
            try
            {
                if (user.IsAdmin)
                {
                    // Admin user - redirect to main admin form
                    ShowMessage("Welcome Administrator! Opening admin panel...", Color.Green);
                    
                    this.Hide();
                    var mainForm = new MainForm();
                    mainForm.FormClosed += (s, e) => this.Close();
                    mainForm.Show();
                }
                else
                {
                    // Regular user - redirect to user main form (Task 4.3)
                    ShowMessage($"Welcome {user.FullName}! Opening quadratic calculator...", Color.Green);
                    
                    this.Hide();
                    var userMainForm = new UserMainForm(user);
                    userMainForm.FormClosed += (s, e) => this.Close();
                    userMainForm.Show();
                }

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ShowMessage($"Error during redirection: {ex.Message}", Color.Red);
                this.Show(); // Show login form again if redirection fails
            }
        }

        #endregion

        #region Helper Methods

        private void ShowMessage(string message, Color color)
        {
            lblMessage.Text = message;
            lblMessage.ForeColor = color;
        }

        private void ClearMessage()
        {
            lblMessage.Text = "";
        }

        private void SetControlsEnabled(bool enabled)
        {
            txtUsername.Enabled = enabled;
            txtPassword.Enabled = enabled;
            btnLogin.Enabled = enabled && !string.IsNullOrWhiteSpace(txtUsername.Text) && !string.IsNullOrWhiteSpace(txtPassword.Text);
            btnRegister.Enabled = enabled;
            chkShowPassword.Enabled = enabled;
            chkRememberMe.Enabled = enabled;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Shows the login form as a dialog and returns the authenticated user
        /// </summary>
        /// <param name="parent">Parent window</param>
        /// <returns>Authenticated user if login successful, null otherwise</returns>
        public static User? ShowLoginDialog(IWin32Window? parent = null)
        {
            using (var loginForm = new LoginFormTask42())
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
        /// Shows the login form and handles role-based application startup
        /// </summary>
        /// <returns>True if login successful and application should continue</returns>
        public static bool ShowLoginAndStartApplication()
        {
            using (var loginForm = new LoginFormTask42())
            {
                var result = loginForm.ShowDialog();
                return result == DialogResult.OK && loginForm.LoginSuccessful;
            }
        }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            // Test database connection
            if (!_authService.TestConnection())
            {
                ShowMessage("Warning: Cannot connect to database. Please check your connection.", Color.Orange);
            }
        }
    }

    /// <summary>
    /// Simple user dashboard form for regular users
    /// </summary>
    public partial class UserDashboardForm : Form
    {
        private readonly User _user;

        public UserDashboardForm(User user)
        {
            _user = user;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = $"User Dashboard - Welcome {_user.FullName}";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Welcome message
            var lblWelcome = new Label
            {
                Text = $"Welcome, {_user.FullName}!",
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(50, 50),
                Size = new Size(400, 30),
                ForeColor = Color.DarkBlue
            };
            this.Controls.Add(lblWelcome);

            // User info
            var lblUserInfo = new Label
            {
                Text = $"Email: {_user.Email}\nRole: {_user.Role}\nMember since: {_user.DateCreated:yyyy-MM-dd}",
                Font = new Font("Arial", 10),
                Location = new Point(50, 100),
                Size = new Size(400, 60)
            };
            this.Controls.Add(lblUserInfo);

            // Quadratic calculator button
            var btnQuadratic = new Button
            {
                Text = "Quadratic Calculator",
                Location = new Point(50, 200),
                Size = new Size(150, 40),
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightBlue
            };
            btnQuadratic.Click += (s, e) => {
                var quadraticForm = new QuadraticResultForm();
                quadraticForm.Show();
            };
            this.Controls.Add(btnQuadratic);

            // Profile button
            var btnProfile = new Button
            {
                Text = "My Profile",
                Location = new Point(220, 200),
                Size = new Size(150, 40),
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightGreen
            };
            btnProfile.Click += (s, e) => {
                MessageBox.Show("Profile management feature coming soon!", "Profile", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            this.Controls.Add(btnProfile);

            // Logout button
            var btnLogout = new Button
            {
                Text = "Logout",
                Location = new Point(50, 500),
                Size = new Size(100, 35),
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightCoral
            };
            btnLogout.Click += (s, e) => {
                this.Close();
            };
            this.Controls.Add(btnLogout);
        }
    }
}