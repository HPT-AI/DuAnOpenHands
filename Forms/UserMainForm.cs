using System;
using System.Drawing;
using System.Windows.Forms;
using UserCRUD.Models;
using UserCRUD.DAL;
using UserCRUD.Services;

namespace UserCRUD.Forms
{
    /// <summary>
    /// Main screen for User - Task 4.3 Implementation
    /// Provides quadratic equation solving functionality for regular users
    /// </summary>
    public partial class UserMainForm : Form
    {
        private readonly User _currentUser;
        private readonly QuadraticResultDAL _quadraticDAL;
        private readonly AuthorizationService _authService;

        // Task 4.3 Required Controls
        private TextBox txtA;           // Field: a (coefficient of x²)
        private TextBox txtB;           // Field: b (coefficient of x)
        private TextBox txtC;           // Field: c (constant term)
        private Button btnSolve;        // Button: Solve
        private Button btnSave;         // Button: Save
        private Button btnClear;        // Button: Clear
        private Label lblResult;        // Label: Display result

        // Additional UI Controls
        private Label lblTitle;
        private Label lblA;
        private Label lblB;
        private Label lblC;
        private Label lblEquation;
        private Label lblWelcome;
        private Label lblStatus;
        private GroupBox grpInputs;
        private GroupBox grpResults;
        private GroupBox grpActions;
        private Panel pnlMain;
        private MenuStrip menuStrip;
        private DataGridView dgvHistory;
        private Button btnViewHistory;
        private Button btnLogout;
        private Button btnProfile;

        // Properties
        public QuadraticResult? CurrentResult { get; private set; }
        public bool HasUnsavedResult { get; private set; }

        public UserMainForm(User user)
        {
            _currentUser = user ?? throw new ArgumentNullException(nameof(user));
            _quadraticDAL = new QuadraticResultDAL();
            _authService = new AuthorizationService();
            
            InitializeComponent();
            LoadUserHistory();
        }

        private void InitializeComponent()
        {
            // Form properties
            this.Text = $"Quadratic Calculator - Welcome {_currentUser.FirstName}";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 248, 255); // Light blue background
            this.MinimumSize = new Size(800, 600);

            // Create menu strip
            CreateMenuStrip();

            // Main panel
            pnlMain = new Panel
            {
                Location = new Point(20, 50),
                Size = new Size(840, 600),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(pnlMain);

            // Welcome label
            lblWelcome = new Label
            {
                Text = $"Welcome, {_currentUser.FullName}!",
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(20, 20),
                Size = new Size(400, 30)
            };
            pnlMain.Controls.Add(lblWelcome);

            // Title
            lblTitle = new Label
            {
                Text = "Quadratic Equation Solver",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                Location = new Point(20, 60),
                Size = new Size(300, 25)
            };
            pnlMain.Controls.Add(lblTitle);

            // Equation display
            lblEquation = new Label
            {
                Text = "ax² + bx + c = 0",
                Font = new Font("Arial", 12, FontStyle.Italic),
                ForeColor = Color.Gray,
                Location = new Point(20, 90),
                Size = new Size(200, 20)
            };
            pnlMain.Controls.Add(lblEquation);

            // Input group
            CreateInputGroup();

            // Results group
            CreateResultsGroup();

            // Actions group
            CreateActionsGroup();

            // History section
            CreateHistorySection();

            // Status bar
            CreateStatusBar();

            // Set tab order
            SetTabOrder();

            // Set default button
            this.AcceptButton = btnSolve;

            // Focus on first input
            this.Load += (s, e) => txtA.Focus();
        }

        #region UI Creation Methods

