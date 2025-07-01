using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UserCRUD.Models;
using UserCRUD.DAL;
using UserCRUD.Services;

namespace UserCRUD.Forms
{
    /// <summary>
    /// Admin User Management Screen - Task 4.4 Implementation
    /// Provides comprehensive user management functionality for administrators
    /// </summary>
    public partial class AdminUserManagementForm : Form
    {
        private readonly User _currentAdmin;
        private readonly UserDAL _userDAL;
        private readonly AuthorizationService _authService;
        private List<User> _allUsers;
        private User? _selectedUser;

        // Task 4.4 Required Controls
        private DataGridView dgvUsers;           // Grid/List: UserId, Username, IsActive, IsAdmin, CreatedAt
        private Button btnActivateDeactivate;    // Button: Activate/Deactivate
        private Button btnDelete;                // Button: Delete
        private Button btnViewDetail;            // Button: View detail

        // Additional UI Controls
        private Label lblTitle;
        private Label lblUserCount;
        private Label lblSelectedUser;
        private Label lblStatus;
        private TextBox txtSearch;
        private Button btnSearch;
        private Button btnRefresh;
        private Button btnAddUser;
        private Button btnEditUser;
        private Button btnExport;
        private ComboBox cmbFilter;
        private GroupBox grpUserList;
        private GroupBox grpActions;
        private GroupBox grpFilters;
        private Panel pnlMain;
        private MenuStrip menuStrip;
        private ContextMenuStrip contextMenu;

        public AdminUserManagementForm(User currentAdmin)
        {
            _currentAdmin = currentAdmin ?? throw new ArgumentNullException(nameof(currentAdmin));
            
            // Verify admin permissions
            if (!_currentAdmin.IsAdmin)
            {
                throw new UnauthorizedAccessException("Only administrators can access user management.");
            }

            _userDAL = new UserDAL();
            _authService = new AuthorizationService();
            _allUsers = new List<User>();

            InitializeComponent();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            // Form properties
            this.Text = "Admin User Management - User Management System";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 248, 255); // Light blue background
            this.MinimumSize = new Size(1000, 600);

            // Create menu strip
            CreateMenuStrip();

            // Main panel
            pnlMain = new Panel
            {
                Location = new Point(10, 50),
                Size = new Size(1160, 720),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(pnlMain);

            // Title
            lblTitle = new Label
            {
                Text = $"User Management - Logged in as: {_currentAdmin.FullName}",
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(20, 20),
                Size = new Size(600, 30)
            };
            pnlMain.Controls.Add(lblTitle);

            // Create filter section
            CreateFilterSection();

            // Create user list section
            CreateUserListSection();

            // Create action buttons section
            CreateActionSection();

            // Create status bar
            CreateStatusBar();

            // Create context menu
            CreateContextMenu();

            // Set initial state
            UpdateButtonStates();
        }

        #region UI Creation Methods

