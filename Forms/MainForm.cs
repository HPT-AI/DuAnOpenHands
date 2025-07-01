using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UserCRUD.DAL;
using UserCRUD.Models;

namespace UserCRUD.Forms
{
    public partial class MainForm : Form
    {
        private readonly UserDAL _userDAL;
        private User? _selectedUser;

        // Controls
        private DataGridView dgvUsers;
        private TextBox txtFirstName;
        private TextBox txtLastName;
        private TextBox txtEmail;
        private TextBox txtPhone;
        private TextBox txtSearch;
        private CheckBox chkIsActive;
        private Button btnCreate;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClear;
        private Button btnSearch;
        private Button btnRefresh;
        private Button btnQuadraticResults;
        private Label lblStatus;

        public MainForm()
        {
            _userDAL = new UserDAL();
            InitializeComponent();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            this.Text = "User CRUD Application";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create main layout
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            // Left panel - DataGridView
            var leftPanel = CreateLeftPanel();
            mainPanel.Controls.Add(leftPanel, 0, 0);

            // Right panel - Form controls
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
                PlaceholderText = "Search users..."
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

            btnQuadraticResults = new Button
            {
                Text = "Quadratic Results",
                Location = new Point(380, 8),
                Width = 120,
                Height = 25,
                BackColor = Color.LightCyan
            };
            btnQuadraticResults.Click += BtnQuadraticResults_Click;

            var btnRegisterUser = new Button
            {
                Text = "Register User",
                Location = new Point(510, 8),
                Width = 100,
                Height = 25,
                BackColor = Color.LightGreen
            };
            btnRegisterUser.Click += BtnRegisterUser_Click;

            var btnUserManagement = new Button
            {
                Text = "User Management",
                Location = new Point(620, 8),
                Width = 120,
                Height = 25,
                BackColor = Color.FromArgb(220, 53, 69), // Red for admin function
                ForeColor = Color.White
            };
            btnUserManagement.Click += BtnUserManagement_Click;

            searchPanel.Controls.AddRange(new Control[] { txtSearch, btnSearch, btnRefresh, btnQuadraticResults, btnRegisterUser, btnUserManagement });
            panel.Controls.Add(searchPanel);

            // DataGridView
            dgvUsers = new DataGridView
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
            dgvUsers.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", DataPropertyName = "Id", Width = 50 },
                new DataGridViewTextBoxColumn { Name = "FirstName", HeaderText = "First Name", DataPropertyName = "FirstName", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "LastName", HeaderText = "Last Name", DataPropertyName = "LastName", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "Email", DataPropertyName = "Email", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "Phone", HeaderText = "Phone", DataPropertyName = "Phone", Width = 100 },
                new DataGridViewCheckBoxColumn { Name = "IsActive", HeaderText = "Active", DataPropertyName = "IsActive", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "DateCreated", HeaderText = "Created", DataPropertyName = "DateCreated", Width = 120 }
            });

            dgvUsers.SelectionChanged += DgvUsers_SelectionChanged;
            panel.Controls.Add(dgvUsers);