        private void CreateMenuStrip()
        {
            menuStrip = new MenuStrip();

            // File menu
            var fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add("New Calculation", null, (s, e) => BtnClear_Click(s, e));
            fileMenu.DropDownItems.Add("Save Result", null, (s, e) => BtnSave_Click(s, e));
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Exit", null, (s, e) => this.Close());

            // View menu
            var viewMenu = new ToolStripMenuItem("View");
            viewMenu.DropDownItems.Add("My History", null, (s, e) => BtnViewHistory_Click(s, e));
            viewMenu.DropDownItems.Add("Refresh", null, (s, e) => LoadUserHistory());

            // Help menu
            var helpMenu = new ToolStripMenuItem("Help");
            helpMenu.DropDownItems.Add("About Quadratic Equations", null, ShowQuadraticHelp);
            helpMenu.DropDownItems.Add("How to Use", null, ShowUsageHelp);

            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, viewMenu, helpMenu });
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        private void CreateInputGroup()
        {
            // Input group box
            grpInputs = new GroupBox
            {
                Text = "Equation Coefficients",
                Location = new Point(20, 120),
                Size = new Size(400, 120),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            pnlMain.Controls.Add(grpInputs);

            // Coefficient A
            lblA = new Label
            {
                Text = "a (coefficient of x²):",
                Location = new Point(20, 30),
                Size = new Size(120, 20),
                Font = new Font("Arial", 9)
            };
            grpInputs.Controls.Add(lblA);

            txtA = new TextBox
            {
                Location = new Point(150, 28),
                Size = new Size(100, 25),
                Font = new Font("Arial", 10),
                PlaceholderText = "Enter a"
            };
            txtA.KeyPress += TxtCoefficient_KeyPress;
            txtA.TextChanged += TxtCoefficient_TextChanged;
            grpInputs.Controls.Add(txtA);

            // Coefficient B
            lblB = new Label
            {
                Text = "b (coefficient of x):",
                Location = new Point(20, 60),
                Size = new Size(120, 20),
                Font = new Font("Arial", 9)
            };
            grpInputs.Controls.Add(lblB);

            txtB = new TextBox
            {
                Location = new Point(150, 58),
                Size = new Size(100, 25),
                Font = new Font("Arial", 10),
                PlaceholderText = "Enter b"
            };
            txtB.KeyPress += TxtCoefficient_KeyPress;
            txtB.TextChanged += TxtCoefficient_TextChanged;
            grpInputs.Controls.Add(txtB);

            // Coefficient C
            lblC = new Label
            {
                Text = "c (constant term):",
                Location = new Point(20, 90),
                Size = new Size(120, 20),
                Font = new Font("Arial", 9)
            };
            grpInputs.Controls.Add(lblC);

            txtC = new TextBox
            {
                Location = new Point(150, 88),
                Size = new Size(100, 25),
                Font = new Font("Arial", 10),
                PlaceholderText = "Enter c"
            };
            txtC.KeyPress += TxtCoefficient_KeyPress;
            txtC.TextChanged += TxtCoefficient_TextChanged;
            grpInputs.Controls.Add(txtC);

            // Example values button
            var btnExample = new Button
            {
                Text = "Example",
                Location = new Point(270, 30),
                Size = new Size(70, 25),
                Font = new Font("Arial", 8),
                BackColor = Color.LightYellow
            };
            btnExample.Click += BtnExample_Click;
            grpInputs.Controls.Add(btnExample);

            // Validation info
            var lblValidation = new Label
            {
                Text = "Note: 'a' cannot be zero for quadratic equations",
                Location = new Point(270, 60),
                Size = new Size(120, 40),
                Font = new Font("Arial", 8),
                ForeColor = Color.Gray
            };
            grpInputs.Controls.Add(lblValidation);
        }

        private void CreateResultsGroup()
        {
            // Results group box
            grpResults = new GroupBox
            {
                Text = "Solution",
                Location = new Point(440, 120),
                Size = new Size(380, 120),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            pnlMain.Controls.Add(grpResults);

            // Result display label (Task 4.3 requirement)
            lblResult = new Label
            {
                Text = "Enter coefficients and click Solve to see the result",
                Location = new Point(20, 30),
                Size = new Size(340, 80),
                Font = new Font("Arial", 10),
                ForeColor = Color.DarkBlue,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 248, 255),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10)
            };
            grpResults.Controls.Add(lblResult);
        }

