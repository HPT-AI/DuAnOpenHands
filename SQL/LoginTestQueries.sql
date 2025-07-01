-- Login Test Queries for Authentication System
-- Use these queries to test the login functionality manually

USE UserCRUDDB;
GO

-- 1. VIEW ALL USERS WITH AUTHENTICATION STATUS
SELECT 
    Id, 
    FirstName, 
    LastName, 
    Email, 
    IsActive,
    LoginAttempts,
    LockedUntil,
    LastLoginDate,
    CASE 
        WHEN PasswordHash IS NULL OR PasswordHash = '' THEN 'No Password Set'
        ELSE 'Password Set'
    END AS PasswordStatus,
    CASE 
        WHEN LockedUntil IS NOT NULL AND LockedUntil > GETDATE() THEN 'Locked'
        WHEN IsActive = 0 THEN 'Inactive'
        WHEN PasswordHash IS NULL OR PasswordHash = '' THEN 'No Password'
        ELSE 'Available'
    END AS LoginStatus
FROM Users
ORDER BY Id;
GO

-- 2. CHECK SPECIFIC USER BY EMAIL (for login testing)
DECLARE @Email NVARCHAR(100) = 'john.doe@email.com';

SELECT 
    Id, 
    FirstName + ' ' + LastName AS FullName,
    Email, 
    IsActive,
    LoginAttempts,
    LockedUntil,
    LastLoginDate,
    CASE 
        WHEN PasswordHash IS NULL OR PasswordHash = '' THEN 'No Password Set'
        ELSE 'Password Set'
    END AS PasswordStatus
FROM Users 
WHERE Email = @Email;
GO

-- 3. SIMULATE FAILED LOGIN ATTEMPTS (for testing lockout)
-- WARNING: This will lock the test account
/*
DECLARE @UserId INT = 1;
DECLARE @Attempts INT = 5;
DECLARE @LockUntil DATETIME2 = DATEADD(MINUTE, 15, GETDATE());

UPDATE Users 
SET LoginAttempts = @Attempts, 
    LockedUntil = @LockUntil,
    DateModified = GETDATE()
WHERE Id = @UserId;

PRINT 'User account locked for testing';
*/

-- 4. UNLOCK USER ACCOUNT (reset login attempts)
/*
DECLARE @UserId INT = 1;

UPDATE Users 
SET LoginAttempts = 0, 
    LockedUntil = NULL,
    DateModified = GETDATE()
WHERE Id = @UserId;

PRINT 'User account unlocked';
*/

-- 5. SIMULATE SUCCESSFUL LOGIN (update last login date)
/*
DECLARE @UserId INT = 1;

UPDATE Users 
SET LastLoginDate = GETDATE(),
    LoginAttempts = 0,
    LockedUntil = NULL,
    DateModified = GETDATE()
WHERE Id = @UserId;

PRINT 'Successful login recorded';
*/

-- 6. CHECK LOCKED ACCOUNTS
SELECT 
    Id,
    FirstName + ' ' + LastName AS FullName,
    Email,
    LoginAttempts,
    LockedUntil,
    DATEDIFF(MINUTE, GETDATE(), LockedUntil) AS MinutesUntilUnlock
FROM Users 
WHERE LockedUntil IS NOT NULL AND LockedUntil > GETDATE()
ORDER BY LockedUntil;
GO

-- 7. CHECK INACTIVE ACCOUNTS
SELECT 
    Id,
    FirstName + ' ' + LastName AS FullName,
    Email,
    IsActive,
    DateCreated,
    DateModified
FROM Users 
WHERE IsActive = 0
ORDER BY LastName, FirstName;
GO

-- 8. CHECK ACCOUNTS WITHOUT PASSWORDS
SELECT 
    Id,
    FirstName + ' ' + LastName AS FullName,
    Email,
    IsActive,
    DateCreated
FROM Users 
WHERE PasswordHash IS NULL OR PasswordHash = '' OR Salt IS NULL OR Salt = ''
ORDER BY LastName, FirstName;
GO

-- 9. LOGIN ACTIVITY REPORT
SELECT 
    Id,
    FirstName + ' ' + LastName AS FullName,
    Email,
    LastLoginDate,
    LoginAttempts,
    CASE 
        WHEN LastLoginDate IS NULL THEN 'Never logged in'
        WHEN LastLoginDate < DATEADD(DAY, -30, GETDATE()) THEN 'Inactive (30+ days)'
        WHEN LastLoginDate < DATEADD(DAY, -7, GETDATE()) THEN 'Inactive (7+ days)'
        ELSE 'Recently active'
    END AS ActivityStatus
FROM Users 
WHERE IsActive = 1
ORDER BY LastLoginDate DESC;
GO

-- 10. SECURITY AUDIT - ACCOUNTS WITH MULTIPLE FAILED ATTEMPTS
SELECT 
    Id,
    FirstName + ' ' + LastName AS FullName,
    Email,
    LoginAttempts,
    LockedUntil,
    LastLoginDate,
    DateModified
FROM Users 
WHERE LoginAttempts > 0
ORDER BY LoginAttempts DESC, DateModified DESC;
GO

-- 11. CREATE TEST USER WITH PASSWORD (for testing)
-- Note: This creates a user without a password - use the application to set the password
/*
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'test@example.com')
BEGIN
    INSERT INTO Users (FirstName, LastName, Email, Phone, IsActive, LoginAttempts)
    VALUES ('Test', 'User', 'test@example.com', '555-TEST', 1, 0);
    
    PRINT 'Test user created. Use the application to set password: test@example.com';
END
ELSE
BEGIN
    PRINT 'Test user already exists: test@example.com';
END
*/

-- 12. CLEANUP TEST DATA (removes test users)
/*
DELETE FROM Users WHERE Email LIKE '%test%' OR Email LIKE '%example%';
PRINT 'Test users removed';
*/

-- 13. PASSWORD SECURITY CHECK
SELECT 
    COUNT(*) AS TotalUsers,
    SUM(CASE WHEN PasswordHash IS NULL OR PasswordHash = '' THEN 1 ELSE 0 END) AS UsersWithoutPassword,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveUsers,
    SUM(CASE WHEN IsActive = 1 AND (PasswordHash IS NULL OR PasswordHash = '') THEN 1 ELSE 0 END) AS ActiveUsersWithoutPassword,
    SUM(CASE WHEN LockedUntil IS NOT NULL AND LockedUntil > GETDATE() THEN 1 ELSE 0 END) AS LockedUsers,
    SUM(CASE WHEN LoginAttempts > 0 THEN 1 ELSE 0 END) AS UsersWithFailedAttempts
FROM Users;
GO

-- 14. RECENT LOGIN ACTIVITY (last 24 hours)
SELECT 
    Id,
    FirstName + ' ' + LastName AS FullName,
    Email,
    LastLoginDate,
    DATEDIFF(HOUR, LastLoginDate, GETDATE()) AS HoursAgo
FROM Users 
WHERE LastLoginDate >= DATEADD(HOUR, -24, GETDATE())
ORDER BY LastLoginDate DESC;
GO

-- 15. VALIDATE DATABASE SCHEMA FOR AUTHENTICATION
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Users' 
    AND COLUMN_NAME IN ('PasswordHash', 'Salt', 'LastLoginDate', 'LoginAttempts', 'LockedUntil')
ORDER BY ORDINAL_POSITION;
GO

PRINT 'Login test queries completed successfully.';
PRINT 'Use the application to test actual login functionality with password hashing.';
GO