using System;
using System.Collections.Generic;
using System.Linq;

namespace UserCRUD.Models
{
    /// <summary>
    /// Enumeration of available user roles in the system
    /// </summary>
    public enum UserRole
    {
        User = 1,
        Admin = 2
    }

    /// <summary>
    /// Enumeration of system permissions
    /// </summary>
    public enum Permission
    {
        // User Management Permissions
        ViewUsers = 1,
        CreateUser = 2,
        UpdateUser = 3,
        DeleteUser = 4,
        DeactivateUser = 5,
        
        // Self Management Permissions
        ViewOwnProfile = 10,
        UpdateOwnProfile = 11,
        ChangeOwnPassword = 12,
        
        // Quadratic Results Permissions
        ViewQuadraticResults = 20,
        CreateQuadraticResult = 21,
        UpdateQuadraticResult = 22,
        DeleteQuadraticResult = 23,
        ViewOwnQuadraticResults = 24,
        UpdateOwnQuadraticResults = 25,
        DeleteOwnQuadraticResults = 26,
        
        // Administrative Permissions
        ManageRoles = 30,
        ViewSystemLogs = 31,
        ManageSystemSettings = 32,
        UnlockUserAccounts = 33,
        ResetUserPasswords = 34,
        ViewUserStatistics = 35,
        
        // Security Permissions
        ViewSecurityLogs = 40,
        ManageSecuritySettings = 41
    }

    /// <summary>
    /// Role definition with associated permissions
    /// </summary>
    public class Role
    {
        public UserRole RoleType { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Permission> Permissions { get; set; } = new List<Permission>();

        public Role() { }

        public Role(UserRole roleType, string name, string description, List<Permission> permissions)
        {
            RoleType = roleType;
            Name = name;
            Description = description;
            Permissions = permissions ?? new List<Permission>();
        }

        /// <summary>
        /// Checks if this role has a specific permission
        /// </summary>
        /// <param name="permission">Permission to check</param>
        /// <returns>True if role has the permission</returns>
        public bool HasPermission(Permission permission)
        {
            return Permissions.Contains(permission);
        }

        /// <summary>
        /// Checks if this role has any of the specified permissions
        /// </summary>
        /// <param name="permissions">Permissions to check</param>
        /// <returns>True if role has any of the permissions</returns>
        public bool HasAnyPermission(params Permission[] permissions)
        {
            return permissions.Any(p => Permissions.Contains(p));
        }

        /// <summary>
        /// Checks if this role has all of the specified permissions
        /// </summary>
        /// <param name="permissions">Permissions to check</param>
        /// <returns>True if role has all of the permissions</returns>
        public bool HasAllPermissions(params Permission[] permissions)
        {
            return permissions.All(p => Permissions.Contains(p));
        }

        public override string ToString()
        {
            return $"{Name} ({Permissions.Count} permissions)";
        }
    }

    /// <summary>
    /// Static class defining default roles and their permissions
    /// </summary>
    public static class DefaultRoles
    {
        /// <summary>
        /// Gets the User role with standard user permissions
        /// </summary>
        public static Role UserRole => new Role(
            UserRole.User,
            "User",
            "Standard user with limited permissions",
            new List<Permission>
            {
                // Self-management permissions
                Permission.ViewOwnProfile,
                Permission.UpdateOwnProfile,
                Permission.ChangeOwnPassword,
                
                // Own quadratic results permissions
                Permission.ViewOwnQuadraticResults,
                Permission.CreateQuadraticResult,
                Permission.UpdateOwnQuadraticResults,
                Permission.DeleteOwnQuadraticResults
            }
        );

        /// <summary>
        /// Gets the Admin role with full system permissions
        /// </summary>
        public static Role AdminRole => new Role(
            UserRole.Admin,
            "Administrator",
            "System administrator with full permissions",
            new List<Permission>
            {
                // All user management permissions
                Permission.ViewUsers,
                Permission.CreateUser,
                Permission.UpdateUser,
                Permission.DeleteUser,
                Permission.DeactivateUser,
                
                // Self-management permissions
                Permission.ViewOwnProfile,
                Permission.UpdateOwnProfile,
                Permission.ChangeOwnPassword,
                
                // All quadratic results permissions
                Permission.ViewQuadraticResults,
                Permission.CreateQuadraticResult,
                Permission.UpdateQuadraticResult,
                Permission.DeleteQuadraticResult,
                Permission.ViewOwnQuadraticResults,
                Permission.UpdateOwnQuadraticResults,
                Permission.DeleteOwnQuadraticResults,
                
                // Administrative permissions
                Permission.ManageRoles,
                Permission.ViewSystemLogs,
                Permission.ManageSystemSettings,
                Permission.UnlockUserAccounts,
                Permission.ResetUserPasswords,
                Permission.ViewUserStatistics,
                
                // Security permissions
                Permission.ViewSecurityLogs,
                Permission.ManageSecuritySettings
            }
        );