        private void CreateActionsGroup()
        {
            // Actions group box
            grpActions = new GroupBox
            {
                Text = "Actions",
                Location = new Point(20, 260),
                Size = new Size(800, 80),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            pnlMain.Controls.Add(grpActions);

            // Solve button (Task 4.3 requirement)
            btnSolve = new Button
            {
                Text = "Solve",
                Location = new Point(30, 30),
                Size = new Size(100, 35),
                Font = new Font("Arial", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 123, 255), // Blue
                ForeColor = Color.White,
                UseVisualStyleBackColor = false
            };
            btnSolve.Click += BtnSolve_Click;
            grpActions.Controls.Add(btnSolve);

            // Save button (Task 4.3 requirement)
            btnSave = new Button
            {
                Text = "Save",
                Location = new Point(150, 30),
                Size = new Size(100, 35),
                Font = new Font("Arial", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(40, 167, 69), // Green
                ForeColor = Color.White,
                UseVisualStyleBackColor = false,
                Enabled = false
            };
            btnSave.Click += BtnSave_Click;
            grpActions.Controls.Add(btnSave);

            // Clear button (Task 4.3 requirement)
            btnClear = new Button
            {
                Text = "Clear",
                Location = new Point(270, 30),
                Size = new Size(100, 35),
                Font = new Font("Arial", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(108, 117, 125), // Gray
                ForeColor = Color.White,
                UseVisualStyleBackColor = false
            };
            btnClear.Click += BtnClear_Click;
            grpActions.Controls.Add(btnClear);

            // Additional buttons
            btnViewHistory = new Button
            {
                Text = "View History",
                Location = new Point(400, 30),
                Size = new Size(100, 35),
                Font = new Font("Arial", 10),
                BackColor = Color.FromArgb(255, 193, 7), // Yellow
                ForeColor = Color.Black,
                UseVisualStyleBackColor = false
            };
            btnViewHistory.Click += BtnViewHistory_Click;
            grpActions.Controls.Add(btnViewHistory);

            btnProfile = new Button
            {
                Text = "My Profile",
                Location = new Point(520, 30),
                Size = new Size(100, 35),
                Font = new Font("Arial", 10),
                BackColor = Color.FromArgb(23, 162, 184), // Cyan
                ForeColor = Color.White,
                UseVisualStyleBackColor = false
            };
            btnProfile.Click += BtnProfile_Click;
            grpActions.Controls.Add(btnProfile);

            btnLogout = new Button
            {
                Text = "Logout",
                Location = new Point(640, 30),
                Size = new Size(100, 35),
                Font = new Font("Arial", 10),
                BackColor = Color.FromArgb(220, 53, 69), // Red
                ForeColor = Color.White,
                UseVisualStyleBackColor = false
            };
            btnLogout.Click += BtnLogout_Click;
            grpActions.Controls.Add(btnLogout);
        }

        private void CreateHistorySection()
        {
            // History label
            var lblHistory = new Label
            {
                Text = "My Calculation History",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Location = new Point(20, 360),
                Size = new Size(200, 25),
                ForeColor = Color.DarkBlue
            };
            pnlMain.Controls.Add(lblHistory);

            // History grid
            dgvHistory = new DataGridView
            {
                Location = new Point(20, 390),
                Size = new Size(800, 150),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvHistory.DoubleClick += DgvHistory_DoubleClick;
            pnlMain.Controls.Add(dgvHistory);
        }

        private void CreateStatusBar()
        {
            lblStatus = new Label
            {
                Location = new Point(20, 560),
                Size = new Size(800, 20),
                Font = new Font("Arial", 9),
                ForeColor = Color.Gray,
                Text = "Ready - Enter coefficients and click Solve"
            };
            pnlMain.Controls.Add(lblStatus);
        }

        private void SetTabOrder()
        {
            txtA.TabIndex = 0;
            txtB.TabIndex = 1;
            txtC.TabIndex = 2;
            btnSolve.TabIndex = 3;
            btnSave.TabIndex = 4;
            btnClear.TabIndex = 5;
            btnViewHistory.TabIndex = 6;
            btnProfile.TabIndex = 7;
            btnLogout.TabIndex = 8;
        }

        #endregion

        #region Event Handlers

        private void TxtCoefficient_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // Allow numbers, decimal point, minus sign, and control characters
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && 
                e.KeyChar != '.' && e.KeyChar != '-')
            {
                e.Handled = true;
            }

            // Allow only one decimal point
            if (e.KeyChar == '.' && (sender as TextBox)?.Text.Contains('.') == true)
            {
                e.Handled = true;
            }

            // Allow minus only at the beginning
            if (e.KeyChar == '-' && (sender as TextBox)?.SelectionStart != 0)
            {
                e.Handled = true;
            }

            // Handle Enter key
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (sender == txtA) txtB.Focus();
                else if (sender == txtB) txtC.Focus();
                else if (sender == txtC) btnSolve.PerformClick();
                e.Handled = true;
            }
        }

        private void TxtCoefficient_TextChanged(object? sender, EventArgs e)
        {
            ValidateInputs();
            UpdateEquationDisplay();
        }

        // Task 4.3 Solve Button Implementation
        private void BtnSolve_Click(object? sender, EventArgs e)
        {
            PerformSolve();
        }

        // Task 4.3 Save Button Implementation
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            PerformSave();
        }

