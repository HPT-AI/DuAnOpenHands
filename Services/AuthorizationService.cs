using System;
using System.Collections.Generic;
using System.Linq;
using UserCRUD.Models;
using UserCRUD.DAL;

namespace UserCRUD.Services
{
    /// <summary>
    /// Result of an authorization check
    /// </summary>
    public enum AuthorizationResult
    {
        Granted,
        Denied,
        UserNotFound,
        UserInactive,
        SessionExpired,
        InsufficientPermissions
    }

    /// <summary>
    /// Authorization response with detailed information
    /// </summary>
    public class AuthorizationResponse
    {
        public AuthorizationResult Result { get; set; }
        public string Message { get; set; } = string.Empty;
        public Permission? RequiredPermission { get; set; }
        public UserRole? UserRole { get; set; }
        public bool IsGranted => Result == AuthorizationResult.Granted;

        public AuthorizationResponse(AuthorizationResult result, string message = "")
        {
            Result = result;
            Message = message;
        }
    }

    /// <summary>
    /// Service for handling role-based authorization and access control
    /// </summary>
    public class AuthorizationService
    {
        private readonly UserDAL _userDAL;
        private static readonly Dictionary<int, UserSession> _activeSessions = new Dictionary<int, UserSession>();
        private static readonly TimeSpan SessionTimeout = TimeSpan.FromHours(8); // 8-hour session timeout

        public AuthorizationService()
        {
            _userDAL = new UserDAL();
        }

        public AuthorizationService(string connectionString)
        {
            _userDAL = new UserDAL(connectionString);
        }

        #region Session Management

        /// <summary>
        /// Creates a new user session after successful authentication
        /// </summary>
        /// <param name="user">Authenticated user</param>
        /// <returns>User session</returns>
        public UserSession CreateSession(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var session = new UserSession(user);
            
            // Remove any existing session for this user
            RemoveUserSessions(user.Id);
            
            // Add new session
            _activeSessions[user.Id] = session;
            
            return session;
        }

        /// <summary>
        /// Gets the current session for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User session or null if not found</returns>
        public UserSession? GetSession(int userId)
        {
            if (_activeSessions.TryGetValue(userId, out var session))
            {
                // Check if session is expired
                if (DateTime.Now - session.LastActivity > SessionTimeout)
                {
                    RemoveSession(userId);
                    return null;
                }

                session.UpdateActivity();
                return session;
            }

            return null;
        }

        /// <summary>
        /// Removes a user session (logout)
        /// </summary>
        /// <param name="userId">User ID</param>
        public void RemoveSession(int userId)
        {
            if (_activeSessions.ContainsKey(userId))
            {
                _activeSessions[userId].Invalidate();
                _activeSessions.Remove(userId);
            }
        }

        /// <summary>
        /// Removes all sessions for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        public void RemoveUserSessions(int userId)
        {
            RemoveSession(userId);
        }

        /// <summary>
        /// Gets all active sessions (admin only)
        /// </summary>
        /// <returns>List of active sessions</returns>
        public List<UserSession> GetActiveSessions()
        {
            // Clean up expired sessions
            var expiredSessions = _activeSessions.Values
                .Where(s => DateTime.Now - s.LastActivity > SessionTimeout)
                .ToList();

            foreach (var session in expiredSessions)
            {
                RemoveSession(session.UserId);
            }

            return _activeSessions.Values.ToList();
        }

        #endregion

        #region Permission Checking

