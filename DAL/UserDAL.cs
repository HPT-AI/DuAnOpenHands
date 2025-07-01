using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using UserCRUD.Models;

namespace UserCRUD.DAL
{
    public class UserDAL
    {
        private readonly string _connectionString;

        public UserDAL()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString
                ?? "Server=localhost;Database=UserCRUDDB;Integrated Security=true;TrustServerCertificate=true;";
        }

        public UserDAL(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Creates a new user in the database
        /// </summary>
        /// <param name="user">User object to create</param>
        /// <returns>The ID of the newly created user, or -1 if failed</returns>
        public int CreateUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            const string query = @"
                INSERT INTO Users (FirstName, LastName, Email, Phone, DateCreated, IsActive)
                VALUES (@FirstName, @LastName, @Email, @Phone, @DateCreated, @IsActive);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FirstName", user.FirstName ?? string.Empty);
                        command.Parameters.AddWithValue("@LastName", user.LastName ?? string.Empty);
                        command.Parameters.AddWithValue("@Email", user.Email ?? string.Empty);
                        command.Parameters.AddWithValue("@Phone", user.Phone ?? string.Empty);
                        command.Parameters.AddWithValue("@DateCreated", user.DateCreated);
                        command.Parameters.AddWithValue("@IsActive", user.IsActive);

                        connection.Open();
                        var result = command.ExecuteScalar();
                        return result != null ? Convert.ToInt32(result) : -1;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while creating user: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while creating user: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves a user by their ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User object if found, null otherwise</returns>
        public User? GetUserById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("User ID must be greater than 0", nameof(id));

            const string query = @"
                SELECT Id, FirstName, LastName, Email, Phone, PasswordHash, Salt, 
                       DateCreated, DateModified, LastLoginDate, IsActive, LoginAttempts, LockedUntil, Role
                FROM Users 
                WHERE Id = @Id";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return MapReaderToUser(reader);
                            }
                        }
                    }
                }
                return null;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while retrieving user with ID {id}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving user with ID {id}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves all users from the database
        /// </summary>
        /// <returns>List of all users</returns>
        public List<User> GetAllUsers()
        {
            const string query = @"
                SELECT Id, FirstName, LastName, Email, Phone, DateCreated, DateModified, IsActive
                FROM Users 
                ORDER BY LastName, FirstName";

            var users = new List<User>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(MapReaderToUser(reader));
                            }
                        }
                    }
                }
                return users;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while retrieving all users: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving all users: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates an existing user in the database
        /// </summary>
        /// <param name="user">User object with updated information</param>
        /// <returns>True if update was successful, false otherwise</returns>
        public bool UpdateUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (user.Id <= 0)
                throw new ArgumentException("User ID must be greater than 0", nameof(user));

            const string query = @"
                UPDATE Users 
                SET FirstName = @FirstName, 
                    LastName = @LastName, 
                    Email = @Email, 
                    Phone = @Phone, 
                    DateModified = @DateModified, 
                    IsActive = @IsActive
                WHERE Id = @Id";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", user.Id);
                        command.Parameters.AddWithValue("@FirstName", user.FirstName ?? string.Empty);
                        command.Parameters.AddWithValue("@LastName", user.LastName ?? string.Empty);
                        command.Parameters.AddWithValue("@Email", user.Email ?? string.Empty);
                        command.Parameters.AddWithValue("@Phone", user.Phone ?? string.Empty);
                        command.Parameters.AddWithValue("@DateModified", DateTime.Now);
                        command.Parameters.AddWithValue("@IsActive", user.IsActive);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while updating user with ID {user.Id}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while updating user with ID {user.Id}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deletes a user from the database
        /// </summary>
        /// <param name="id">User ID to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        public bool DeleteUser(int id)
        {
            if (id <= 0)
                throw new ArgumentException("User ID must be greater than 0", nameof(id));

            const string query = "DELETE FROM Users WHERE Id = @Id";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while deleting user with ID {id}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while deleting user with ID {id}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Soft delete - marks user as inactive instead of deleting
        /// </summary>
        /// <param name="id">User ID to deactivate</param>
        /// <returns>True if deactivation was successful, false otherwise</returns>
        public bool DeactivateUser(int id)
        {
            if (id <= 0)
                throw new ArgumentException("User ID must be greater than 0", nameof(id));

            const string query = @"
                UPDATE Users 
                SET IsActive = 0, DateModified = @DateModified 
                WHERE Id = @Id";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@DateModified", DateTime.Now);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while deactivating user with ID {id}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while deactivating user with ID {id}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Searches users by name or email
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns>List of matching users</returns>
        public List<User> SearchUsers(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllUsers();

            const string query = @"
                SELECT Id, FirstName, LastName, Email, Phone, DateCreated, DateModified, IsActive
                FROM Users 
                WHERE FirstName LIKE @SearchTerm 
                   OR LastName LIKE @SearchTerm 
                   OR Email LIKE @SearchTerm
                ORDER BY LastName, FirstName";

            var users = new List<User>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(MapReaderToUser(reader));
                            }
                        }
                    }
                }
                return users;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while searching users: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while searching users: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves a user by their email address
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>User object if found, null otherwise</returns>
        public User? GetUserByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            const string query = @"
                SELECT Id, FirstName, LastName, Email, Phone, PasswordHash, Salt, 
                       DateCreated, DateModified, LastLoginDate, IsActive, LoginAttempts, LockedUntil, Role
                FROM Users 
                WHERE Email = @Email";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);

                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return MapReaderToUser(reader);
                            }
                        }
                    }
                }
                return null;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while retrieving user with email {email}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving user with email {email}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates user's password hash and salt
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="passwordHash">New password hash</param>
        /// <param name="salt">New salt</param>
        /// <returns>True if update was successful</returns>
        public bool UpdateUserPassword(int userId, string passwordHash, string salt)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than 0", nameof(userId));

            if (string.IsNullOrWhiteSpace(passwordHash) || string.IsNullOrWhiteSpace(salt))
                throw new ArgumentException("Password hash and salt cannot be empty");

            const string query = @"
                UPDATE Users 
                SET PasswordHash = @PasswordHash, Salt = @Salt, DateModified = @DateModified
                WHERE Id = @Id";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", userId);
                        command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                        command.Parameters.AddWithValue("@Salt", salt);
                        command.Parameters.AddWithValue("@DateModified", DateTime.Now);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while updating password for user {userId}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while updating password for user {userId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates user's login attempts and lockout status
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="loginAttempts">Number of login attempts</param>
        /// <param name="lockedUntil">Lockout expiry date (null if not locked)</param>
        /// <returns>True if update was successful</returns>
        public bool UpdateLoginAttempts(int userId, int loginAttempts, DateTime? lockedUntil)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than 0", nameof(userId));

            const string query = @"
                UPDATE Users 
                SET LoginAttempts = @LoginAttempts, LockedUntil = @LockedUntil, DateModified = @DateModified
                WHERE Id = @Id";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", userId);
                        command.Parameters.AddWithValue("@LoginAttempts", loginAttempts);
                        command.Parameters.AddWithValue("@LockedUntil", (object?)lockedUntil ?? DBNull.Value);
                        command.Parameters.AddWithValue("@DateModified", DateTime.Now);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while updating login attempts for user {userId}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while updating login attempts for user {userId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates user's last login date and resets login attempts
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="lastLoginDate">Last login date</param>
        /// <returns>True if update was successful</returns>
        public bool UpdateLoginSuccess(int userId, DateTime lastLoginDate)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than 0", nameof(userId));

            const string query = @"
                UPDATE Users 
                SET LastLoginDate = @LastLoginDate, LoginAttempts = 0, LockedUntil = NULL, DateModified = @DateModified
                WHERE Id = @Id";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", userId);
                        command.Parameters.AddWithValue("@LastLoginDate", lastLoginDate);
                        command.Parameters.AddWithValue("@DateModified", DateTime.Now);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while updating login success for user {userId}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while updating login success for user {userId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates a user's role
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="role">New role</param>
        /// <returns>True if update was successful</returns>
        public bool UpdateUserRole(int userId, UserRole role)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than 0", nameof(userId));

            const string query = @"
                UPDATE Users 
                SET Role = @Role, DateModified = @DateModified
                WHERE Id = @Id";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", userId);
                        command.Parameters.AddWithValue("@Role", (int)role);
                        command.Parameters.AddWithValue("@DateModified", DateTime.Now);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while updating role for user {userId}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while updating role for user {userId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets users by role
        /// </summary>
        /// <param name="role">Role to filter by</param>
        /// <returns>List of users with the specified role</returns>
        public List<User> GetUsersByRole(UserRole role)
        {
            const string query = @"
                SELECT Id, FirstName, LastName, Email, Phone, PasswordHash, Salt, 
                       DateCreated, DateModified, LastLoginDate, IsActive, LoginAttempts, LockedUntil, Role
                FROM Users 
                WHERE Role = @Role
                ORDER BY LastName, FirstName";

            var users = new List<User>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Role", (int)role);

                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(MapReaderToUser(reader));
                            }
                        }
                    }
                }
                return users;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while retrieving users by role {role}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving users by role {role}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets count of users by role
        /// </summary>
        /// <param name="role">Role to count</param>
        /// <returns>Number of users with the specified role</returns>
        public int GetUserCountByRole(UserRole role)
        {
            const string query = "SELECT COUNT(*) FROM Users WHERE Role = @Role AND IsActive = 1";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Role", (int)role);

                        connection.Open();
                        var result = command.ExecuteScalar();
                        return result != null ? Convert.ToInt32(result) : 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while counting users by role {role}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while counting users by role {role}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Tests the database connection
        /// </summary>
        /// <returns>True if connection is successful, false otherwise</returns>
        public bool TestConnection()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    return connection.State == ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Maps SqlDataReader to User object
        /// </summary>
        /// <param name="reader">SqlDataReader</param>
        /// <returns>User object</returns>
        private static User MapReaderToUser(SqlDataReader reader)
        {
            return new User
            {
                Id = reader.GetInt32("Id"),
                FirstName = reader.GetString("FirstName"),
                LastName = reader.GetString("LastName"),
                Email = reader.GetString("Email"),
                Phone = reader.IsDBNull("Phone") ? string.Empty : reader.GetString("Phone"),
                PasswordHash = reader.IsDBNull("PasswordHash") ? string.Empty : reader.GetString("PasswordHash"),
                Salt = reader.IsDBNull("Salt") ? string.Empty : reader.GetString("Salt"),
                DateCreated = reader.GetDateTime("DateCreated"),
                DateModified = reader.IsDBNull("DateModified") ? null : reader.GetDateTime("DateModified"),
                LastLoginDate = reader.IsDBNull("LastLoginDate") ? null : reader.GetDateTime("LastLoginDate"),
                IsActive = reader.GetBoolean("IsActive"),
                LoginAttempts = reader.IsDBNull("LoginAttempts") ? 0 : reader.GetInt32("LoginAttempts"),
                LockedUntil = reader.IsDBNull("LockedUntil") ? null : reader.GetDateTime("LockedUntil"),
                Role = reader.IsDBNull("Role") ? UserRole.User : (UserRole)reader.GetInt32("Role")
            };
        }
    }
}