        // Task 4.3 Clear Button Implementation
        private void BtnClear_Click(object? sender, EventArgs e)
        {
            PerformClear();
        }

        private void BtnExample_Click(object? sender, EventArgs e)
        {
            // Set example values
            txtA.Text = "1";
            txtB.Text = "-5";
            txtC.Text = "6";
            UpdateStatus("Example loaded: x² - 5x + 6 = 0");
        }

        private void BtnViewHistory_Click(object? sender, EventArgs e)
        {
            LoadUserHistory();
            UpdateStatus("History refreshed");
        }

        private void BtnProfile_Click(object? sender, EventArgs e)
        {
            ShowUserProfile();
        }

        private void BtnLogout_Click(object? sender, EventArgs e)
        {
            PerformLogout();
        }

        private void DgvHistory_DoubleClick(object? sender, EventArgs e)
        {
            LoadSelectedResult();
        }

        #endregion

        #region Task 4.3 Core Logic Implementation

        /// <summary>
        /// Task 4.3 Solve Logic: Calculate roots correctly (no root, double root, two distinct roots)
        /// </summary>
        private void PerformSolve()
        {
            try
            {
                // Get coefficient values
                if (!GetCoefficients(out double a, out double b, out double c))
                {
                    return;
                }

                // Validate that 'a' is not zero (quadratic requirement)
                if (Math.Abs(a) < 1e-10)
                {
                    lblResult.Text = "Error: 'a' cannot be zero for a quadratic equation.";
                    lblResult.ForeColor = Color.Red;
                    UpdateStatus("Error: Not a quadratic equation");
                    return;
                }

                // Use the QuadraticFunction from Task 3.4
                string result = QuadraticFunction.SolveQuadraticEquation(a, b, c);

                // Create QuadraticResult object
                CurrentResult = new QuadraticResult
                {
                    A = a,
                    B = b,
                    C = c,
                    UserId = _currentUser.Id,
                    DateCreated = DateTime.Now
                };

                // Calculate and set roots
                CurrentResult.CalculateRoots();

                // Display result (Task 4.3 requirement)
                lblResult.Text = result;
                lblResult.ForeColor = Color.DarkBlue;

                // Enable save button
                btnSave.Enabled = true;
                HasUnsavedResult = true;

                // Update status
                UpdateStatus($"Solution calculated: {GetSolutionType(a, b, c)}");

            }
            catch (Exception ex)
            {
                lblResult.Text = $"Error calculating solution: {ex.Message}";
                lblResult.ForeColor = Color.Red;
                UpdateStatus("Error during calculation");
            }
        }