        /// <summary>
        /// Checks if a user has a specific permission
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="permission">Required permission</param>
        /// <returns>Authorization response</returns>
        public AuthorizationResponse CheckPermission(int userId, Permission permission)
        {
            try
            {
                // Get user session
                var session = GetSession(userId);
                if (session == null)
                {
                    return new AuthorizationResponse(AuthorizationResult.SessionExpired, 
                        "Session expired. Please log in again.");
                }

                // Check if user is active
                if (!session.IsActive)
                {
                    return new AuthorizationResponse(AuthorizationResult.UserInactive, 
                        "User account is inactive.");
                }

                // Check permission
                if (session.HasPermission(permission))
                {
                    return new AuthorizationResponse(AuthorizationResult.Granted, 
                        "Permission granted.")
                    {
                        RequiredPermission = permission,
                        UserRole = session.Role
                    };
                }

                return new AuthorizationResponse(AuthorizationResult.InsufficientPermissions, 
                    $"Insufficient permissions. Required: {permission}")
                {
                    RequiredPermission = permission,
                    UserRole = session.Role
                };
            }
            catch (Exception ex)
            {
                return new AuthorizationResponse(AuthorizationResult.Denied, 
                    $"Authorization check failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if a user has any of the specified permissions
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="permissions">Required permissions (any)</param>
        /// <returns>Authorization response</returns>
        public AuthorizationResponse CheckAnyPermission(int userId, params Permission[] permissions)
        {
            var session = GetSession(userId);
            if (session == null)
            {
                return new AuthorizationResponse(AuthorizationResult.SessionExpired, 
                    "Session expired. Please log in again.");
            }

            if (!session.IsActive)
            {
                return new AuthorizationResponse(AuthorizationResult.UserInactive, 
                    "User account is inactive.");
            }

            if (session.HasAnyPermission(permissions))
            {
                return new AuthorizationResponse(AuthorizationResult.Granted, 
                    "Permission granted.")
                {
                    UserRole = session.Role
                };
            }

            return new AuthorizationResponse(AuthorizationResult.InsufficientPermissions, 
                $"Insufficient permissions. Required any of: {string.Join(", ", permissions)}")
            {
                UserRole = session.Role
            };
        }

        /// <summary>
        /// Checks if a user has all of the specified permissions
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="permissions">Required permissions (all)</param>
        /// <returns>Authorization response</returns>
        public AuthorizationResponse CheckAllPermissions(int userId, params Permission[] permissions)
        {
            var session = GetSession(userId);
            if (session == null)
            {
                return new AuthorizationResponse(AuthorizationResult.SessionExpired, 
                    "Session expired. Please log in again.");
            }

            if (!session.IsActive)
            {
                return new AuthorizationResponse(AuthorizationResult.UserInactive, 
                    "User account is inactive.");
            }

            if (session.HasAllPermissions(permissions))
            {
                return new AuthorizationResponse(AuthorizationResult.Granted, 
                    "Permission granted.")
                {
                    UserRole = session.Role
                };
            }

            return new AuthorizationResponse(AuthorizationResult.InsufficientPermissions, 
                $"Insufficient permissions. Required all of: {string.Join(", ", permissions)}")
            {
                UserRole = session.Role
            };
        }

        #endregion

        #region Role Checking

        /// <summary>
        /// Checks if a user has a specific role
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="requiredRole">Required role</param>
        /// <returns>Authorization response</returns>
        public AuthorizationResponse CheckRole(int userId, UserRole requiredRole)
        {
            var session = GetSession(userId);
            if (session == null)
            {
                return new AuthorizationResponse(AuthorizationResult.SessionExpired, 
                    "Session expired. Please log in again.");
            }

            if (!session.IsActive)
            {
                return new AuthorizationResponse(AuthorizationResult.UserInactive, 
                    "User account is inactive.");
            }

            if (session.Role == requiredRole)
            {
                return new AuthorizationResponse(AuthorizationResult.Granted, 
                    $"User has required role: {requiredRole}")
                {
                    UserRole = session.Role
                };
            }

            return new AuthorizationResponse(AuthorizationResult.InsufficientPermissions, 
                $"Insufficient role. Required: {requiredRole}, Current: {session.Role}")
            {
                UserRole = session.Role
            };
        }

        /// <summary>
        /// Checks if a user is an administrator
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Authorization response</returns>
        public AuthorizationResponse CheckAdminRole(int userId)
        {
            return CheckRole(userId, UserRole.Admin);
        }

        /// <summary>
        /// Checks if a user can access another user's data
        /// </summary>
        /// <param name="currentUserId">Current user ID</param>
        /// <param name="targetUserId">Target user ID</param>
        /// <returns>Authorization response</returns>
        public AuthorizationResponse CheckUserDataAccess(int currentUserId, int targetUserId)
        {
            var session = GetSession(currentUserId);
            if (session == null)
            {
                return new AuthorizationResponse(AuthorizationResult.SessionExpired, 
                    "Session expired. Please log in again.");
            }

            // Users can always access their own data
            if (currentUserId == targetUserId)
            {
                return new AuthorizationResponse(AuthorizationResult.Granted, 
                    "User can access own data.");
            }

            // Admins can access any user's data
            if (session.IsAdmin)
            {
                return new AuthorizationResponse(AuthorizationResult.Granted, 
                    "Admin can access any user data.");
            }

            return new AuthorizationResponse(AuthorizationResult.InsufficientPermissions, 
                "Users can only access their own data.");
        }

        #endregion

        #region Resource-Specific Authorization

        /// <summary>
        /// Checks if a user can perform an action on a quadratic result
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="quadraticResultId">Quadratic result ID</param>
        /// <param name="action">Action to perform</param>
        /// <returns>Authorization response</returns>
        public AuthorizationResponse CheckQuadraticResultAccess(int userId, int quadraticResultId, string action)
        {
            var session = GetSession(userId);
            if (session == null)
            {
                return new AuthorizationResponse(AuthorizationResult.SessionExpired, 
                    "Session expired. Please log in again.");
            }

            try
            {
                // Get the quadratic result to check ownership
                var quadraticDAL = new QuadraticResultDAL();
                var result = quadraticDAL.GetQuadraticResultById(quadraticResultId);
                
                if (result == null)
                {
                    return new AuthorizationResponse(AuthorizationResult.Denied, 
                        "Quadratic result not found.");
                }

                // Check permissions based on action
                switch (action.ToLower())
                {
                    case "view":
                        if (session.IsAdmin || session.HasPermission(Permission.ViewQuadraticResults))
                        {
                            return new AuthorizationResponse(AuthorizationResult.Granted, 
                                "Permission granted to view quadratic result.");
                        }
                        if (result.UserId == userId && session.HasPermission(Permission.ViewOwnQuadraticResults))
                        {
                            return new AuthorizationResponse(AuthorizationResult.Granted, 
                                "Permission granted to view own quadratic result.");
                        }
                        break;

                    case "update":
                        if (session.IsAdmin || session.HasPermission(Permission.UpdateQuadraticResult))
                        {
                            return new AuthorizationResponse(AuthorizationResult.Granted, 
                                "Permission granted to update quadratic result.");
                        }
                        if (result.UserId == userId && session.HasPermission(Permission.UpdateOwnQuadraticResults))
                        {
                            return new AuthorizationResponse(AuthorizationResult.Granted, 
                                "Permission granted to update own quadratic result.");
                        }
                        break;

                    case "delete":
                        if (session.IsAdmin || session.HasPermission(Permission.DeleteQuadraticResult))
                        {
                            return new AuthorizationResponse(AuthorizationResult.Granted, 
                                "Permission granted to delete quadratic result.");
                        }
                        if (result.UserId == userId && session.HasPermission(Permission.DeleteOwnQuadraticResults))
                        {
                            return new AuthorizationResponse(AuthorizationResult.Granted, 
                                "Permission granted to delete own quadratic result.");
                        }
                        break;
                }

                return new AuthorizationResponse(AuthorizationResult.InsufficientPermissions, 
                    $"Insufficient permissions to {action} quadratic result.");
            }
            catch (Exception ex)
            {
                return new AuthorizationResponse(AuthorizationResult.Denied, 
                    $"Authorization check failed: {ex.Message}");
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets the current user's role
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User role or null if session not found</returns>
        public UserRole? GetUserRole(int userId)
        {
            var session = GetSession(userId);
            return session?.Role;
        }

        /// <summary>
        /// Checks if a user is currently logged in
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>True if user has an active session</returns>
        public bool IsUserLoggedIn(int userId)
        {
            return GetSession(userId) != null;
        }

        /// <summary>
        /// Updates a user's role (admin only)
        /// </summary>
        /// <param name="adminUserId">Admin user ID</param>
        /// <param name="targetUserId">Target user ID</param>
        /// <param name="newRole">New role</param>
        /// <returns>True if role was updated successfully</returns>
        public bool UpdateUserRole(int adminUserId, int targetUserId, UserRole newRole)
        {
            // Check admin permissions
            var authResult = CheckPermission(adminUserId, Permission.ManageRoles);
            if (!authResult.IsGranted)
            {
                return false;
            }

            try
            {
                // Update role in database
                var success = _userDAL.UpdateUserRole(targetUserId, newRole);
                
                if (success)
                {
                    // Update active session if user is logged in
                    var targetSession = GetSession(targetUserId);
                    if (targetSession != null)
                    {
                        targetSession.Role = newRole;
                    }
                }

                return success;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates that a user can perform a specific operation
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="operation">Operation name</param>
        /// <param name="resourceId">Resource ID (optional)</param>
        /// <returns>True if operation is allowed</returns>
        public bool ValidateOperation(int userId, string operation, int? resourceId = null)
        {
            var session = GetSession(userId);
            if (session == null || !session.IsActive)
            {
                return false;
            }

            // Map operations to permissions
            var permissionMap = new Dictionary<string, Permission[]>
            {
                ["create_user"] = new[] { Permission.CreateUser },
                ["view_users"] = new[] { Permission.ViewUsers },
                ["update_user"] = new[] { Permission.UpdateUser },
                ["delete_user"] = new[] { Permission.DeleteUser },
                ["manage_roles"] = new[] { Permission.ManageRoles },
                ["view_system_logs"] = new[] { Permission.ViewSystemLogs },
                ["unlock_accounts"] = new[] { Permission.UnlockUserAccounts },
                ["reset_passwords"] = new[] { Permission.ResetUserPasswords }
            };

            if (permissionMap.TryGetValue(operation.ToLower(), out var requiredPermissions))
            {
                return session.HasAnyPermission(requiredPermissions);
            }

            // Default to deny for unknown operations
            return false;
        }

        #endregion
    }

    /// <summary>
    /// Static helper class for common authorization checks
    /// </summary>
    public static class AuthorizationHelper
    {
        private static readonly AuthorizationService _authService = new AuthorizationService();

        /// <summary>
        /// Quick check if user is admin
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>True if user is admin</returns>
        public static bool IsAdmin(int userId)
        {
            return _authService.CheckAdminRole(userId).IsGranted;
        }

        /// <summary>
        /// Quick check if user has permission
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="permission">Permission to check</param>
        /// <returns>True if user has permission</returns>
        public static bool HasPermission(int userId, Permission permission)
        {
            return _authService.CheckPermission(userId, permission).IsGranted;
        }

        /// <summary>
        /// Quick check if user can access another user's data
        /// </summary>
        /// <param name="currentUserId">Current user ID</param>
        /// <param name="targetUserId">Target user ID</param>
        /// <returns>True if access is allowed</returns>
        public static bool CanAccessUserData(int currentUserId, int targetUserId)
        {
            return _authService.CheckUserDataAccess(currentUserId, targetUserId).IsGranted;
        }

        /// <summary>
        /// Requires admin role or throws exception
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <exception cref="UnauthorizedAccessException">Thrown if user is not admin</exception>
        public static void RequireAdmin(int userId)
        {
            if (!IsAdmin(userId))
            {
                throw new UnauthorizedAccessException("Administrator privileges required.");
            }
        }

        /// <summary>
        /// Requires specific permission or throws exception
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="permission">Required permission</param>
        /// <exception cref="UnauthorizedAccessException">Thrown if user lacks permission</exception>
        public static void RequirePermission(int userId, Permission permission)
        {
            if (!HasPermission(userId, permission))
            {
                throw new UnauthorizedAccessException($"Permission required: {permission}");
            }
        }
    }
}