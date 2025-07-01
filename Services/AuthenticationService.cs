using System;
using System.Configuration;
using UserCRUD.Models;
using UserCRUD.DAL;

namespace UserCRUD.Services
{
    /// <summary>
    /// Represents the result of a login attempt
    /// </summary>
    public enum LoginResult
    {
        Success,
        InvalidCredentials,
        UserNotFound,
        UserInactive,
        UserLocked,
        DatabaseError,
        InvalidInput
    }

    /// <summary>
    /// Contains the result of a login attempt with additional information
    /// </summary>
    public class LoginResponse
    {
        public LoginResult Result { get; set; }
        public User? User { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime? LockoutExpiry { get; set; }
        public int RemainingAttempts { get; set; }

        public bool IsSuccess => Result == LoginResult.Success;
    }

    /// <summary>
    /// Service class for handling user authentication and login logic
    /// </summary>
    public class AuthenticationService
    {
        private readonly UserDAL _userDAL;
        private const int MaxLoginAttempts = 5;
        private const int LockoutMinutes = 15;

        public AuthenticationService()
        {
            _userDAL = new UserDAL();
        }

        public AuthenticationService(string connectionString)
        {
            _userDAL = new UserDAL(connectionString);
        }

        /// <summary>
        /// Authenticates a user with email and password
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="password">User's password</param>
        /// <returns>LoginResponse containing the result and user information</returns>
        public LoginResponse Login(string email, string password)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return new LoginResponse
                {
                    Result = LoginResult.InvalidInput,
                    Message = "Email and password are required."
                };
            }

            try
            {
                // Get user by email
                var user = _userDAL.GetUserByEmail(email);
                if (user == null)
                {
                    return new LoginResponse
                    {
                        Result = LoginResult.UserNotFound,
                        Message = "Invalid email or password."
                    };
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    return new LoginResponse
                    {
                        Result = LoginResult.UserInactive,
                        Message = "Your account has been deactivated. Please contact an administrator."
                    };
                }

                // Check if user is locked
                if (user.IsLocked)
                {
                    return new LoginResponse
                    {
                        Result = LoginResult.UserLocked,
                        Message = $"Your account is locked until {user.LockedUntil:yyyy-MM-dd HH:mm}. Please try again later.",
                        LockoutExpiry = user.LockedUntil
                    };
                }

                // Verify password
                if (!user.VerifyPassword(password))
                {
                    // Increment failed login attempts
                    user.IncrementLoginAttempts();
                    _userDAL.UpdateLoginAttempts(user.Id, user.LoginAttempts, user.LockedUntil);

                    var remainingAttempts = MaxLoginAttempts - user.LoginAttempts;
                    var message = remainingAttempts > 0 
                        ? $"Invalid email or password. {remainingAttempts} attempts remaining."
                        : "Account locked due to too many failed login attempts. Please try again later.";

                    return new LoginResponse
                    {
                        Result = LoginResult.InvalidCredentials,
                        Message = message,
                        RemainingAttempts = Math.Max(0, remainingAttempts),
                        LockoutExpiry = user.LockedUntil
                    };
                }

                // Successful login
                user.ResetLoginAttempts();
                user.UpdateLastLogin();
                _userDAL.UpdateLoginSuccess(user.Id, user.LastLoginDate.Value);

                return new LoginResponse
                {
                    Result = LoginResult.Success,
                    User = user,
                    Message = "Login successful."
                };
            }
            catch (Exception ex)
            {
                // Log the exception (in a real application, use a logging framework)
                return new LoginResponse
                {
                    Result = LoginResult.DatabaseError,
                    Message = "An error occurred during login. Please try again later."
                };
            }
        }

        /// <summary>
        /// Changes a user's password
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="currentPassword">Current password</param>
        /// <param name="newPassword">New password</param>
        /// <returns>True if password was changed successfully</returns>
        public bool ChangePassword(int userId, string currentPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
                return false;

            try
            {
                var user = _userDAL.GetUserById(userId);
                if (user == null || !user.IsActive)
                    return false;

                // Verify current password
                if (!user.VerifyPassword(currentPassword))
                    return false;

                // Set new password
                user.SetPassword(newPassword);
                return _userDAL.UpdateUserPassword(user.Id, user.PasswordHash, user.Salt);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Resets a user's password (admin function)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="newPassword">New password</param>
        /// <returns>True if password was reset successfully</returns>
        public bool ResetPassword(int userId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                return false;

            try
            {
                var user = _userDAL.GetUserById(userId);
                if (user == null)
                    return false;

                user.SetPassword(newPassword);
                user.ResetLoginAttempts(); // Unlock account when password is reset
                
                return _userDAL.UpdateUserPassword(user.Id, user.PasswordHash, user.Salt) &&
                       _userDAL.UpdateLoginAttempts(user.Id, 0, null);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Unlocks a user account (admin function)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>True if account was unlocked successfully</returns>
        public bool UnlockAccount(int userId)
        {
            try
            {
                return _userDAL.UpdateLoginAttempts(userId, 0, null);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates password strength
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <returns>True if password meets requirements</returns>
        public static bool ValidatePasswordStrength(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            // Minimum 8 characters
            if (password.Length < 8)
                return false;

            // Must contain at least one uppercase letter
            bool hasUpper = false;
            // Must contain at least one lowercase letter
            bool hasLower = false;
            // Must contain at least one digit
            bool hasDigit = false;
            // Must contain at least one special character
            bool hasSpecial = false;

            foreach (char c in password)
            {
                if (char.IsUpper(c)) hasUpper = true;
                else if (char.IsLower(c)) hasLower = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else if (!char.IsLetterOrDigit(c)) hasSpecial = true;
            }

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        /// <summary>
        /// Gets password strength requirements as a string
        /// </summary>
        /// <returns>Password requirements description</returns>
        public static string GetPasswordRequirements()
        {
            return "Password must be at least 8 characters long and contain:\n" +
                   "• At least one uppercase letter (A-Z)\n" +
                   "• At least one lowercase letter (a-z)\n" +
                   "• At least one digit (0-9)\n" +
                   "• At least one special character (!@#$%^&*()_+-=[]{}|;:,.<>?)";
        }

        /// <summary>
        /// Tests the database connection
        /// </summary>
        /// <returns>True if connection is successful</returns>
        public bool TestConnection()
        {
            return _userDAL.TestConnection();
        }
    }
}