        private void CreateMenuStrip()
        {
            menuStrip = new MenuStrip();

            // File menu
            var fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add("Add New User", null, (s, e) => BtnAddUser_Click(s, e));
            fileMenu.DropDownItems.Add("Export User List", null, (s, e) => BtnExport_Click(s, e));
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Close", null, (s, e) => this.Close());

            // Edit menu
            var editMenu = new ToolStripMenuItem("Edit");
            editMenu.DropDownItems.Add("View Details", null, (s, e) => BtnViewDetail_Click(s, e));
            editMenu.DropDownItems.Add("Edit User", null, (s, e) => BtnEditUser_Click(s, e));
            editMenu.DropDownItems.Add(new ToolStripSeparator());
            editMenu.DropDownItems.Add("Activate/Deactivate", null, (s, e) => BtnActivateDeactivate_Click(s, e));
            editMenu.DropDownItems.Add("Delete User", null, (s, e) => BtnDelete_Click(s, e));

            // View menu
            var viewMenu = new ToolStripMenuItem("View");
            viewMenu.DropDownItems.Add("Refresh", null, (s, e) => BtnRefresh_Click(s, e));
            viewMenu.DropDownItems.Add("Show All Users", null, (s, e) => ShowAllUsers());
            viewMenu.DropDownItems.Add("Show Active Only", null, (s, e) => ShowActiveUsers());
            viewMenu.DropDownItems.Add("Show Inactive Only", null, (s, e) => ShowInactiveUsers());

            // Tools menu
            var toolsMenu = new ToolStripMenuItem("Tools");
            toolsMenu.DropDownItems.Add("User Statistics", null, ShowUserStatistics);
            toolsMenu.DropDownItems.Add("Security Report", null, ShowSecurityReport);

            // Help menu
            var helpMenu = new ToolStripMenuItem("Help");
            helpMenu.DropDownItems.Add("User Management Guide", null, ShowUserManagementHelp);
            helpMenu.DropDownItems.Add("About", null, ShowAbout);

            menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, editMenu, viewMenu, toolsMenu, helpMenu });
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
        }

        private void CreateFilterSection()
        {
            // Filter group box
            grpFilters = new GroupBox
            {
                Text = "Search and Filter",
                Location = new Point(20, 60),
                Size = new Size(1120, 80),
                Font = new Font("Arial", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlMain.Controls.Add(grpFilters);

            // Search textbox
            var lblSearch = new Label
            {
                Text = "Search:",
                Location = new Point(20, 30),
                Size = new Size(50, 20),
                Font = new Font("Arial", 9)
            };
            grpFilters.Controls.Add(lblSearch);

            txtSearch = new TextBox
            {
                Location = new Point(80, 28),
                Size = new Size(200, 25),
                Font = new Font("Arial", 10),
                PlaceholderText = "Search by name, email, or phone"
            };
            txtSearch.KeyPress += TxtSearch_KeyPress;
            grpFilters.Controls.Add(txtSearch);

            // Search button
            btnSearch = new Button
            {
                Text = "Search",
                Location = new Point(290, 27),
                Size = new Size(70, 27),
                Font = new Font("Arial", 9),
                BackColor = Color.LightBlue
            };
            btnSearch.Click += BtnSearch_Click;
            grpFilters.Controls.Add(btnSearch);

            // Filter combobox
            var lblFilter = new Label
            {
                Text = "Filter:",
                Location = new Point(380, 30),
                Size = new Size(40, 20),
                Font = new Font("Arial", 9)
            };
            grpFilters.Controls.Add(lblFilter);

            cmbFilter = new ComboBox
            {
                Location = new Point(430, 28),
                Size = new Size(150, 25),
                Font = new Font("Arial", 9),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbFilter.Items.AddRange(new string[] { "All Users", "Active Users", "Inactive Users", "Admin Users", "Regular Users" });
            cmbFilter.SelectedIndex = 0;
            cmbFilter.SelectedIndexChanged += CmbFilter_SelectedIndexChanged;
            grpFilters.Controls.Add(cmbFilter);

            // Refresh button
            btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(600, 27),
                Size = new Size(70, 27),
                Font = new Font("Arial", 9),
                BackColor = Color.LightGreen
            };
            btnRefresh.Click += BtnRefresh_Click;
            grpFilters.Controls.Add(btnRefresh);

            // User count label
            lblUserCount = new Label
            {
                Text = "Total Users: 0",
                Location = new Point(700, 30),
                Size = new Size(150, 20),
                Font = new Font("Arial", 9, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };
            grpFilters.Controls.Add(lblUserCount);
        }

        private void CreateUserListSection()
        {
            // User list group box
            grpUserList = new GroupBox
            {
                Text = "User List",
                Location = new Point(20, 150),
                Size = new Size(1120, 400),
                Font = new Font("Arial", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlMain.Controls.Add(grpUserList);

            // Task 4.4 Required Grid: UserId, Username, IsActive, IsAdmin, CreatedAt
            dgvUsers = new DataGridView
            {
                Location = new Point(20, 30),
                Size = new Size(1080, 350),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // Configure columns for Task 4.4 requirements
            dgvUsers.Columns.Add("UserId", "User ID");
            dgvUsers.Columns.Add("Username", "Username/Email");
            dgvUsers.Columns.Add("FullName", "Full Name");
            dgvUsers.Columns.Add("IsActive", "Active");
            dgvUsers.Columns.Add("IsAdmin", "Admin");
            dgvUsers.Columns.Add("CreatedAt", "Created At");
            dgvUsers.Columns.Add("LastLogin", "Last Login");
            dgvUsers.Columns.Add("Phone", "Phone");

            // Set column widths
            dgvUsers.Columns["UserId"].Width = 80;
            dgvUsers.Columns["Username"].Width = 200;
            dgvUsers.Columns["FullName"].Width = 180;
            dgvUsers.Columns["IsActive"].Width = 80;
            dgvUsers.Columns["IsAdmin"].Width = 80;
            dgvUsers.Columns["CreatedAt"].Width = 120;
            dgvUsers.Columns["LastLogin"].Width = 120;
            dgvUsers.Columns["Phone"].Width = 120;

            // Set column alignment
            dgvUsers.Columns["UserId"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvUsers.Columns["IsActive"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvUsers.Columns["IsAdmin"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Event handlers
            dgvUsers.SelectionChanged += DgvUsers_SelectionChanged;
            dgvUsers.DoubleClick += DgvUsers_DoubleClick;
            dgvUsers.CellFormatting += DgvUsers_CellFormatting;

            // Add context menu
            dgvUsers.ContextMenuStrip = contextMenu;

            grpUserList.Controls.Add(dgvUsers);
        }

        private void CreateActionSection()
        {
            // Actions group box
            grpActions = new GroupBox
            {
                Text = "User Actions",
                Location = new Point(20, 570),
                Size = new Size(1120, 100),
                Font = new Font("Arial", 10, FontStyle.Bold),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlMain.Controls.Add(grpActions);

            // Selected user label
            lblSelectedUser = new Label
            {
                Text = "No user selected",
                Location = new Point(20, 25),
                Size = new Size(400, 20),
                Font = new Font("Arial", 9),
                ForeColor = Color.DarkBlue
            };
            grpActions.Controls.Add(lblSelectedUser);

            // Task 4.4 Required Buttons

            // View Detail button (Task 4.4 requirement)
            btnViewDetail = new Button
            {
                Text = "View Detail",
                Location = new Point(20, 50),
                Size = new Size(100, 35),
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(23, 162, 184), // Cyan
                ForeColor = Color.White,
                UseVisualStyleBackColor = false,
                Enabled = false
            };
            btnViewDetail.Click += BtnViewDetail_Click;
            grpActions.Controls.Add(btnViewDetail);

            // Activate/Deactivate button (Task 4.4 requirement)
            btnActivateDeactivate = new Button
            {
                Text = "Activate",
                Location = new Point(140, 50),
                Size = new Size(100, 35),
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(40, 167, 69), // Green
                ForeColor = Color.White,
                UseVisualStyleBackColor = false,
                Enabled = false
            };
            btnActivateDeactivate.Click += BtnActivateDeactivate_Click;
            grpActions.Controls.Add(btnActivateDeactivate);

            // Delete button (Task 4.4 requirement)
            btnDelete = new Button
            {
                Text = "Delete",
                Location = new Point(260, 50),
                Size = new Size(100, 35),
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(220, 53, 69), // Red
                ForeColor = Color.White,
                UseVisualStyleBackColor = false,
                Enabled = false
            };
            btnDelete.Click += BtnDelete_Click;
            grpActions.Controls.Add(btnDelete);

            // Additional action buttons
            btnEditUser = new Button
            {
                Text = "Edit User",
                Location = new Point(380, 50),
                Size = new Size(100, 35),
                Font = new Font("Arial", 10),
                BackColor = Color.FromArgb(255, 193, 7), // Yellow
                ForeColor = Color.Black,
                UseVisualStyleBackColor = false,
                Enabled = false
            };
            btnEditUser.Click += BtnEditUser_Click;
            grpActions.Controls.Add(btnEditUser);

            btnAddUser = new Button
            {
                Text = "Add User",
                Location = new Point(500, 50),
                Size = new Size(100, 35),
                Font = new Font("Arial", 10),
                BackColor = Color.FromArgb(0, 123, 255), // Blue
                ForeColor = Color.White,
                UseVisualStyleBackColor = false
            };
            btnAddUser.Click += BtnAddUser_Click;
            grpActions.Controls.Add(btnAddUser);

            btnExport = new Button
            {
                Text = "Export",
                Location = new Point(620, 50),
                Size = new Size(100, 35),
                Font = new Font("Arial", 10),
                BackColor = Color.FromArgb(108, 117, 125), // Gray
                ForeColor = Color.White,
                UseVisualStyleBackColor = false
            };
            btnExport.Click += BtnExport_Click;
            grpActions.Controls.Add(btnExport);
        }

        private void CreateStatusBar()
        {
            lblStatus = new Label
            {
                Location = new Point(20, 690),
                Size = new Size(1120, 20),
                Font = new Font("Arial", 9),
                ForeColor = Color.Gray,
                Text = "Ready - Select a user to perform actions",
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlMain.Controls.Add(lblStatus);
        }

        private void CreateContextMenu()
        {
            contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("View Details", null, (s, e) => BtnViewDetail_Click(s, e));
            contextMenu.Items.Add("Edit User", null, (s, e) => BtnEditUser_Click(s, e));
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Activate/Deactivate", null, (s, e) => BtnActivateDeactivate_Click(s, e));
            contextMenu.Items.Add("Delete User", null, (s, e) => BtnDelete_Click(s, e));
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Refresh", null, (s, e) => BtnRefresh_Click(s, e));
        }

        #endregion

        #region Event Handlers

        private void TxtSearch_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                BtnSearch_Click(sender, e);
                e.Handled = true;
            }
        }

        private void BtnSearch_Click(object? sender, EventArgs e)
        {
            PerformSearch();
        }

        private void CmbFilter_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void BtnRefresh_Click(object? sender, EventArgs e)
        {
            LoadUsers();
        }

        private void DgvUsers_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateSelectedUser();
            UpdateButtonStates();
        }

        private void DgvUsers_DoubleClick(object? sender, EventArgs e)
        {
            if (_selectedUser != null)
            {
                BtnViewDetail_Click(sender, e);
            }
        }

        private void DgvUsers_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvUsers.Columns[e.ColumnIndex].Name == "IsActive")
            {
                if (e.Value is bool isActive)
                {
                    e.Value = isActive ? "Yes" : "No";
                    e.CellStyle.ForeColor = isActive ? Color.Green : Color.Red;
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
            }
            else if (dgvUsers.Columns[e.ColumnIndex].Name == "IsAdmin")
            {
                if (e.Value is bool isAdmin)
                {
                    e.Value = isAdmin ? "Yes" : "No";
                    e.CellStyle.ForeColor = isAdmin ? Color.Blue : Color.Gray;
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                }
            }
        }

        // Task 4.4 Required Button Event Handlers

        // Task 4.4 View Detail Button Implementation
        private void BtnViewDetail_Click(object? sender, EventArgs e)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Please select a user to view details.", "No User Selected", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ShowUserDetails(_selectedUser);
        }

        // Task 4.4 Activate/Deactivate Button Implementation
        private void BtnActivateDeactivate_Click(object? sender, EventArgs e)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Please select a user to activate/deactivate.", "No User Selected", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            PerformActivateDeactivate(_selectedUser);
        }

        // Task 4.4 Delete Button Implementation
        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Please select a user to delete.", "No User Selected", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            PerformDelete(_selectedUser);
        }

        private void BtnEditUser_Click(object? sender, EventArgs e)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Please select a user to edit.", "No User Selected", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            EditUser(_selectedUser);
        }

        private void BtnAddUser_Click(object? sender, EventArgs e)
        {
            AddNewUser();
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            ExportUserList();
        }

        #endregion

        #region Task 4.4 Core Logic Implementation

        /// <summary>
        /// Task 4.4 Logic: Load and display users in grid with required columns
        /// </summary>
        private void LoadUsers()
        {
            try
            {
                // Check authorization
                if (!_authService.CheckUserCRUDPermission(_currentAdmin.Id, 0, "read").IsAuthorized)
                {
                    MessageBox.Show("You don't have permission to view users.", "Access Denied", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Load all users from database
                _allUsers = _userDAL.GetAllUsers();

                // Populate grid with Task 4.4 required columns
                PopulateUserGrid(_allUsers);

                UpdateStatus($"Loaded {_allUsers.Count} users successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Load Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Error loading users");
            }
        }

        /// <summary>
        /// Task 4.4 Logic: Populate grid with UserId, Username, IsActive, IsAdmin, CreatedAt
        /// </summary>
        private void PopulateUserGrid(List<User> users)
        {
            dgvUsers.Rows.Clear();

            foreach (var user in users)
            {
                var row = new DataGridViewRow();
                row.CreateCells(dgvUsers);

                // Task 4.4 Required Columns
                row.Cells[0].Value = user.Id;                                    // UserId
                row.Cells[1].Value = user.Email;                                 // Username (using email)
                row.Cells[2].Value = user.FullName;                             // Full Name
                row.Cells[3].Value = user.IsActive;                             // IsActive
                row.Cells[4].Value = user.IsAdmin;                              // IsAdmin
                row.Cells[5].Value = user.DateCreated.ToString("yyyy-MM-dd");   // CreatedAt
                row.Cells[6].Value = user.LastLoginDate?.ToString("yyyy-MM-dd") ?? "Never"; // Last Login
                row.Cells[7].Value = user.Phone ?? "";                          // Phone

                // Store user object in row tag
                row.Tag = user;

                // Color coding for better visibility
                if (!user.IsActive)
                {
                    row.DefaultCellStyle.BackColor = Color.LightGray;
                    row.DefaultCellStyle.ForeColor = Color.DarkGray;
                }
                else if (user.IsAdmin)
                {
                    row.DefaultCellStyle.BackColor = Color.LightBlue;
                }

                dgvUsers.Rows.Add(row);
            }

            // Update user count
            lblUserCount.Text = $"Total Users: {users.Count}";
        }

        /// <summary>
        /// Task 4.4 Logic: Activate/Deactivate user functionality
        /// </summary>
        private void PerformActivateDeactivate(User user)
        {
            try
            {
                // Check authorization
                if (!_authService.CheckUserCRUDPermission(_currentAdmin.Id, user.Id, "update").IsAuthorized)
                {
                    MessageBox.Show("You don't have permission to modify this user.", "Access Denied", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string action = user.IsActive ? "deactivate" : "activate";
                string message = user.IsActive 
                    ? $"Are you sure you want to deactivate user '{user.FullName}'?\n\nThis will prevent them from logging in."
                    : $"Are you sure you want to activate user '{user.FullName}'?\n\nThis will allow them to log in again.";

                var result = MessageBox.Show(message, $"Confirm {action.ToUpper()}", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Toggle active status
                    user.IsActive = !user.IsActive;
                    user.DateModified = DateTime.Now;

                    // Update in database
                    bool success = _userDAL.UpdateUser(user);

                    if (success)
                    {
                        // Refresh the grid
                        LoadUsers();

                        MessageBox.Show($"User '{user.FullName}' has been {action}d successfully.", 
                            "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        UpdateStatus($"User {action}d: {user.FullName}");
                    }
                    else
                    {
                        MessageBox.Show($"Failed to {action} user. Please try again.", 
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        UpdateStatus($"Failed to {action} user");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during activate/deactivate: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Error during user status change");
            }
        }

        /// <summary>
        /// Task 4.4 Logic: Delete user with restrictions (cannot delete own admin account, confirm before delete)
        /// </summary>
        private void PerformDelete(User user)
        {
            try
            {
                // Task 4.4 Logic: Cannot delete own admin account
                if (user.Id == _currentAdmin.Id)
                {
                    MessageBox.Show("You cannot delete your own admin account.", "Cannot Delete Own Account", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check authorization
                if (!_authService.CheckUserCRUDPermission(_currentAdmin.Id, user.Id, "delete").IsAuthorized)
                {
                    MessageBox.Show("You don't have permission to delete this user.", "Access Denied", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Task 4.4 Logic: Confirm before delete
                string warningMessage = $"Are you sure you want to DELETE user '{user.FullName}'?\n\n" +
                                       $"Email: {user.Email}\n" +
                                       $"Role: {(user.IsAdmin ? "Administrator" : "User")}\n" +
                                       $"Status: {(user.IsActive ? "Active" : "Inactive")}\n\n" +
                                       "⚠️ WARNING: This action cannot be undone!\n" +
                                       "All user data and associated records will be permanently deleted.";

                var result = MessageBox.Show(warningMessage, "CONFIRM DELETE", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

                if (result == DialogResult.Yes)
                {
                    // Additional confirmation for admin users
                    if (user.IsAdmin)
                    {
                        var adminConfirm = MessageBox.Show(
                            $"You are about to delete an ADMINISTRATOR account!\n\n" +
                            $"This will remove all administrative privileges for '{user.FullName}'.\n\n" +
                            "Are you absolutely certain you want to proceed?",
                            "DELETE ADMINISTRATOR - FINAL CONFIRMATION",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button2);

                        if (adminConfirm != DialogResult.Yes)
                        {
                            UpdateStatus("Admin deletion cancelled");
                            return;
                        }
                    }

                    // Perform deletion
                    bool success = _userDAL.DeleteUser(user.Id);

                    if (success)
                    {
                        // Refresh the grid
                        LoadUsers();

                        MessageBox.Show($"User '{user.FullName}' has been deleted successfully.", 
                            "Deletion Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        UpdateStatus($"User deleted: {user.FullName}");

                        // Clear selection
                        _selectedUser = null;
                        UpdateButtonStates();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete user. The user may have associated data that prevents deletion.", 
                            "Deletion Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        UpdateStatus("Failed to delete user");
                    }
                }
                else
                {
                    UpdateStatus("User deletion cancelled");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during user deletion: {ex.Message}", 
                    "Deletion Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Error during user deletion");
            }
        }

        /// <summary>
        /// Task 4.4 Logic: View user details
        /// </summary>
        private void ShowUserDetails(User user)
        {
            try
            {
                // Check authorization
                if (!_authService.CheckUserCRUDPermission(_currentAdmin.Id, user.Id, "read").IsAuthorized)
                {
                    MessageBox.Show("You don't have permission to view this user's details.", "Access Denied", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Create detailed user information
                var details = $"User Details\n" +
                             $"{'='*50}\n\n" +
                             $"User ID: {user.Id}\n" +
                             $"Full Name: {user.FullName}\n" +
                             $"Email: {user.Email}\n" +
                             $"Phone: {user.Phone ?? "Not provided"}\n" +
                             $"Role: {(user.IsAdmin ? "Administrator" : "Regular User")}\n" +
                             $"Status: {(user.IsActive ? "Active" : "Inactive")}\n\n" +
                             $"Account Information:\n" +
                             $"Created: {user.DateCreated:yyyy-MM-dd HH:mm:ss}\n" +
                             $"Modified: {user.DateModified?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never"}\n" +
                             $"Last Login: {user.LastLoginDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Never"}\n\n" +
                             $"Security Information:\n" +
                             $"Login Attempts: {user.LoginAttempts}\n" +
                             $"Account Locked: {(user.IsLocked ? $"Yes (until {user.LockedUntil:yyyy-MM-dd HH:mm:ss})" : "No")}\n" +
                             $"Password Set: {(!string.IsNullOrEmpty(user.PasswordHash) ? "Yes" : "No")}";

                // Show in a custom dialog
                var detailForm = new Form
                {
                    Text = $"User Details - {user.FullName}",
                    Size = new Size(500, 600),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                };

                var textBox = new TextBox
                {
                    Text = details,
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    Font = new Font("Consolas", 10),
                    Dock = DockStyle.Fill,
                    BackColor = Color.White
                };

                var closeButton = new Button
                {
                    Text = "Close",
                    DialogResult = DialogResult.OK,
                    Size = new Size(100, 30),
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right
                };
                closeButton.Location = new Point(detailForm.Width - closeButton.Width - 30, 
                                                detailForm.Height - closeButton.Height - 60);

                detailForm.Controls.Add(textBox);
                detailForm.Controls.Add(closeButton);
                detailForm.AcceptButton = closeButton;

                detailForm.ShowDialog(this);

                UpdateStatus($"Viewed details for user: {user.FullName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing user details: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Error showing user details");
            }
        }

        #endregion

        #region Helper Methods

        private void UpdateSelectedUser()
        {
            if (dgvUsers.SelectedRows.Count > 0)
            {
                _selectedUser = dgvUsers.SelectedRows[0].Tag as User;
                lblSelectedUser.Text = _selectedUser != null 
                    ? $"Selected: {_selectedUser.FullName} ({_selectedUser.Email})"
                    : "No user selected";
            }
            else
            {
                _selectedUser = null;
                lblSelectedUser.Text = "No user selected";
            }
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = _selectedUser != null;
            bool canModify = hasSelection && _authService.CheckUserCRUDPermission(_currentAdmin.Id, _selectedUser?.Id ?? 0, "update").IsAuthorized;
            bool canDelete = hasSelection && _selectedUser?.Id != _currentAdmin.Id && 
                           _authService.CheckUserCRUDPermission(_currentAdmin.Id, _selectedUser?.Id ?? 0, "delete").IsAuthorized;

            btnViewDetail.Enabled = hasSelection;
            btnActivateDeactivate.Enabled = canModify;
            btnDelete.Enabled = canDelete;
            btnEditUser.Enabled = canModify;

            // Update activate/deactivate button text
            if (_selectedUser != null)
            {
                btnActivateDeactivate.Text = _selectedUser.IsActive ? "Deactivate" : "Activate";
                btnActivateDeactivate.BackColor = _selectedUser.IsActive 
                    ? Color.FromArgb(220, 53, 69)   // Red for deactivate
                    : Color.FromArgb(40, 167, 69);  // Green for activate
            }
            else
            {
                btnActivateDeactivate.Text = "Activate";
                btnActivateDeactivate.BackColor = Color.FromArgb(40, 167, 69);
            }
        }

        private void PerformSearch()
        {
            string searchTerm = txtSearch.Text.Trim().ToLower();
            
            if (string.IsNullOrEmpty(searchTerm))
            {
                PopulateUserGrid(_allUsers);
                UpdateStatus("Search cleared - showing all users");
                return;
            }

            var filteredUsers = _allUsers.Where(u => 
                u.FullName.ToLower().Contains(searchTerm) ||
                u.Email.ToLower().Contains(searchTerm) ||
                (u.Phone?.ToLower().Contains(searchTerm) ?? false)
            ).ToList();

            PopulateUserGrid(filteredUsers);
            UpdateStatus($"Search results: {filteredUsers.Count} users found");
        }

        private void ApplyFilter()
        {
            List<User> filteredUsers;

            switch (cmbFilter.SelectedIndex)
            {
                case 1: // Active Users
                    filteredUsers = _allUsers.Where(u => u.IsActive).ToList();
                    break;
                case 2: // Inactive Users
                    filteredUsers = _allUsers.Where(u => !u.IsActive).ToList();
                    break;
                case 3: // Admin Users
                    filteredUsers = _allUsers.Where(u => u.IsAdmin).ToList();
                    break;
                case 4: // Regular Users
                    filteredUsers = _allUsers.Where(u => !u.IsAdmin).ToList();
                    break;
                default: // All Users
                    filteredUsers = _allUsers;
                    break;
            }

            PopulateUserGrid(filteredUsers);
            UpdateStatus($"Filter applied: {filteredUsers.Count} users shown");
        }

        private void ShowAllUsers()
        {
            cmbFilter.SelectedIndex = 0;
            txtSearch.Clear();
            PopulateUserGrid(_allUsers);
        }

        private void ShowActiveUsers()
        {
            cmbFilter.SelectedIndex = 1;
        }

        private void ShowInactiveUsers()
        {
            cmbFilter.SelectedIndex = 2;
        }

        private void EditUser(User user)
        {
            try
            {
                // Open registration form in edit mode
                // This would typically open a user edit form
                MessageBox.Show($"Edit functionality for user '{user.FullName}' would open here.\n\n" +
                               "This would typically open a form similar to the registration form\n" +
                               "but pre-populated with the user's current information.",
                               "Edit User", MessageBoxButtons.OK, MessageBoxIcon.Information);

                UpdateStatus($"Edit requested for user: {user.FullName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening edit form: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddNewUser()
        {
            try
            {
                // Open registration form for new user
                var registeredUser = RegistrationForm.ShowRegistrationDialog(this);
                
                if (registeredUser != null)
                {
                    // Refresh the user list
                    LoadUsers();
                    
                    // Select the newly created user
                    SelectUserInGrid(registeredUser.Id);
                    
                    MessageBox.Show($"User '{registeredUser.FullName}' has been created successfully.", 
                        "User Created", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    UpdateStatus($"New user created: {registeredUser.FullName}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating new user: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Error creating new user");
            }
        }

        private void SelectUserInGrid(int userId)
        {
            foreach (DataGridViewRow row in dgvUsers.Rows)
            {
                if (row.Tag is User user && user.Id == userId)
                {
                    row.Selected = true;
                    dgvUsers.CurrentCell = row.Cells[0];
                    dgvUsers.FirstDisplayedScrollingRowIndex = row.Index;
                    break;
                }
            }
        }

        private void ExportUserList()
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt",
                    DefaultExt = "csv",
                    FileName = $"UserList_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    var csv = new System.Text.StringBuilder();
                    csv.AppendLine("UserId,Username,FullName,IsActive,IsAdmin,CreatedAt,LastLogin,Phone");

                    foreach (var user in _allUsers)
                    {
                        csv.AppendLine($"{user.Id},{user.Email},{user.FullName},{user.IsActive},{user.IsAdmin}," +
                                     $"{user.DateCreated:yyyy-MM-dd},{user.LastLoginDate?.ToString("yyyy-MM-dd") ?? "Never"},{user.Phone ?? ""}");
                    }

                    System.IO.File.WriteAllText(saveDialog.FileName, csv.ToString());
                    
                    MessageBox.Show($"User list exported successfully to:\n{saveDialog.FileName}", 
                        "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    UpdateStatus($"User list exported to: {saveDialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting user list: {ex.Message}", 
                    "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Error exporting user list");
            }
        }

        private void UpdateStatus(string message)
        {
            lblStatus.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
        }

        #endregion

        #region Menu Event Handlers

        private void ShowUserStatistics(object? sender, EventArgs e)
        {
            var totalUsers = _allUsers.Count;
            var activeUsers = _allUsers.Count(u => u.IsActive);
            var inactiveUsers = _allUsers.Count(u => !u.IsActive);
            var adminUsers = _allUsers.Count(u => u.IsAdmin);
            var regularUsers = _allUsers.Count(u => !u.IsAdmin);
            var usersWithLogins = _allUsers.Count(u => u.LastLoginDate.HasValue);

            var stats = $"User Statistics\n" +
                       $"{'='*30}\n\n" +
                       $"Total Users: {totalUsers}\n" +
                       $"Active Users: {activeUsers} ({(totalUsers > 0 ? (double)activeUsers / totalUsers * 100 : 0):F1}%)\n" +
                       $"Inactive Users: {inactiveUsers} ({(totalUsers > 0 ? (double)inactiveUsers / totalUsers * 100 : 0):F1}%)\n\n" +
                       $"Admin Users: {adminUsers} ({(totalUsers > 0 ? (double)adminUsers / totalUsers * 100 : 0):F1}%)\n" +
                       $"Regular Users: {regularUsers} ({(totalUsers > 0 ? (double)regularUsers / totalUsers * 100 : 0):F1}%)\n\n" +
                       $"Users with Login History: {usersWithLogins} ({(totalUsers > 0 ? (double)usersWithLogins / totalUsers * 100 : 0):F1}%)\n" +
                       $"Users Never Logged In: {totalUsers - usersWithLogins}";

            MessageBox.Show(stats, "User Statistics", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowSecurityReport(object? sender, EventArgs e)
        {
            var lockedUsers = _allUsers.Count(u => u.IsLocked);
            var usersWithFailedAttempts = _allUsers.Count(u => u.LoginAttempts > 0);
            var inactiveAdmins = _allUsers.Count(u => u.IsAdmin && !u.IsActive);

            var report = $"Security Report\n" +
                        $"{'='*30}\n\n" +
                        $"Locked Accounts: {lockedUsers}\n" +
                        $"Users with Failed Login Attempts: {usersWithFailedAttempts}\n" +
                        $"Inactive Admin Accounts: {inactiveAdmins}\n\n" +
                        $"Recommendations:\n" +
                        $"• Review locked accounts regularly\n" +
                        $"• Monitor failed login attempts\n" +
                        $"• Deactivate unused admin accounts\n" +
                        $"• Enforce strong password policies";

            MessageBox.Show(report, "Security Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowUserManagementHelp(object? sender, EventArgs e)
        {
            var help = @"User Management Guide

VIEWING USERS:
• The grid shows all users with their key information
• Use filters to show specific user types
• Search by name, email, or phone number

MANAGING USERS:
• Select a user to enable action buttons
• View Details: Shows complete user information
• Activate/Deactivate: Controls user login access
• Delete: Permanently removes user (with confirmation)
• Edit: Modify user information

SECURITY FEATURES:
• Cannot delete your own admin account
• Confirmation required before deletion
• Admin deletions require additional confirmation
• All actions are logged for audit purposes

KEYBOARD SHORTCUTS:
• F5: Refresh user list
• Ctrl+F: Focus search box
• Delete: Delete selected user
• Enter: View details of selected user";

            MessageBox.Show(help, "User Management Guide", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowAbout(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "Admin User Management System\n" +
                "Version 1.0 - Task 4.4 Implementation\n\n" +
                "Features:\n" +
                "• Complete user management\n" +
                "• Role-based access control\n" +
                "• Security audit capabilities\n" +
                "• Export functionality\n\n" +
                "Developed for User CRUD Application",
                "About User Management",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            // Set focus to search box
            txtSearch.Focus();
        }

        #region Static Methods

        /// <summary>
        /// Shows the admin user management form
        /// </summary>
        /// <param name="currentAdmin">Current admin user</param>
        /// <param name="parent">Parent window</param>
        /// <returns>Dialog result</returns>
        public static DialogResult ShowAdminUserManagement(User currentAdmin, IWin32Window? parent = null)
        {
            using (var adminForm = new AdminUserManagementForm(currentAdmin))
            {
                return adminForm.ShowDialog(parent);
            }
        }

        #endregion
    }
}