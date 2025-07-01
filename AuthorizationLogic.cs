using System;
using System.Collections.Generic;
using System.Linq;
using UserCRUD.Models;
using UserCRUD.Services;
using UserCRUD.DAL;

namespace UserCRUD
{
    /// <summary>
    /// Core authorization logic implementation for Task 3.3
    /// Implements role-based authorization for User/Admin roles
    /// </summary>
    public static class AuthorizationLogic
    {
        #region Core Authorization Functions

        /// <summary>
        /// Main authorization function - checks if user has permission for an action
        /// This is the core function for Task 3.3
        /// </summary>
        /// <param name="userId">User ID performing the action</param>
        /// <param name="requiredPermission">Permission required for the action</param>
        /// <param name="resourceOwnerId">ID of the resource owner (optional, for resource-specific checks)</param>
        /// <returns>Tuple containing authorization result, user role, and message</returns>
        public static (bool IsAuthorized, UserRole? UserRole, string Message) CheckAuthorization(
            int userId, 
            Permission requiredPermission, 
            int? resourceOwnerId = null)
        {
            try
            {
                var authService = new AuthorizationService();
                
                // Get user session
                var session = authService.GetSession(userId);
                if (session == null)
                {
                    return (false, null, "User session not found. Please log in.");
                }

                // Check if user is active
                if (!session.IsActive)
                {
                    return (false, session.Role, "User account is inactive.");
                }

                // For resource-specific permissions, check ownership
                if (resourceOwnerId.HasValue && resourceOwnerId.Value != userId)
                {
                    // Only admins can access other users' resources
                    if (!session.IsAdmin)
                    {
                        return (false, session.Role, "Access denied. Users can only access their own resources.");
                    }
                }

                // Check if user has the required permission
                if (session.HasPermission(requiredPermission))
                {
                    return (true, session.Role, "Authorization granted.");
                }

                return (false, session.Role, $"Insufficient permissions. Required: {requiredPermission}");
            }
            catch (Exception ex)
            {
                return (false, null, $"Authorization check failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if user has admin role
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Tuple containing result and message</returns>
        public static (bool IsAdmin, string Message) CheckAdminRole(int userId)
        {
            var (isAuthorized, userRole, message) = CheckAuthorization(userId, Permission.ManageRoles);
            
            if (userRole == UserRole.Admin)
            {
                return (true, "User has admin privileges.");
            }

            return (false, userRole.HasValue ? $"User role is {userRole}, admin required." : "User role not found.");
        }

        /// <summary>
        /// Checks if user can perform CRUD operations on another user
        /// </summary>
        /// <param name="currentUserId">Current user ID</param>
        /// <param name="targetUserId">Target user ID</param>
        /// <param name="operation">CRUD operation (Create, Read, Update, Delete)</param>
        /// <returns>Authorization result</returns>
        public static (bool IsAuthorized, string Message) CheckUserCRUDPermission(
            int currentUserId, 
            int targetUserId, 
            string operation)
        {
            var authService = new AuthorizationService();
            var session = authService.GetSession(currentUserId);
            
            if (session == null)
            {
                return (false, "User session not found.");
            }

            // Users can always access their own data
            if (currentUserId == targetUserId)
            {
                switch (operation.ToLower())
                {
                    case "read":
                        return session.HasPermission(Permission.ViewOwnProfile) 
                            ? (true, "User can view own profile.")
                            : (false, "Permission denied to view own profile.");
                    
                    case "update":
                        return session.HasPermission(Permission.UpdateOwnProfile)
                            ? (true, "User can update own profile.")
                            : (false, "Permission denied to update own profile.");
                    
                    case "delete":
                        // Users typically cannot delete their own accounts
                        return (false, "Users cannot delete their own accounts.");
                    
                    default:
                        return (false, $"Unknown operation: {operation}");
                }
            }

            // For other users, admin privileges required
            if (!session.IsAdmin)
            {
                return (false, "Admin privileges required to access other users' data.");
            }

            // Check admin permissions for the operation
            switch (operation.ToLower())
            {
                case "create":
                    return session.HasPermission(Permission.CreateUser)
                        ? (true, "Admin can create users.")
                        : (false, "Permission denied to create users.");
                
                case "read":
                    return session.HasPermission(Permission.ViewUsers)
                        ? (true, "Admin can view users.")
                        : (false, "Permission denied to view users.");
                
                case "update":
                    return session.HasPermission(Permission.UpdateUser)
                        ? (true, "Admin can update users.")
                        : (false, "Permission denied to update users.");
                
                case "delete":
                    return session.HasPermission(Permission.DeleteUser)
                        ? (true, "Admin can delete users.")
                        : (false, "Permission denied to delete users.");
                
                default:
                    return (false, $"Unknown operation: {operation}");
            }
        }

        /// <summary>
        /// Checks if user can perform operations on quadratic results
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="quadraticResultId">Quadratic result ID</param>
        /// <param name="operation">Operation to perform</param>
        /// <returns>Authorization result</returns>
        public static (bool IsAuthorized, string Message) CheckQuadraticResultPermission(
            int userId, 
            int quadraticResultId, 
            string operation)
        {
            var authService = new AuthorizationService();
            var response = authService.CheckQuadraticResultAccess(userId, quadraticResultId, operation);
            
            return (response.IsGranted, response.Message);
        }

        #endregion

        #region Role-Based Access Control

        /// <summary>
        /// Gets all permissions for a user role
        /// </summary>
        /// <param name="role">User role</param>
        /// <returns>List of permissions</returns>
        public static List<Permission> GetRolePermissions(UserRole role)
        {
            return DefaultRoles.GetPermissions(role);
        }

        /// <summary>
        /// Checks if a role has a specific permission
        /// </summary>
        /// <param name="role">User role</param>
        /// <param name="permission">Permission to check</param>
        /// <returns>True if role has permission</returns>
        public static bool RoleHasPermission(UserRole role, Permission permission)
        {
            return DefaultRoles.HasPermission(role, permission);
        }

        /// <summary>
        /// Gets the minimum role required for a permission
        /// </summary>
        /// <param name="permission">Permission</param>
        /// <returns>Minimum required role</returns>
        public static UserRole GetMinimumRoleForPermission(Permission permission)
        {
            if (DefaultRoles.HasPermission(UserRole.User, permission))
            {
                return UserRole.User;
            }
            
            if (DefaultRoles.HasPermission(UserRole.Admin, permission))
            {
                return UserRole.Admin;
            }

            // If no role has the permission, return Admin as safest default
            return UserRole.Admin;
        }

        #endregion

        #region Security Validation

        /// <summary>
        /// Validates that a user can perform a sensitive operation
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="operation">Sensitive operation</param>
        /// <param name="targetUserId">Target user ID (optional)</param>
        /// <returns>Validation result</returns>
        public static (bool IsValid, string Message) ValidateSensitiveOperation(
            int userId, 
            string operation, 
            int? targetUserId = null)
        {
            var authService = new AuthorizationService();
            var session = authService.GetSession(userId);
            
            if (session == null)
            {
                return (false, "User session not found.");
            }

            if (!session.IsActive)
            {
                return (false, "User account is inactive.");
            }

            // Define sensitive operations and their required permissions
            var sensitiveOperations = new Dictionary<string, Permission[]>
            {
                ["delete_user"] = new[] { Permission.DeleteUser },
                ["deactivate_user"] = new[] { Permission.DeactivateUser },
                ["manage_roles"] = new[] { Permission.ManageRoles },
                ["reset_password"] = new[] { Permission.ResetUserPasswords },
                ["unlock_account"] = new[] { Permission.UnlockUserAccounts },
                ["view_security_logs"] = new[] { Permission.ViewSecurityLogs },
                ["manage_security_settings"] = new[] { Permission.ManageSecuritySettings }
            };

            if (!sensitiveOperations.TryGetValue(operation.ToLower(), out var requiredPermissions))
            {
                return (false, $"Unknown sensitive operation: {operation}");
            }

            // Check if user has any of the required permissions
            if (!session.HasAnyPermission(requiredPermissions))
            {
                return (false, $"Insufficient permissions for {operation}. Required: {string.Join(" or ", requiredPermissions)}");
            }

            // Additional validation for operations on other users
            if (targetUserId.HasValue && targetUserId.Value != userId)
            {
                // Prevent users from performing sensitive operations on admins (unless they are also admin)
                if (!session.IsAdmin)
                {
                    return (false, "Only administrators can perform sensitive operations on other users.");
                }

                // Prevent admins from deleting themselves
                if (operation.ToLower() == "delete_user" && targetUserId.Value == userId)
                {
                    return (false, "Users cannot delete their own accounts.");
                }
            }

            return (true, $"User authorized for {operation}.");
        }

        /// <summary>
        /// Checks if user can escalate privileges
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="targetRole">Target role to escalate to</param>
        /// <returns>Escalation result</returns>
        public static (bool CanEscalate, string Message) CheckPrivilegeEscalation(
            int userId, 
            UserRole targetRole)
        {
            var authService = new AuthorizationService();
            var session = authService.GetSession(userId);
            
            if (session == null)
            {
                return (false, "User session not found.");
            }

            // Only admins can manage roles
            if (!session.IsAdmin)
            {
                return (false, "Only administrators can change user roles.");
            }

            // Check if user has role management permission
            if (!session.HasPermission(Permission.ManageRoles))
            {
                return (false, "Insufficient permissions to manage roles.");
            }

            // Prevent privilege escalation beyond current user's level
            if (targetRole > session.Role)
            {
                return (false, "Cannot escalate privileges beyond your own role level.");
            }

            return (true, $"User can assign {targetRole} role.");
        }

        #endregion

        #region Utility Functions

        /// <summary>
        /// Gets user's current permissions
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of user's permissions</returns>
        public static List<Permission> GetUserPermissions(int userId)
        {
            var authService = new AuthorizationService();
            var session = authService.GetSession(userId);
            
            return session?.RoleDefinition.Permissions ?? new List<Permission>();
        }

        /// <summary>
        /// Checks if user is currently logged in and active
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>True if user is active</returns>
        public static bool IsUserActive(int userId)
        {
            var authService = new AuthorizationService();
            var session = authService.GetSession(userId);
            
            return session?.IsActive ?? false;
        }

        /// <summary>
        /// Gets user's role information
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Role information or null if not found</returns>
        public static (UserRole Role, string RoleName, int PermissionCount)? GetUserRoleInfo(int userId)
        {
            var authService = new AuthorizationService();
            var session = authService.GetSession(userId);
            
            if (session == null)
                return null;

            var roleDefinition = session.RoleDefinition;
            return (session.Role, roleDefinition.Name, roleDefinition.Permissions.Count);
        }

        /// <summary>
        /// Logs authorization events (for audit purposes)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="action">Action performed</param>
        /// <param name="resource">Resource accessed</param>
        /// <param name="result">Authorization result</param>
        public static void LogAuthorizationEvent(int userId, string action, string resource, bool result)
        {
            // In a real application, this would write to a security log
            var timestamp = DateTime.Now;
            var logEntry = $"[{timestamp:yyyy-MM-dd HH:mm:ss}] User {userId} {(result ? "GRANTED" : "DENIED")} {action} on {resource}";
            
            // For demonstration, we'll just use Console.WriteLine
            // In production, use a proper logging framework
            Console.WriteLine($"SECURITY LOG: {logEntry}");
        }

        #endregion

        #region Example Usage Functions

        /// <summary>
        /// Example: Check if user can view all users (admin function)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Authorization result</returns>
        public static bool CanViewAllUsers(int userId)
        {
            var (isAuthorized, _, _) = CheckAuthorization(userId, Permission.ViewUsers);
            LogAuthorizationEvent(userId, "VIEW_ALL_USERS", "Users", isAuthorized);
            return isAuthorized;
        }

        /// <summary>
        /// Example: Check if user can create new users (admin function)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Authorization result</returns>
        public static bool CanCreateUser(int userId)
        {
            var (isAuthorized, _, _) = CheckAuthorization(userId, Permission.CreateUser);
            LogAuthorizationEvent(userId, "CREATE_USER", "Users", isAuthorized);
            return isAuthorized;
        }

        /// <summary>
        /// Example: Check if user can delete another user (admin function)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="targetUserId">Target user to delete</param>
        /// <returns>Authorization result</returns>
        public static bool CanDeleteUser(int userId, int targetUserId)
        {
            // Prevent self-deletion
            if (userId == targetUserId)
            {
                LogAuthorizationEvent(userId, "DELETE_USER", $"User {targetUserId}", false);
                return false;
            }

            var (isAuthorized, _, _) = CheckAuthorization(userId, Permission.DeleteUser);
            LogAuthorizationEvent(userId, "DELETE_USER", $"User {targetUserId}", isAuthorized);
            return isAuthorized;
        }

        /// <summary>
        /// Example: Check if user can manage system settings (admin function)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Authorization result</returns>
        public static bool CanManageSystemSettings(int userId)
        {
            var (isAuthorized, _, _) = CheckAuthorization(userId, Permission.ManageSystemSettings);
            LogAuthorizationEvent(userId, "MANAGE_SYSTEM_SETTINGS", "System", isAuthorized);
            return isAuthorized;
        }

        /// <summary>
        /// Example: Check if user can view their own profile (user function)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Authorization result</returns>
        public static bool CanViewOwnProfile(int userId)
        {
            var (isAuthorized, _, _) = CheckAuthorization(userId, Permission.ViewOwnProfile, userId);
            LogAuthorizationEvent(userId, "VIEW_OWN_PROFILE", $"User {userId}", isAuthorized);
            return isAuthorized;
        }

        #endregion
    }

    /// <summary>
    /// Authorization attributes for method-level security
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequirePermissionAttribute : Attribute
    {
        public Permission RequiredPermission { get; }
        public string Description { get; set; } = string.Empty;

        public RequirePermissionAttribute(Permission requiredPermission)
        {
            RequiredPermission = requiredPermission;
        }

        /// <summary>
        /// Validates that the current user has the required permission
        /// </summary>
        /// <param name="userId">User ID to check</param>
        /// <returns>True if user has permission</returns>
        public bool ValidatePermission(int userId)
        {
            var (isAuthorized, _, _) = AuthorizationLogic.CheckAuthorization(userId, RequiredPermission);
            return isAuthorized;
        }
    }

    /// <summary>
    /// Authorization attribute for admin-only methods
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequireAdminAttribute : Attribute
    {
        public string Description { get; set; } = "Administrator privileges required";

        /// <summary>
        /// Validates that the current user is an administrator
        /// </summary>
        /// <param name="userId">User ID to check</param>
        /// <returns>True if user is admin</returns>
        public bool ValidateAdmin(int userId)
        {
            var (isAdmin, _) = AuthorizationLogic.CheckAdminRole(userId);
            return isAdmin;
        }
    }
}