using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UserCRUD.DAL;
using UserCRUD.Models;

namespace UserCRUD.Forms
{
    public partial class QuadraticResultForm : Form
    {
        private readonly QuadraticResultDAL _quadraticResultDAL;
        private readonly UserDAL _userDAL;
        private QuadraticResult? _selectedResult;

        // Controls
        private DataGridView dgvResults;
        private TextBox txtA;
        private TextBox txtB;
        private TextBox txtC;
        private TextBox txtNotes;
        private TextBox txtSearch;
        private ComboBox cmbUser;
        private Label lblEquation;
        private Label lblRoots;
        private Label lblDiscriminant;
        private Button btnCalculate;
        private Button btnSave;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClear;
        private Button btnSearch;
        private Button btnRefresh;
        private Button btnStatistics;
        private Label lblStatus;

        public QuadraticResultForm()
        {
            _quadraticResultDAL = new QuadraticResultDAL();
            _userDAL = new UserDAL();
            InitializeComponent();
            LoadUsers();
            LoadResults();
        }

        private void InitializeComponent()
        {
            this.Text = "Quadratic Equation Calculator & Results";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create main layout
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));

            // Left panel - DataGridView
            var leftPanel = CreateLeftPanel();
            mainPanel.Controls.Add(leftPanel, 0, 0);

            // Right panel - Calculator and form controls
            var rightPanel = CreateRightPanel();
            mainPanel.Controls.Add(rightPanel, 1, 0);

            this.Controls.Add(mainPanel);