        /// <summary>
        /// Task 4.3 Save Logic: Save result to DB when clicking Save
        /// </summary>
        private void PerformSave()
        {
            try
            {
                if (CurrentResult == null)
                {
                    MessageBox.Show("No result to save. Please solve an equation first.", 
                        "No Result", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check authorization
                if (!_authService.CheckQuadraticResultAccess(_currentUser.Id, 0, "create").IsGranted)
                {
                    MessageBox.Show("You don't have permission to save results.", 
                        "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Save to database
                int resultId = _quadraticDAL.CreateQuadraticResult(CurrentResult);

                if (resultId > 0)
                {
                    CurrentResult.Id = resultId;
                    HasUnsavedResult = false;
                    btnSave.Enabled = false;

                    // Refresh history
                    LoadUserHistory();

                    // Show success message
                    MessageBox.Show("Result saved successfully!", 
                        "Save Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    UpdateStatus($"Result saved with ID: {resultId}");
                }
                else
                {
                    MessageBox.Show("Failed to save result. Please try again.", 
                        "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    UpdateStatus("Save failed");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving result: {ex.Message}", 
                    "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Error during save");
            }
        }

        /// <summary>
        /// Task 4.3 Clear Logic: Clear all fields and results
        /// </summary>
        private void PerformClear()
        {
            // Check for unsaved changes
            if (HasUnsavedResult)
            {
                var result = MessageBox.Show(
                    "You have an unsaved result. Do you want to save it before clearing?",
                    "Unsaved Changes",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    PerformSave();
                    if (HasUnsavedResult) return; // Save failed or cancelled
                }
                else if (result == DialogResult.Cancel)
                {
                    return; // User cancelled clear operation
                }
            }

            // Clear all fields
            txtA.Clear();
            txtB.Clear();
            txtC.Clear();

            // Clear result
            lblResult.Text = "Enter coefficients and click Solve to see the result";
            lblResult.ForeColor = Color.DarkBlue;

            // Reset state
            CurrentResult = null;
            HasUnsavedResult = false;
            btnSave.Enabled = false;

            // Update equation display
            UpdateEquationDisplay();

            // Focus on first field
            txtA.Focus();

            UpdateStatus("Form cleared - ready for new calculation");
        }

        #endregion

        #region Helper Methods

        private bool GetCoefficients(out double a, out double b, out double c)
        {
            a = b = c = 0;

            // Parse coefficient A
            if (!double.TryParse(txtA.Text.Trim(), out a))
            {
                lblResult.Text = "Error: Invalid value for coefficient 'a'";
                lblResult.ForeColor = Color.Red;
                txtA.Focus();
                UpdateStatus("Invalid input for coefficient 'a'");
                return false;
            }

            // Parse coefficient B
            if (!double.TryParse(txtB.Text.Trim(), out b))
            {
                lblResult.Text = "Error: Invalid value for coefficient 'b'";
                lblResult.ForeColor = Color.Red;
                txtB.Focus();
                UpdateStatus("Invalid input for coefficient 'b'");
                return false;
            }

            // Parse coefficient C
            if (!double.TryParse(txtC.Text.Trim(), out c))
            {
                lblResult.Text = "Error: Invalid value for coefficient 'c'";
                lblResult.ForeColor = Color.Red;
                txtC.Focus();
                UpdateStatus("Invalid input for coefficient 'c'");
                return false;
            }

            return true;
        }

        private void ValidateInputs()
        {
            bool hasA = !string.IsNullOrWhiteSpace(txtA.Text) && double.TryParse(txtA.Text, out double a) && Math.Abs(a) > 1e-10;
            bool hasB = !string.IsNullOrWhiteSpace(txtB.Text) && double.TryParse(txtB.Text, out _);
            bool hasC = !string.IsNullOrWhiteSpace(txtC.Text) && double.TryParse(txtC.Text, out _);

            btnSolve.Enabled = hasA && hasB && hasC;
        }

        private void UpdateEquationDisplay()
        {
            try
            {
                if (double.TryParse(txtA.Text, out double a) && 
                    double.TryParse(txtB.Text, out double b) && 
                    double.TryParse(txtC.Text, out double c))
                {
                    lblEquation.Text = FormatEquation(a, b, c) + " = 0";
                }
                else
                {
                    lblEquation.Text = "ax² + bx + c = 0";
                }
            }
            catch
            {
                lblEquation.Text = "ax² + bx + c = 0";
            }
        }

        private string FormatEquation(double a, double b, double c)
        {
            string equation = "";

            // x² term
            if (Math.Abs(a - 1) < 1e-10)
                equation = "x²";
            else if (Math.Abs(a + 1) < 1e-10)
                equation = "-x²";
            else
                equation = $"{a}x²";

            // x term
            if (Math.Abs(b) > 1e-10)
            {
                if (b > 0)
                    equation += " + ";
                else
                    equation += " - ";

                double absB = Math.Abs(b);
                if (Math.Abs(absB - 1) < 1e-10)
                    equation += "x";
                else
                    equation += $"{absB}x";
            }

            // Constant term
            if (Math.Abs(c) > 1e-10)
            {
                if (c > 0)
                    equation += " + ";
                else
                    equation += " - ";

                equation += Math.Abs(c).ToString();
            }

            return equation;
        }

        private string GetSolutionType(double a, double b, double c)
        {
            double discriminant = b * b - 4 * a * c;

            if (discriminant > 0)
                return "Two distinct real roots";
            else if (Math.Abs(discriminant) < 1e-10)
                return "One repeated root";
            else
                return "Two complex roots";
        }

        private void LoadUserHistory()
        {
            try
            {
                var history = _quadraticDAL.GetQuadraticResultsByUserId(_currentUser.Id);
                
                dgvHistory.DataSource = history.Select(r => new
                {
                    ID = r.Id,
                    A = r.A,
                    B = r.B,
                    C = r.C,
                    Root1 = r.Root1?.ToString("F4") ?? "N/A",
                    Root2 = r.Root2?.ToString("F4") ?? "N/A",
                    Date = r.DateCreated.ToString("yyyy-MM-dd HH:mm")
                }).ToList();

                // Configure columns
                if (dgvHistory.Columns.Count > 0)
                {
                    dgvHistory.Columns["ID"].Width = 50;
                    dgvHistory.Columns["A"].Width = 60;
                    dgvHistory.Columns["B"].Width = 60;
                    dgvHistory.Columns["C"].Width = 60;
                    dgvHistory.Columns["Root1"].Width = 100;
                    dgvHistory.Columns["Root2"].Width = 100;
                    dgvHistory.Columns["Date"].Width = 120;
                }

                UpdateStatus($"Loaded {history.Count} calculation(s) from history");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error loading history: {ex.Message}");
            }
        }

        private void LoadSelectedResult()
        {
            try
            {
                if (dgvHistory.SelectedRows.Count > 0)
                {
                    var row = dgvHistory.SelectedRows[0];
                    txtA.Text = row.Cells["A"].Value?.ToString() ?? "";
                    txtB.Text = row.Cells["B"].Value?.ToString() ?? "";
                    txtC.Text = row.Cells["C"].Value?.ToString() ?? "";

                    // Automatically solve
                    PerformSolve();
                    UpdateStatus("Loaded calculation from history");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error loading selected result: {ex.Message}");
            }
        }

        private void ShowUserProfile()
        {
            var profileInfo = $"User Profile\n\n" +
                             $"Name: {_currentUser.FullName}\n" +
                             $"Email: {_currentUser.Email}\n" +
                             $"Phone: {_currentUser.Phone}\n" +
                             $"Role: {_currentUser.Role}\n" +
                             $"Member since: {_currentUser.DateCreated:yyyy-MM-dd}\n" +
                             $"Last login: {_currentUser.LastLoginDate?.ToString("yyyy-MM-dd HH:mm") ?? "Never"}";

            MessageBox.Show(profileInfo, "My Profile", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void PerformLogout()
        {
            // Check for unsaved changes
            if (HasUnsavedResult)
            {
                var result = MessageBox.Show(
                    "You have an unsaved result. Do you want to save it before logging out?",
                    "Unsaved Changes",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    PerformSave();
                    if (HasUnsavedResult) return; // Save failed or cancelled
                }
                else if (result == DialogResult.Cancel)
                {
                    return; // User cancelled logout
                }
            }

            // Confirm logout
            var confirmResult = MessageBox.Show(
                "Are you sure you want to logout?",
                "Confirm Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void UpdateStatus(string message)
        {
            lblStatus.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
        }

        #endregion

        #region Menu Event Handlers

        private void ShowQuadraticHelp(object? sender, EventArgs e)
        {
            var helpText = @"About Quadratic Equations

A quadratic equation is a polynomial equation of degree 2, in the form:
ax² + bx + c = 0

Where:
• a, b, c are coefficients (a ≠ 0)
• x is the variable

Types of Solutions:
• Two distinct real roots: When discriminant > 0
• One repeated root: When discriminant = 0  
• Two complex roots: When discriminant < 0

Discriminant = b² - 4ac

Examples:
• x² - 5x + 6 = 0 → roots: x = 2, x = 3
• x² - 4x + 4 = 0 → root: x = 2 (repeated)
• x² + x + 1 = 0 → complex roots";

            MessageBox.Show(helpText, "About Quadratic Equations", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowUsageHelp(object? sender, EventArgs e)
        {
            var helpText = @"How to Use the Quadratic Calculator

1. Enter Coefficients:
   • Enter value for 'a' (cannot be zero)
   • Enter value for 'b' 
   • Enter value for 'c'

2. Solve Equation:
   • Click 'Solve' button
   • View the result in the solution area

3. Save Result:
   • Click 'Save' to store the result in database
   • View your saved calculations in the history

4. Clear Form:
   • Click 'Clear' to reset all fields
   • Start a new calculation

5. View History:
   • See all your previous calculations
   • Double-click to reload a calculation

Tips:
• Use decimal numbers (e.g., 1.5, -2.3)
• Press Enter to move between fields
• Use the Example button for sample values";

            MessageBox.Show(helpText, "How to Use", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Check for unsaved changes
            if (HasUnsavedResult)
            {
                var result = MessageBox.Show(
                    "You have an unsaved result. Do you want to save it before closing?",
                    "Unsaved Changes",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    PerformSave();
                    if (HasUnsavedResult)
                    {
                        e.Cancel = true; // Cancel close if save failed
                        return;
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true; // User cancelled close
                    return;
                }
            }

            base.OnFormClosing(e);
        }

        #region Static Methods

        /// <summary>
        /// Shows the user main form for the specified user
        /// </summary>
        /// <param name="user">User to show the form for</param>
        /// <param name="parent">Parent window</param>
        /// <returns>Dialog result</returns>
        public static DialogResult ShowUserMainForm(User user, IWin32Window? parent = null)
        {
            using (var userMainForm = new UserMainForm(user))
            {
                return userMainForm.ShowDialog(parent);
            }
        }

        #endregion
    }
}