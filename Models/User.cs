using System;
using System.Security.Cryptography;
using System.Text;

namespace UserCRUD.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Salt { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public bool IsActive { get; set; } = true;
        public int LoginAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
        public UserRole Role { get; set; } = UserRole.User;

        public string FullName => $"{FirstName} {LastName}";
        public bool IsLocked => LockedUntil.HasValue && LockedUntil.Value > DateTime.Now;
        public bool IsAdmin => Role == UserRole.Admin;
        public bool IsUser => Role == UserRole.User;

        public User()
        {
            DateCreated = DateTime.Now;
            LoginAttempts = 0;
        }

        public User(string firstName, string lastName, string email, string phone)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Phone = phone;
            DateCreated = DateTime.Now;
            IsActive = true;
            LoginAttempts = 0;
        }

        /// <summary>
        /// Sets the password for the user by generating a salt and hash
        /// </summary>
        /// <param name="password">Plain text password</param>
        public void SetPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));

            Salt = GenerateSalt();
            PasswordHash = HashPassword(password, Salt);
        }

        /// <summary>
        /// Verifies if the provided password matches the stored hash
        /// </summary>
        /// <param name="password">Plain text password to verify</param>
        /// <returns>True if password matches, false otherwise</returns>
        public bool VerifyPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(PasswordHash) || string.IsNullOrWhiteSpace(Salt))
                return false;

            string hashedInput = HashPassword(password, Salt);
            return hashedInput == PasswordHash;
        }

        /// <summary>
        /// Generates a random salt for password hashing
        /// </summary>
        /// <returns>Base64 encoded salt string</returns>
        private static string GenerateSalt()
        {
            byte[] saltBytes = new byte[32]; // 256 bits
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        /// <summary>
        /// Hashes a password with the provided salt using PBKDF2
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="salt">Salt string</param>
        /// <returns>Base64 encoded hash</returns>
        private static string HashPassword(string password, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256))
            {
                byte[] hashBytes = pbkdf2.GetBytes(32); // 256 bits
                return Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// Resets login attempts and unlock the account
        /// </summary>
        public void ResetLoginAttempts()
        {
            LoginAttempts = 0;
            LockedUntil = null;
        }

        /// <summary>
        /// Increments login attempts and locks account if necessary
        /// </summary>
        public void IncrementLoginAttempts()
        {
            LoginAttempts++;
            
            // Lock account for 15 minutes after 5 failed attempts
            if (LoginAttempts >= 5)
            {
                LockedUntil = DateTime.Now.AddMinutes(15);
            }
        }

        /// <summary>
        /// Updates the last login date to current time
        /// </summary>
        public void UpdateLastLogin()
        {
            LastLoginDate = DateTime.Now;
        }

        public override string ToString()
        {
            return $"Id: {Id}, Name: {FullName}, Email: {Email}, Phone: {Phone}, Active: {IsActive}";
        }
    }
}