            // Status bar
            lblStatus = new Label
            {
                Text = "Ready",
                Dock = DockStyle.Bottom,
                BackColor = Color.LightGray,
                Padding = new Padding(5),
                Height = 25
            };
            this.Controls.Add(lblStatus);
        }

        private Panel CreateLeftPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill };

            // Search controls
            var searchPanel = new Panel { Height = 40, Dock = DockStyle.Top };
            
            txtSearch = new TextBox
            {
                Location = new Point(10, 10),
                Width = 200,
                PlaceholderText = "Search results..."
            };
            
            btnSearch = new Button
            {
                Text = "Search",
                Location = new Point(220, 8),
                Width = 70,
                Height = 25
            };
            btnSearch.Click += BtnSearch_Click;

            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(300, 8),
                Width = 70,
                Height = 25
            };
            btnRefresh.Click += BtnRefresh_Click;

            btnStatistics = new Button
            {
                Text = "Statistics",
                Location = new Point(380, 8),
                Width = 80,
                Height = 25
            };
            btnStatistics.Click += BtnStatistics_Click;

            searchPanel.Controls.AddRange(new Control[] { txtSearch, btnSearch, btnRefresh, btnStatistics });
            panel.Controls.Add(searchPanel);

            // DataGridView
            dgvResults = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false
            };

            // Configure columns
            dgvResults.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "Id", Width = 50 },
                new DataGridViewTextBoxColumn { Name = "A", HeaderText = "A", DataPropertyName = "A", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "B", HeaderText = "B", DataPropertyName = "B", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "C", HeaderText = "C", DataPropertyName = "C", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "Discriminant", HeaderText = "Δ", DataPropertyName = "Discriminant", Width = 80 },
                new DataGridViewCheckBoxColumn { Name = "HasRealRoots", HeaderText = "Real", DataPropertyName = "HasRealRoots", Width = 50 },
                new DataGridViewTextBoxColumn { Name = "UserName", HeaderText = "User", DataPropertyName = "UserName", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "CalculatedDate", HeaderText = "Date", DataPropertyName = "CalculatedDate", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "Notes", HeaderText = "Notes", DataPropertyName = "Notes", Width = 150 }
            });

            dgvResults.SelectionChanged += DgvResults_SelectionChanged;
            panel.Controls.Add(dgvResults);

            return panel;
        }

        private Panel CreateRightPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            var y = 20;
            const int labelWidth = 80;
            const int textBoxWidth = 150;
            const int spacing = 35;

            // Title
            var titleLabel = new Label
            {
                Text = "Quadratic Equation Calculator",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Location = new Point(10, y),
                Width = 300
            };
            panel.Controls.Add(titleLabel);
            y += 30;

            // Coefficient A
            panel.Controls.Add(new Label { Text = "Coefficient A:", Location = new Point(10, y), Width = labelWidth });
            txtA = new TextBox { Location = new Point(100, y), Width = textBoxWidth, Text = "1" };
            txtA.TextChanged += CoefficientsChanged;
            panel.Controls.Add(txtA);
            y += spacing;

            // Coefficient B
            panel.Controls.Add(new Label { Text = "Coefficient B:", Location = new Point(10, y), Width = labelWidth });
            txtB = new TextBox { Location = new Point(100, y), Width = textBoxWidth, Text = "0" };
            txtB.TextChanged += CoefficientsChanged;
            panel.Controls.Add(txtB);
            y += spacing;

            // Coefficient C
            panel.Controls.Add(new Label { Text = "Coefficient C:", Location = new Point(10, y), Width = labelWidth });
            txtC = new TextBox { Location = new Point(100, y), Width = textBoxWidth, Text = "0" };
            txtC.TextChanged += CoefficientsChanged;
            panel.Controls.Add(txtC);
            y += spacing;

            // User selection
            panel.Controls.Add(new Label { Text = "User:", Location = new Point(10, y), Width = labelWidth });
            cmbUser = new ComboBox 
            { 
                Location = new Point(100, y), 
                Width = textBoxWidth,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            panel.Controls.Add(cmbUser);
            y += spacing;

            // Notes
            panel.Controls.Add(new Label { Text = "Notes:", Location = new Point(10, y), Width = labelWidth });
            txtNotes = new TextBox 
            { 
                Location = new Point(100, y), 
                Width = textBoxWidth,
                Multiline = true,
                Height = 60
            };
            panel.Controls.Add(txtNotes);
            y += 70;

            // Calculate button
            btnCalculate = new Button
            {
                Text = "Calculate",
                Location = new Point(10, y),
                Width = 100,
                Height = 30,
                BackColor = Color.LightBlue
            };
            btnCalculate.Click += BtnCalculate_Click;
            panel.Controls.Add(btnCalculate);
            y += 40;

            // Results display
            panel.Controls.Add(new Label { Text = "Equation:", Location = new Point(10, y), Width = labelWidth });
            lblEquation = new Label 
            { 
                Location = new Point(100, y), 
                Width = textBoxWidth,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightYellow
            };
            panel.Controls.Add(lblEquation);
            y += spacing;

            panel.Controls.Add(new Label { Text = "Discriminant:", Location = new Point(10, y), Width = labelWidth });
            lblDiscriminant = new Label 
            { 
                Location = new Point(100, y), 
                Width = textBoxWidth,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightYellow
            };
            panel.Controls.Add(lblDiscriminant);
            y += spacing;

            panel.Controls.Add(new Label { Text = "Roots:", Location = new Point(10, y), Width = labelWidth });
            lblRoots = new Label 
            { 
                Location = new Point(100, y), 
                Width = textBoxWidth,
                Height = 40,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightYellow
            };
            panel.Controls.Add(lblRoots);
            y += 50;

            // Action buttons
            const int buttonWidth = 80;
            const int buttonHeight = 30;

            btnSave = new Button
            {
                Text = "Save",
                Location = new Point(10, y),
                Width = buttonWidth,
                Height = buttonHeight,
                BackColor = Color.LightGreen,
                Enabled = false
            };
            btnSave.Click += BtnSave_Click;

            btnUpdate = new Button
            {
                Text = "Update",
                Location = new Point(100, y),
                Width = buttonWidth,
                Height = buttonHeight,
                BackColor = Color.LightBlue,
                Enabled = false
            };
            btnUpdate.Click += BtnUpdate_Click;

            btnDelete = new Button
            {
                Text = "Delete",
                Location = new Point(190, y),
                Width = buttonWidth,
                Height = buttonHeight,
                BackColor = Color.LightCoral,
                Enabled = false
            };
            btnDelete.Click += BtnDelete_Click;

            y += buttonHeight + 10;

            btnClear = new Button
            {
                Text = "Clear",
                Location = new Point(10, y),
                Width = buttonWidth,
                Height = buttonHeight
            };
            btnClear.Click += BtnClear_Click;

            panel.Controls.AddRange(new Control[] { btnSave, btnUpdate, btnDelete, btnClear });

            return panel;
        }

        private void LoadUsers()
        {
            try
            {
                var users = _userDAL.GetAllUsers();
                cmbUser.Items.Clear();
                cmbUser.Items.Add(new { Id = (int?)null, Name = "-- No User --" });
                
                foreach (var user in users.Where(u => u.IsActive))
                {
                    cmbUser.Items.Add(new { Id = (int?)user.Id, Name = user.FullName });
                }
                
                cmbUser.DisplayMember = "Name";
                cmbUser.ValueMember = "Id";
                cmbUser.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadResults()
        {
            try
            {
                var results = _quadraticResultDAL.GetAllQuadraticResults();
                dgvResults.DataSource = results;
                UpdateStatus($"Loaded {results.Count} results");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading results: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Error loading results");
            }
        }

        private void CoefficientsChanged(object? sender, EventArgs e)
        {
            if (double.TryParse(txtA.Text, out double a) &&
                double.TryParse(txtB.Text, out double b) &&
                double.TryParse(txtC.Text, out double c))
            {
                var tempResult = new QuadraticResult(a, b, c);
                lblEquation.Text = tempResult.GetEquationString();
                lblDiscriminant.Text = tempResult.Discriminant.ToString("F4");
                lblRoots.Text = tempResult.GetRootsString();
                btnSave.Enabled = true;
            }
            else
            {
                lblEquation.Text = "";
                lblDiscriminant.Text = "";
                lblRoots.Text = "";
                btnSave.Enabled = false;
            }
        }

        private void DgvResults_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvResults.SelectedRows.Count > 0)
            {
                _selectedResult = dgvResults.SelectedRows[0].DataBoundItem as QuadraticResult;
                if (_selectedResult != null)
                {
                    PopulateForm(_selectedResult);
                    btnUpdate.Enabled = true;
                    btnDelete.Enabled = true;
                }
            }
            else
            {
                _selectedResult = null;
                btnUpdate.Enabled = false;
                btnDelete.Enabled = false;
            }
        }

        private void PopulateForm(QuadraticResult result)
        {
            txtA.Text = result.A.ToString();
            txtB.Text = result.B.ToString();
            txtC.Text = result.C.ToString();
            txtNotes.Text = result.Notes ?? "";

            // Select user in combo box
            if (result.UserId.HasValue)
            {
                for (int i = 0; i < cmbUser.Items.Count; i++)
                {
                    dynamic item = cmbUser.Items[i];
                    if (item.Id == result.UserId)
                    {
                        cmbUser.SelectedIndex = i;
                        break;
                    }
                }
            }
            else
            {
                cmbUser.SelectedIndex = 0;
            }

            lblEquation.Text = result.GetEquationString();
            lblDiscriminant.Text = result.Discriminant.ToString("F4");
            lblRoots.Text = result.GetRootsString();
        }

        private void ClearForm()
        {
            txtA.Text = "1";
            txtB.Text = "0";
            txtC.Text = "0";
            txtNotes.Clear();
            cmbUser.SelectedIndex = 0;
            lblEquation.Text = "";
            lblDiscriminant.Text = "";
            lblRoots.Text = "";
            _selectedResult = null;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
            btnSave.Enabled = false;
        }

        private bool ValidateForm()
        {
            if (!double.TryParse(txtA.Text, out _))
            {
                MessageBox.Show("Coefficient A must be a valid number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtA.Focus();
                return false;
            }

            if (!double.TryParse(txtB.Text, out _))
            {
                MessageBox.Show("Coefficient B must be a valid number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtB.Focus();
                return false;
            }

            if (!double.TryParse(txtC.Text, out _))
            {
                MessageBox.Show("Coefficient C must be a valid number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtC.Focus();
                return false;
            }

            return true;
        }

        private QuadraticResult CreateResultFromForm()
        {
            var a = double.Parse(txtA.Text);
            var b = double.Parse(txtB.Text);
            var c = double.Parse(txtC.Text);
            
            dynamic selectedUser = cmbUser.SelectedItem;
            var userId = selectedUser?.Id;
            
            return new QuadraticResult(a, b, c, userId, txtNotes.Text);
        }

        private void BtnCalculate_Click(object? sender, EventArgs e)
        {
            if (!ValidateForm()) return;

            var result = CreateResultFromForm();
            lblEquation.Text = result.GetEquationString();
            lblDiscriminant.Text = result.Discriminant.ToString("F4");
            lblRoots.Text = result.GetRootsString();
            btnSave.Enabled = true;
            UpdateStatus("Calculation completed");
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (!ValidateForm()) return;

            try
            {
                var result = CreateResultFromForm();
                int newId = _quadraticResultDAL.CreateQuadraticResult(result);
                
                if (newId > 0)
                {
                    MessageBox.Show($"Result saved successfully with ID: {newId}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadResults();
                    UpdateStatus($"Result saved with ID: {newId}");
                }
                else
                {
                    MessageBox.Show("Failed to save result.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    UpdateStatus("Failed to save result");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving result: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Error saving result");
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (_selectedResult == null || !ValidateForm()) return;

            try
            {
                _selectedResult.A = double.Parse(txtA.Text);
                _selectedResult.B = double.Parse(txtB.Text);
                _selectedResult.C = double.Parse(txtC.Text);
                _selectedResult.Notes = txtNotes.Text;
                
                dynamic selectedUser = cmbUser.SelectedItem;
                _selectedResult.UserId = selectedUser?.Id;

                bool success = _quadraticResultDAL.UpdateQuadraticResult(_selectedResult);
                
                if (success)
                {
                    MessageBox.Show("Result updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadResults();
                    UpdateStatus($"Result ID {_selectedResult.Id} updated");
                }
                else
                {
                    MessageBox.Show("Failed to update result.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    UpdateStatus("Failed to update result");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating result: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Error updating result");
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_selectedResult == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete this quadratic result?\nEquation: {_selectedResult.GetEquationString()}",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    bool success = _quadraticResultDAL.DeleteQuadraticResult(_selectedResult.Id);
                    
                    if (success)
                    {
                        MessageBox.Show("Result deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearForm();
                        LoadResults();
                        UpdateStatus($"Result ID {_selectedResult.Id} deleted");
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete result.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        UpdateStatus("Failed to delete result");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting result: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    UpdateStatus("Error deleting result");
                }
            }
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            ClearForm();
            UpdateStatus("Form cleared");
        }

        private void BtnSearch_Click(object? sender, EventArgs e)
        {
            try
            {
                var searchTerm = txtSearch.Text.Trim();
                var results = string.IsNullOrWhiteSpace(searchTerm) 
                    ? _quadraticResultDAL.GetAllQuadraticResults() 
                    : _quadraticResultDAL.SearchQuadraticResults(searchTerm);
                
                dgvResults.DataSource = results;
                UpdateStatus($"Found {results.Count} results");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching results: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Error searching results");
            }
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            LoadResults();
            txtSearch.Clear();
        }

        private void BtnStatistics_Click(object? sender, EventArgs e)
        {
            try
            {
                var stats = _quadraticResultDAL.GetQuadraticResultsStatistics();
                var message = $"Quadratic Results Statistics:\n\n" +
                             $"Total Results: {stats["TotalResults"]}\n" +
                             $"Results with Real Roots: {stats["ResultsWithRealRoots"]}\n" +
                             $"Results with Complex Roots: {stats["ResultsWithComplexRoots"]}\n" +
                             $"Average Discriminant: {stats["AverageDiscriminant"]:F4}\n";
                
                if (stats["EarliestCalculation"] != null)
                    message += $"Earliest Calculation: {stats["EarliestCalculation"]}\n";
                if (stats["LatestCalculation"] != null)
                    message += $"Latest Calculation: {stats["LatestCalculation"]}\n";

                MessageBox.Show(message, "Statistics", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving statistics: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateStatus(string message)
        {
            lblStatus.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            // Test database connection
            if (!_quadraticResultDAL.TestConnection())
            {
                MessageBox.Show(
                    "Warning: Could not connect to the database. Please check your connection string in App.config.",
                    "Database Connection Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                UpdateStatus("Database connection failed");
            }
            else
            {
                UpdateStatus("Database connected successfully");
            }

            // Initialize the form
            CoefficientsChanged(null, EventArgs.Empty);
        }
    }
}