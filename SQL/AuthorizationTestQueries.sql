-- Authorization Test Queries for Role-Based Access Control
-- Use these queries to test the authorization system

USE UserCRUDDB;
GO

-- 1. VIEW ALL USERS WITH THEIR ROLES
SELECT 
    Id,
    FirstName + ' ' + LastName AS FullName,
    Email,
    CASE Role
        WHEN 1 THEN 'User'
        WHEN 2 THEN 'Admin'
        ELSE 'Unknown'
    END AS RoleName,
    Role AS RoleValue,
    IsActive,
    DateCreated,
    LastLoginDate
FROM Users
ORDER BY Role DESC, LastName, FirstName;
GO

-- 2. COUNT USERS BY ROLE
SELECT 
    CASE Role
        WHEN 1 THEN 'User'
        WHEN 2 THEN 'Admin'
        ELSE 'Unknown'
    END AS RoleName,
    COUNT(*) AS UserCount,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveUsers,
    SUM(CASE WHEN IsActive = 0 THEN 1 ELSE 0 END) AS InactiveUsers
FROM Users
GROUP BY Role
ORDER BY Role;
GO

-- 3. GET ADMIN USERS ONLY
SELECT 
    Id,
    FirstName + ' ' + LastName AS FullName,
    Email,
    IsActive,
    DateCreated,
    LastLoginDate
FROM Users 
WHERE Role = 2 -- Admin role
ORDER BY LastName, FirstName;
GO

-- 4. GET REGULAR USERS ONLY
SELECT 
    Id,
    FirstName + ' ' + LastName AS FullName,
    Email,
    IsActive,
    DateCreated,
    LastLoginDate
FROM Users 
WHERE Role = 1 -- User role
ORDER BY LastName, FirstName;
GO

-- 5. CREATE TEST ADMIN USER (for testing authorization)
/*
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'admin@test.com')
BEGIN
    INSERT INTO Users (FirstName, LastName, Email, Phone, IsActive, Role, LoginAttempts)
    VALUES ('Test', 'Admin', 'admin@test.com', '555-ADMIN', 1, 2, 0);
    
    PRINT 'Test admin user created: admin@test.com';
    PRINT 'Use the application to set password for this user.';
END
ELSE
BEGIN
    PRINT 'Test admin user already exists: admin@test.com';
END
*/

-- 6. CREATE TEST REGULAR USER (for testing authorization)
/*
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'user@test.com')
BEGIN
    INSERT INTO Users (FirstName, LastName, Email, Phone, IsActive, Role, LoginAttempts)
    VALUES ('Test', 'User', 'user@test.com', '555-USER', 1, 1, 0);
    
    PRINT 'Test regular user created: user@test.com';
    PRINT 'Use the application to set password for this user.';
END
ELSE
BEGIN
    PRINT 'Test regular user already exists: user@test.com';
END
*/

-- 7. PROMOTE USER TO ADMIN (for testing role changes)
/*
DECLARE @UserId INT = 1; -- Change this to the user ID you want to promote
DECLARE @UserEmail NVARCHAR(100);

SELECT @UserEmail = Email FROM Users WHERE Id = @UserId;

IF @UserEmail IS NOT NULL
BEGIN
    UPDATE Users 
    SET Role = 2, DateModified = GETDATE()
    WHERE Id = @UserId;
    
    PRINT 'User ' + @UserEmail + ' promoted to Admin role.';
END
ELSE
BEGIN
    PRINT 'User not found with ID: ' + CAST(@UserId AS NVARCHAR(10));
END
*/

-- 8. DEMOTE ADMIN TO USER (for testing role changes)
/*
DECLARE @UserId INT = 1; -- Change this to the user ID you want to demote
DECLARE @UserEmail NVARCHAR(100);

SELECT @UserEmail = Email FROM Users WHERE Id = @UserId;

IF @UserEmail IS NOT NULL
BEGIN
    UPDATE Users 
    SET Role = 1, DateModified = GETDATE()
    WHERE Id = @UserId;
    
    PRINT 'User ' + @UserEmail + ' demoted to User role.';
END
ELSE
BEGIN
    PRINT 'User not found with ID: ' + CAST(@UserId AS NVARCHAR(10));
END
*/

-- 9. CHECK QUADRATIC RESULTS OWNERSHIP (for resource-based authorization)
SELECT 
    qr.Id AS ResultId,
    qr.A, qr.B, qr.C,
    qr.UserId AS OwnerId,
    u.FirstName + ' ' + u.LastName AS OwnerName,
    u.Email AS OwnerEmail,
    CASE u.Role
        WHEN 1 THEN 'User'
        WHEN 2 THEN 'Admin'
        ELSE 'Unknown'
    END AS OwnerRole,
    qr.DateCreated
FROM QuadraticResults qr
INNER JOIN Users u ON qr.UserId = u.Id
ORDER BY qr.DateCreated DESC;
GO

-- 10. AUTHORIZATION SIMULATION - Check what each user can access
SELECT 
    u.Id AS UserId,
    u.FirstName + ' ' + u.LastName AS UserName,
    u.Email,
    CASE u.Role
        WHEN 1 THEN 'User'
        WHEN 2 THEN 'Admin'
        ELSE 'Unknown'
    END AS Role,
    
    -- What they can access
    CASE 
        WHEN u.Role = 2 THEN 'All Users, All Quadratic Results, System Settings'
        WHEN u.Role = 1 THEN 'Own Profile, Own Quadratic Results'
        ELSE 'No Access'
    END AS CanAccess,
    
    -- What they can modify
    CASE 
        WHEN u.Role = 2 THEN 'All Users, All Quadratic Results, Roles, System'
        WHEN u.Role = 1 THEN 'Own Profile, Own Quadratic Results'
        ELSE 'Nothing'
    END AS CanModify,
    
    u.IsActive