            return panel;
        }

        private Panel CreateRightPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            var y = 20;
            const int labelWidth = 80;
            const int textBoxWidth = 200;
            const int spacing = 35;

            // First Name
            panel.Controls.Add(new Label { Text = "First Name:", Location = new Point(10, y), Width = labelWidth });
            txtFirstName = new TextBox { Location = new Point(100, y), Width = textBoxWidth };
            panel.Controls.Add(txtFirstName);
            y += spacing;

            // Last Name
            panel.Controls.Add(new Label { Text = "Last Name:", Location = new Point(10, y), Width = labelWidth });
            txtLastName = new TextBox { Location = new Point(100, y), Width = textBoxWidth };
            panel.Controls.Add(txtLastName);
            y += spacing;

            // Email
            panel.Controls.Add(new Label { Text = "Email:", Location = new Point(10, y), Width = labelWidth });
            txtEmail = new TextBox { Location = new Point(100, y), Width = textBoxWidth };
            panel.Controls.Add(txtEmail);
            y += spacing;

            // Phone
            panel.Controls.Add(new Label { Text = "Phone:", Location = new Point(10, y), Width = labelWidth });
            txtPhone = new TextBox { Location = new Point(100, y), Width = textBoxWidth };
            panel.Controls.Add(txtPhone);
            y += spacing;

            // Is Active
            chkIsActive = new CheckBox { Text = "Active", Location = new Point(100, y), Checked = true };
            panel.Controls.Add(chkIsActive);
            y += spacing + 10;

            // Buttons
            const int buttonWidth = 80;
            const int buttonHeight = 30;

            btnCreate = new Button
            {
                Text = "Create",
                Location = new Point(10, y),
                Width = buttonWidth,
                Height = buttonHeight,
                BackColor = Color.LightGreen
            };
            btnCreate.Click += BtnCreate_Click;

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

            panel.Controls.AddRange(new Control[] { btnCreate, btnUpdate, btnDelete, btnClear });

            return panel;
        }

        private void LoadUsers()
        {
            try
            {
                var users = _userDAL.GetAllUsers();
                dgvUsers.DataSource = users;
                UpdateStatus($"Loaded {users.Count} users");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Error loading users");
            }
        }

        private void DgvUsers_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count > 0)
            {
                _selectedUser = dgvUsers.SelectedRows[0].DataBoundItem as User;
                if (_selectedUser != null)
                {
                    PopulateForm(_selectedUser);
                    btnUpdate.Enabled = true;
                    btnDelete.Enabled = true;
                }
            }
            else
            {
                _selectedUser = null;
                btnUpdate.Enabled = false;
                btnDelete.Enabled = false;
            }
        }

        private void PopulateForm(User user)
        {
            txtFirstName.Text = user.FirstName;
            txtLastName.Text = user.LastName;
            txtEmail.Text = user.Email;
            txtPhone.Text = user.Phone;
            chkIsActive.Checked = user.IsActive;
        }

        private void ClearForm()
        {
            txtFirstName.Clear();
            txtLastName.Clear();
            txtEmail.Clear();
            txtPhone.Clear();
            chkIsActive.Checked = true;
            _selectedUser = null;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
        }

        private User CreateUserFromForm()
        {
            return new User
            {
                FirstName = txtFirstName.Text.Trim(),
                LastName = txtLastName.Text.Trim(),
                Email = txtEmail.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                IsActive = chkIsActive.Checked
            };
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("First name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFirstName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Last name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLastName.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Email is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return false;
            }

            return true;
        }

        private void BtnCreate_Click(object? sender, EventArgs e)
        {
            if (!ValidateForm()) return;

            try
            {
                var user = CreateUserFromForm();
                int newId = _userDAL.CreateUser(user);
                
                if (newId > 0)
                {
                    MessageBox.Show($"User created successfully with ID: {newId}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadUsers();
                    UpdateStatus($"User created with ID: {newId}");
                }
                else
                {
                    MessageBox.Show("Failed to create user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    UpdateStatus("Failed to create user");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating user: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Error creating user");
            }
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (_selectedUser == null || !ValidateForm()) return;

            try
            {
                _selectedUser.FirstName = txtFirstName.Text.Trim();
                _selectedUser.LastName = txtLastName.Text.Trim();
                _selectedUser.Email = txtEmail.Text.Trim();
                _selectedUser.Phone = txtPhone.Text.Trim();
                _selectedUser.IsActive = chkIsActive.Checked;

                bool success = _userDAL.UpdateUser(_selectedUser);
                
                if (success)
                {
                    MessageBox.Show("User updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadUsers();
                    UpdateStatus($"User ID {_selectedUser.Id} updated");
                }
                else
                {
                    MessageBox.Show("Failed to update user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    UpdateStatus("Failed to update user");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating user: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Error updating user");
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_selectedUser == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete user '{_selectedUser.FullName}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    bool success = _userDAL.DeleteUser(_selectedUser.Id);
                    
                    if (success)
                    {
                        MessageBox.Show("User deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearForm();
                        LoadUsers();
                        UpdateStatus($"User ID {_selectedUser.Id} deleted");
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        UpdateStatus("Failed to delete user");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting user: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    UpdateStatus("Error deleting user");
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
                var users = string.IsNullOrWhiteSpace(searchTerm) 
                    ? _userDAL.GetAllUsers() 
                    : _userDAL.SearchUsers(searchTerm);
                
                dgvUsers.DataSource = users;
                UpdateStatus($"Found {users.Count} users");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching users: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Error searching users");
            }
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            LoadUsers();
            txtSearch.Clear();
        }

        private void BtnQuadraticResults_Click(object? sender, EventArgs e)
        {
            try
            {
                var quadraticForm = new QuadraticResultForm();
                quadraticForm.Show();
                UpdateStatus("Opened Quadratic Results form");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Quadratic Results form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Error opening Quadratic Results form");
            }
        }

        private void BtnRegisterUser_Click(object? sender, EventArgs e)
        {
            try
            {
                var registeredUser = RegistrationForm.ShowRegistrationDialog(this);
                
                if (registeredUser != null)
                {
                    // Registration successful - refresh the user list
                    LoadUsers();
                    UpdateStatus($"User '{registeredUser.FullName}' registered successfully.");
                    
                    // Select the newly registered user in the grid
                    SelectUserInGrid(registeredUser.Id);
                }
                else
                {
                    UpdateStatus("User registration cancelled.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening registration form: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Error during user registration.");
            }
        }

        private void SelectUserInGrid(int userId)
        {
            try
            {
                foreach (DataGridViewRow row in dgvUsers.Rows)
                {
                    if (row.Cells["Id"].Value != null && (int)row.Cells["Id"].Value == userId)
                    {
                        row.Selected = true;
                        dgvUsers.CurrentCell = row.Cells[0];
                        dgvUsers.FirstDisplayedScrollingRowIndex = row.Index;
                        DgvUsers_SelectionChanged(null, EventArgs.Empty);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Ignore selection errors
                System.Diagnostics.Debug.WriteLine($"Error selecting user in grid: {ex.Message}");
            }
        }

        private void BtnUserManagement_Click(object? sender, EventArgs e)
        {
            try
            {
                // Create a current admin user object (this would typically come from login)
                var currentAdmin = new User
                {
                    Id = 1, // This should be the actual logged-in admin ID
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@example.com",
                    Role = UserRole.Admin,
                    IsActive = true
                };

                // Open the admin user management form
                var result = AdminUserManagementForm.ShowAdminUserManagement(currentAdmin, this);
                
                if (result == DialogResult.OK)
                {
                    // Refresh the main form's user list if needed
                    LoadUsers();
                    UpdateStatus("User management completed - user list refreshed");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening user management: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Error opening user management");
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
            if (!_userDAL.TestConnection())
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
        }
    }
}