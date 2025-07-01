using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using UserCRUD.Models;

namespace UserCRUD.DAL
{
    public class QuadraticResultDAL
    {
        private readonly string _connectionString;

        public QuadraticResultDAL()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString
                ?? "Server=localhost;Database=UserCRUDDB;Integrated Security=true;TrustServerCertificate=true;";
        }

        public QuadraticResultDAL(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Creates a new quadratic result in the database
        /// </summary>
        /// <param name="result">QuadraticResult object to create</param>
        /// <returns>The ID of the newly created result, or -1 if failed</returns>
        public int CreateQuadraticResult(QuadraticResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            // Ensure roots are calculated
            result.CalculateRoots();

            const string query = @"
                INSERT INTO QuadraticResults (A, B, C, Root1, Root2, Discriminant, HasRealRoots, CalculatedDate, UserId, Notes)
                VALUES (@A, @B, @C, @Root1, @Root2, @Discriminant, @HasRealRoots, @CalculatedDate, @UserId, @Notes);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@A", result.A);
                        command.Parameters.AddWithValue("@B", result.B);
                        command.Parameters.AddWithValue("@C", result.C);
                        command.Parameters.AddWithValue("@Root1", (object?)result.Root1 ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Root2", (object?)result.Root2 ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Discriminant", result.Discriminant);
                        command.Parameters.AddWithValue("@HasRealRoots", result.HasRealRoots);
                        command.Parameters.AddWithValue("@CalculatedDate", result.CalculatedDate);
                        command.Parameters.AddWithValue("@UserId", (object?)result.UserId ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Notes", result.Notes ?? string.Empty);

                        connection.Open();
                        var newId = command.ExecuteScalar();
                        return newId != null ? Convert.ToInt32(newId) : -1;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while creating quadratic result: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while creating quadratic result: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves a quadratic result by its ID
        /// </summary>
        /// <param name="id">Result ID</param>
        /// <returns>QuadraticResult object if found, null otherwise</returns>
        public QuadraticResult? GetQuadraticResultById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Result ID must be greater than 0", nameof(id));

            const string query = @"
                SELECT qr.Id, qr.A, qr.B, qr.C, qr.Root1, qr.Root2, qr.Discriminant, 
                       qr.HasRealRoots, qr.CalculatedDate, qr.UserId, qr.Notes,
                       u.FirstName + ' ' + u.LastName AS UserName
                FROM QuadraticResults qr
                LEFT JOIN Users u ON qr.UserId = u.Id
                WHERE qr.Id = @Id";

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
                                return MapReaderToQuadraticResult(reader);
                            }
                        }
                    }
                }
                return null;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while retrieving quadratic result with ID {id}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving quadratic result with ID {id}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves all quadratic results from the database
        /// </summary>
        /// <returns>List of all quadratic results</returns>
        public List<QuadraticResult> GetAllQuadraticResults()
        {
            const string query = @"
                SELECT qr.Id, qr.A, qr.B, qr.C, qr.Root1, qr.Root2, qr.Discriminant, 
                       qr.HasRealRoots, qr.CalculatedDate, qr.UserId, qr.Notes,
                       u.FirstName + ' ' + u.LastName AS UserName
                FROM QuadraticResults qr
                LEFT JOIN Users u ON qr.UserId = u.Id
                ORDER BY qr.CalculatedDate DESC";

            var results = new List<QuadraticResult>();

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
                                results.Add(MapReaderToQuadraticResult(reader));
                            }
                        }
                    }
                }
                return results;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while retrieving all quadratic results: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving all quadratic results: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates an existing quadratic result in the database
        /// </summary>
        /// <param name="result">QuadraticResult object with updated information</param>
        /// <returns>True if update was successful, false otherwise</returns>
        public bool UpdateQuadraticResult(QuadraticResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            if (result.Id <= 0)
                throw new ArgumentException("Result ID must be greater than 0", nameof(result));

            // Recalculate roots in case coefficients changed
            result.CalculateRoots();

            const string query = @"
                UPDATE QuadraticResults 
                SET A = @A, B = @B, C = @C, Root1 = @Root1, Root2 = @Root2, 
                    Discriminant = @Discriminant, HasRealRoots = @HasRealRoots, 
                    UserId = @UserId, Notes = @Notes
                WHERE Id = @Id";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", result.Id);
                        command.Parameters.AddWithValue("@A", result.A);
                        command.Parameters.AddWithValue("@B", result.B);
                        command.Parameters.AddWithValue("@C", result.C);
                        command.Parameters.AddWithValue("@Root1", (object?)result.Root1 ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Root2", (object?)result.Root2 ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Discriminant", result.Discriminant);
                        command.Parameters.AddWithValue("@HasRealRoots", result.HasRealRoots);
                        command.Parameters.AddWithValue("@UserId", (object?)result.UserId ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Notes", result.Notes ?? string.Empty);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while updating quadratic result with ID {result.Id}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while updating quadratic result with ID {result.Id}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Deletes a quadratic result from the database
        /// </summary>
        /// <param name="id">Result ID to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        public bool DeleteQuadraticResult(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Result ID must be greater than 0", nameof(id));

            const string query = "DELETE FROM QuadraticResults WHERE Id = @Id";

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
                throw new Exception($"Database error while deleting quadratic result with ID {id}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while deleting quadratic result with ID {id}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves quadratic results by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of quadratic results for the specified user</returns>
        public List<QuadraticResult> GetQuadraticResultsByUserId(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("User ID must be greater than 0", nameof(userId));

            const string query = @"
                SELECT qr.Id, qr.A, qr.B, qr.C, qr.Root1, qr.Root2, qr.Discriminant, 
                       qr.HasRealRoots, qr.CalculatedDate, qr.UserId, qr.Notes,
                       u.FirstName + ' ' + u.LastName AS UserName
                FROM QuadraticResults qr
                LEFT JOIN Users u ON qr.UserId = u.Id
                WHERE qr.UserId = @UserId
                ORDER BY qr.CalculatedDate DESC";

            var results = new List<QuadraticResult>();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);

                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(MapReaderToQuadraticResult(reader));
                            }
                        }
                    }
                }
                return results;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while retrieving quadratic results for user {userId}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving quadratic results for user {userId}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Searches quadratic results by equation coefficients or notes
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <returns>List of matching quadratic results</returns>
        public List<QuadraticResult> SearchQuadraticResults(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllQuadraticResults();

            const string query = @"
                SELECT qr.Id, qr.A, qr.B, qr.C, qr.Root1, qr.Root2, qr.Discriminant, 
                       qr.HasRealRoots, qr.CalculatedDate, qr.UserId, qr.Notes,
                       u.FirstName + ' ' + u.LastName AS UserName
                FROM QuadraticResults qr
                LEFT JOIN Users u ON qr.UserId = u.Id
                WHERE qr.Notes LIKE @SearchTerm 
                   OR u.FirstName LIKE @SearchTerm 
                   OR u.LastName LIKE @SearchTerm
                ORDER BY qr.CalculatedDate DESC";

            var results = new List<QuadraticResult>();

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
                                results.Add(MapReaderToQuadraticResult(reader));
                            }
                        }
                    }
                }
                return results;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while searching quadratic results: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while searching quadratic results: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets statistics about quadratic results
        /// </summary>
        /// <returns>Dictionary with statistics</returns>
        public Dictionary<string, object> GetQuadraticResultsStatistics()
        {
            const string query = @"
                SELECT 
                    COUNT(*) AS TotalResults,
                    SUM(CASE WHEN HasRealRoots = 1 THEN 1 ELSE 0 END) AS ResultsWithRealRoots,
                    SUM(CASE WHEN HasRealRoots = 0 THEN 1 ELSE 0 END) AS ResultsWithComplexRoots,
                    AVG(Discriminant) AS AverageDiscriminant,
                    MIN(CalculatedDate) AS EarliestCalculation,
                    MAX(CalculatedDate) AS LatestCalculation
                FROM QuadraticResults";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Dictionary<string, object>
                                {
                                    ["TotalResults"] = reader.GetInt32("TotalResults"),
                                    ["ResultsWithRealRoots"] = reader.GetInt32("ResultsWithRealRoots"),
                                    ["ResultsWithComplexRoots"] = reader.GetInt32("ResultsWithComplexRoots"),
                                    ["AverageDiscriminant"] = reader.IsDBNull("AverageDiscriminant") ? 0.0 : reader.GetDouble("AverageDiscriminant"),
                                    ["EarliestCalculation"] = reader.IsDBNull("EarliestCalculation") ? (DateTime?)null : reader.GetDateTime("EarliestCalculation"),
                                    ["LatestCalculation"] = reader.IsDBNull("LatestCalculation") ? (DateTime?)null : reader.GetDateTime("LatestCalculation")
                                };
                            }
                        }
                    }
                }
                return new Dictionary<string, object>();
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error while retrieving statistics: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving statistics: {ex.Message}", ex);
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
        /// Maps SqlDataReader to QuadraticResult object
        /// </summary>
        /// <param name="reader">SqlDataReader</param>
        /// <returns>QuadraticResult object</returns>
        private static QuadraticResult MapReaderToQuadraticResult(SqlDataReader reader)
        {
            return new QuadraticResult
            {
                Id = reader.GetInt32("Id"),
                A = reader.GetDouble("A"),
                B = reader.GetDouble("B"),
                C = reader.GetDouble("C"),
                Root1 = reader.IsDBNull("Root1") ? null : reader.GetDouble("Root1"),
                Root2 = reader.IsDBNull("Root2") ? null : reader.GetDouble("Root2"),
                Discriminant = reader.GetDouble("Discriminant"),
                HasRealRoots = reader.GetBoolean("HasRealRoots"),
                CalculatedDate = reader.GetDateTime("CalculatedDate"),
                UserId = reader.IsDBNull("UserId") ? null : reader.GetInt32("UserId"),
                UserName = reader.IsDBNull("UserName") ? null : reader.GetString("UserName"),
                Notes = reader.IsDBNull("Notes") ? null : reader.GetString("Notes")
            };
        }
    }
}