FROM Users u
ORDER BY u.Role DESC, u.LastName, u.FirstName;
GO

-- 11. SECURITY AUDIT - Find potential security issues
SELECT 
    'Admin Count' AS SecurityCheck,
    COUNT(*) AS Value,
    CASE 
        WHEN COUNT(*) = 0 THEN 'CRITICAL: No admin users found'
        WHEN COUNT(*) = 1 THEN 'WARNING: Only one admin user'
        WHEN COUNT(*) > 5 THEN 'WARNING: Too many admin users'
        ELSE 'OK'
    END AS Status
FROM Users 
WHERE Role = 2 AND IsActive = 1

UNION ALL

SELECT 
    'Inactive Admins' AS SecurityCheck,
    COUNT(*) AS Value,
    CASE 
        WHEN COUNT(*) > 0 THEN 'WARNING: Inactive admin accounts exist'
        ELSE 'OK'
    END AS Status
FROM Users 
WHERE Role = 2 AND IsActive = 0

UNION ALL

SELECT 
    'Users Without Passwords' AS SecurityCheck,
    COUNT(*) AS Value,
    CASE 
        WHEN COUNT(*) > 0 THEN 'WARNING: Users without passwords'
        ELSE 'OK'
    END AS Status
FROM Users 
WHERE (PasswordHash IS NULL OR PasswordHash = '') AND IsActive = 1

UNION ALL

SELECT 
    'Locked Accounts' AS SecurityCheck,
    COUNT(*) AS Value,
    CASE 
        WHEN COUNT(*) > 0 THEN 'INFO: Locked accounts exist'
        ELSE 'OK'
    END AS Status
FROM Users 
WHERE LockedUntil IS NOT NULL AND LockedUntil > GETDATE();
GO

-- 12. ROLE PERMISSION MATRIX (what each role can do)
SELECT 
    'Permission' AS Category,
    'User Role' AS UserRole,
    'Admin Role' AS AdminRole

UNION ALL SELECT 'View Own Profile', 'YES', 'YES'
UNION ALL SELECT 'Update Own Profile', 'YES', 'YES'
UNION ALL SELECT 'Change Own Password', 'YES', 'YES'
UNION ALL SELECT 'View All Users', 'NO', 'YES'
UNION ALL SELECT 'Create Users', 'NO', 'YES'
UNION ALL SELECT 'Update Other Users', 'NO', 'YES'
UNION ALL SELECT 'Delete Users', 'NO', 'YES'
UNION ALL SELECT 'Manage Roles', 'NO', 'YES'
UNION ALL SELECT 'View Own Quadratic Results', 'YES', 'YES'
UNION ALL SELECT 'Create Quadratic Results', 'YES', 'YES'
UNION ALL SELECT 'Update Own Quadratic Results', 'YES', 'YES'
UNION ALL SELECT 'Delete Own Quadratic Results', 'YES', 'YES'
UNION ALL SELECT 'View All Quadratic Results', 'NO', 'YES'
UNION ALL SELECT 'Update Any Quadratic Results', 'NO', 'YES'
UNION ALL SELECT 'Delete Any Quadratic Results', 'NO', 'YES'
UNION ALL SELECT 'View System Logs', 'NO', 'YES'
UNION ALL SELECT 'Manage System Settings', 'NO', 'YES'
UNION ALL SELECT 'Unlock User Accounts', 'NO', 'YES'
UNION ALL SELECT 'Reset User Passwords', 'NO', 'YES';
GO

-- 13. TEST RESOURCE OWNERSHIP SCENARIOS
-- Show which users can access which quadratic results
SELECT 
    qr.Id AS ResultId,
    qr.UserId AS ResultOwnerId,
    owner.FirstName + ' ' + owner.LastName AS ResultOwner,
    
    -- For each user, show if they can access this result
    u.Id AS AccessingUserId,
    u.FirstName + ' ' + u.LastName AS AccessingUser,
    CASE u.Role
        WHEN 1 THEN 'User'
        WHEN 2 THEN 'Admin'
    END AS AccessingUserRole,
    
    -- Access permissions
    CASE 
        WHEN u.Id = qr.UserId THEN 'Owner - Full Access'
        WHEN u.Role = 2 THEN 'Admin - Full Access'
        ELSE 'No Access'
    END AS AccessLevel
    
FROM QuadraticResults qr
INNER JOIN Users owner ON qr.UserId = owner.Id
CROSS JOIN Users u
WHERE u.IsActive = 1
ORDER BY qr.Id, u.Role DESC, u.LastName;
GO

-- 14. VALIDATE DATABASE SCHEMA FOR AUTHORIZATION
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Users' 
    AND COLUMN_NAME = 'Role'
ORDER BY ORDINAL_POSITION;
GO

-- 15. CLEANUP TEST DATA (removes test users)
/*
DELETE FROM Users WHERE Email LIKE '%test.com' OR Email LIKE '%example.com';
PRINT 'Test users removed';
*/

PRINT 'Authorization test queries completed successfully.';
PRINT 'Use the application to test actual authorization functionality.';
PRINT '';
PRINT 'Role Values:';
PRINT '1 = User (Standard user with limited permissions)';
PRINT '2 = Admin (Administrator with full permissions)';
GO