        /// <summary>
        /// Gets all available roles
        /// </summary>
        public static List<Role> AllRoles => new List<Role>
        {
            UserRole,
            AdminRole
        };

        /// <summary>
        /// Gets a role by its type
        /// </summary>
        /// <param name="roleType">Role type</param>
        /// <returns>Role object or null if not found</returns>
        public static Role? GetRole(UserRole roleType)
        {
            return AllRoles.FirstOrDefault(r => r.RoleType == roleType);
        }

        /// <summary>
        /// Gets permissions for a specific role type
        /// </summary>
        /// <param name="roleType">Role type</param>
        /// <returns>List of permissions</returns>
        public static List<Permission> GetPermissions(UserRole roleType)
        {
            var role = GetRole(roleType);
            return role?.Permissions ?? new List<Permission>();
        }

        /// <summary>
        /// Checks if a role type has a specific permission
        /// </summary>
        /// <param name="roleType">Role type</param>
        /// <param name="permission">Permission to check</param>
        /// <returns>True if role has the permission</returns>
        public static bool HasPermission(UserRole roleType, Permission permission)
        {
            var role = GetRole(roleType);
            return role?.HasPermission(permission) ?? false;
        }
    }

    /// <summary>
    /// User session information including role and permissions
    /// </summary>
    public class UserSession
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime LastActivity { get; set; }
        public bool IsActive { get; set; }

        private Role? _roleDefinition;

        public UserSession()
        {
            LoginTime = DateTime.Now;
            LastActivity = DateTime.Now;
            IsActive = true;
        }

        public UserSession(User user) : this()
        {
            UserId = user.Id;
            Email = user.Email;
            FullName = user.FullName;
            Role = user.Role;
        }

        /// <summary>
        /// Gets the role definition with permissions
        /// </summary>
        public Role RoleDefinition
        {
            get
            {
                if (_roleDefinition == null)
                {
                    _roleDefinition = DefaultRoles.GetRole(Role);
                }
                return _roleDefinition ?? DefaultRoles.UserRole;
            }
        }

        /// <summary>
        /// Checks if the user has a specific permission
        /// </summary>
        /// <param name="permission">Permission to check</param>
        /// <returns>True if user has the permission</returns>
        public bool HasPermission(Permission permission)
        {
            return IsActive && RoleDefinition.HasPermission(permission);
        }

        /// <summary>
        /// Checks if the user has any of the specified permissions
        /// </summary>
        /// <param name="permissions">Permissions to check</param>
        /// <returns>True if user has any of the permissions</returns>
        public bool HasAnyPermission(params Permission[] permissions)
        {
            return IsActive && RoleDefinition.HasAnyPermission(permissions);
        }

        /// <summary>
        /// Checks if the user has all of the specified permissions
        /// </summary>
        /// <param name="permissions">Permissions to check</param>
        /// <returns>True if user has all of the permissions</returns>
        public bool HasAllPermissions(params Permission[] permissions)
        {
            return IsActive && RoleDefinition.HasAllPermissions(permissions);
        }

        /// <summary>
        /// Checks if the user is an administrator
        /// </summary>
        public bool IsAdmin => Role == UserRole.Admin;

        /// <summary>
        /// Checks if the user is a standard user
        /// </summary>
        public bool IsUser => Role == UserRole.User;

        /// <summary>
        /// Updates the last activity timestamp
        /// </summary>
        public void UpdateActivity()
        {
            LastActivity = DateTime.Now;
        }

        /// <summary>
        /// Invalidates the session
        /// </summary>
        public void Invalidate()
        {
            IsActive = false;
        }

        public override string ToString()
        {
            return $"{FullName} ({Role}) - {(IsActive ? "Active" : "Inactive")}";
        }
    }
}