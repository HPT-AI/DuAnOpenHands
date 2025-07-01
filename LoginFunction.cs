using System;
using UserCRUD.Models;
using UserCRUD.DAL;
using UserCRUD.Services;

namespace UserCRUD
{
    /// <summary>
    /// Core login function implementation that checks password hash and IsActive status
    /// This is the main function requested for Task 3.2
    /// </summary>
    public static class LoginFunction
    {
        /// <summary>
        /// Authenticates a user by checking password hash and IsActive status
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="password">User's plain text password</param>
        /// <param name="connectionString">Database connection string (optional)</param>
        /// <returns>LoginResponse containing authentication result</returns>
        public static LoginResponse AuthenticateUser(string email, string password, string? connectionString = null)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(email))
            {
                return new LoginResponse
                {
                    Result = LoginResult.InvalidInput,
                    Message = "Email is required."
                };
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return new LoginResponse
                {
                    Result = LoginResult.InvalidInput,
                    Message = "Password is required."
                };
            }

            try
            {
                // Initialize data access layer
                var userDAL = string.IsNullOrEmpty(connectionString) 
                    ? new UserDAL() 
                    : new UserDAL(connectionString);

                // Step 1: Get user by email
                var user = userDAL.GetUserByEmail(email);
                if (user == null)
                {
                    return new LoginResponse
                    {
                        Result = LoginResult.UserNotFound,
                        Message = "Invalid email or password."
                    };
                }

                // Step 2: Check if user is active
                if (!user.IsActive)
                {
                    return new LoginResponse
                    {
                        Result = LoginResult.UserInactive,
                        Message = "Account is deactivated. Please contact an administrator."
                    };
                }

                // Step 3: Check if account is locked
                if (user.IsLocked)
                {
                    return new LoginResponse
                    {
                        Result = LoginResult.UserLocked,
                        Message = $"Account is locked until {user.LockedUntil:yyyy-MM-dd HH:mm}. Please try again later.",
                        LockoutExpiry = user.LockedUntil
                    };
                }

                // Step 4: Verify password hash
                if (!user.VerifyPassword(password))
                {
                    // Increment failed login attempts
                    user.IncrementLoginAttempts();
                    userDAL.UpdateLoginAttempts(user.Id, user.LoginAttempts, user.LockedUntil);

                    var remainingAttempts = 5 - user.LoginAttempts; // Max 5 attempts
                    var message = remainingAttempts > 0 
                        ? $"Invalid password. {remainingAttempts} attempts remaining."
                        : "Account locked due to too many failed attempts.";

                    return new LoginResponse
                    {
                        Result = LoginResult.InvalidCredentials,
                        Message = message,
                        RemainingAttempts = Math.Max(0, remainingAttempts),
                        LockoutExpiry = user.LockedUntil
                    };
                }

                // Step 5: Successful authentication
                user.ResetLoginAttempts();
                user.UpdateLastLogin();
                userDAL.UpdateLoginSuccess(user.Id, user.LastLoginDate.Value);

                return new LoginResponse
                {
                    Result = LoginResult.Success,
                    User = user,
                    Message = "Authentication successful."
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Result = LoginResult.DatabaseError,
                    Message = $"Database error: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Simple boolean login check - returns true if authentication succeeds
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="password">User's plain text password</param>
        /// <returns>True if login successful, false otherwise</returns>
        public static bool IsValidLogin(string email, string password)
        {
            var result = AuthenticateUser(email, password);
            return result.IsSuccess;
        }

        /// <summary>
        /// Gets authenticated user if login is successful
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="password">User's plain text password</param>
        /// <returns>User object if authentication successful, null otherwise</returns>
        public static User? GetAuthenticatedUser(string email, string password)
        {
            var result = AuthenticateUser(email, password);
            return result.IsSuccess ? result.User : null;
        }

        /// <summary>
        /// Validates user credentials and returns detailed status
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="password">User's plain text password</param>
        /// <param name="errorMessage">Output parameter for error message</param>
        /// <returns>True if authentication successful, false otherwise</returns>
        public static bool ValidateCredentials(string email, string password, out string errorMessage)
        {
            var result = AuthenticateUser(email, password);
            errorMessage = result.Message;
            return result.IsSuccess;
        }

        /// <summary>
        /// Core login logic implementation - the main function for Task 3.2
        /// Checks password hash and IsActive status as requested
        /// </summary>
        /// <param name="email">User's email address</param>
        /// <param name="password">User's plain text password</param>
        /// <returns>Tuple containing success status, user object, and message</returns>
        public static (bool Success, User? User, string Message) Login(string email, string password)
        {
            // This is the core function that implements the login logic
            // checking hash and IsActive as requested in Task 3.2

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return (false, null, "Email and password are required.");
            }

            try
            {
                var userDAL = new UserDAL();
                
                // Get user by email
                var user = userDAL.GetUserByEmail(email);
                if (user == null)
                {
                    return (false, null, "Invalid credentials.");
                }

                // Check IsActive status
                if (!user.IsActive)
                {
                    return (false, null, "Account is inactive.");
                }

                // Check if account is locked
                if (user.IsLocked)
                {
                    return (false, null, $"Account is locked until {user.LockedUntil:yyyy-MM-dd HH:mm}.");
                }

                // Verify password hash
                if (!user.VerifyPassword(password))
                {
                    // Update failed login attempts
                    user.IncrementLoginAttempts();
                    userDAL.UpdateLoginAttempts(user.Id, user.LoginAttempts, user.LockedUntil);
                    
                    return (false, null, "Invalid credentials.");
                }

                // Successful login - reset attempts and update last login
                user.ResetLoginAttempts();
                user.UpdateLastLogin();
                userDAL.UpdateLoginSuccess(user.Id, user.LastLoginDate.Value);

                return (true, user, "Login successful.");
            }
            catch (Exception ex)
            {
                return (false, null, $"Login error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Example usage and testing class for the login function
    /// </summary>
    public static class LoginFunctionExample
    {
        /// <summary>
        /// Demonstrates how to use the login function
        /// </summary>
        public static void ExampleUsage()
        {
            // Example 1: Simple boolean check
            bool isValid = LoginFunction.IsValidLogin("user@example.com", "password123");
            Console.WriteLine($"Login valid: {isValid}");

            // Example 2: Get authenticated user
            var user = LoginFunction.GetAuthenticatedUser("user@example.com", "password123");
            if (user != null)
            {
                Console.WriteLine($"Welcome, {user.FullName}!");
            }

            // Example 3: Detailed validation with error message
            if (LoginFunction.ValidateCredentials("user@example.com", "password123", out string errorMessage))
            {
                Console.WriteLine("Login successful!");
            }
            else
            {
                Console.WriteLine($"Login failed: {errorMessage}");
            }

            // Example 4: Core login function (main implementation for Task 3.2)
            var (success, authenticatedUser, message) = LoginFunction.Login("user@example.com", "password123");
            if (success)
            {
                Console.WriteLine($"Success: {message}");
                Console.WriteLine($"User: {authenticatedUser?.FullName}");
                Console.WriteLine($"Active: {authenticatedUser?.IsActive}");
                Console.WriteLine($"Last Login: {authenticatedUser?.LastLoginDate}");
            }
            else
            {
                Console.WriteLine($"Failed: {message}");
            }

            // Example 5: Full authentication response
            var response = LoginFunction.AuthenticateUser("user@example.com", "password123");
            Console.WriteLine($"Result: {response.Result}");
            Console.WriteLine($"Message: {response.Message}");
            if (response.User != null)
            {
                Console.WriteLine($"User ID: {response.User.Id}");
                Console.WriteLine($"Is Active: {response.User.IsActive}");
                Console.WriteLine($"Login Attempts: {response.User.LoginAttempts}");
            }
        }
    }
}