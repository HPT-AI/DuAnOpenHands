-- Test Queries for User CRUD Application
-- Use these queries to test the database operations manually

USE UserCRUDDB;
GO

-- 1. SELECT ALL USERS
SELECT Id, FirstName, LastName, Email, Phone, DateCreated, DateModified, IsActive
FROM Users
ORDER BY LastName, FirstName;
GO

-- 2. SELECT USER BY ID
SELECT Id, FirstName, LastName, Email, Phone, DateCreated, DateModified, IsActive
FROM Users 
WHERE Id = 1;
GO

-- 3. INSERT NEW USER (Test Create)
INSERT INTO Users (FirstName, LastName, Email, Phone, DateCreated, IsActive)
VALUES ('Test', 'User', 'test.user@email.com', '555-9999', GETDATE(), 1);
GO

-- Get the newly created user ID
SELECT SCOPE_IDENTITY() AS NewUserId;
GO

-- 4. UPDATE USER (Test Update)
UPDATE Users 
SET FirstName = 'Updated', 
    LastName = 'User', 
    Email = 'updated.user@email.com', 
    Phone = '555-8888', 
    DateModified = GETDATE(), 
    IsActive = 1
WHERE Id = (SELECT MAX(Id) FROM Users);
GO

-- 5. SEARCH USERS (Test Search functionality)
-- Search by first name
SELECT Id, FirstName, LastName, Email, Phone, DateCreated, DateModified, IsActive
FROM Users 
WHERE FirstName LIKE '%John%' 
   OR LastName LIKE '%John%' 
   OR Email LIKE '%John%'
ORDER BY LastName, FirstName;
GO

-- Search by email domain
SELECT Id, FirstName, LastName, Email, Phone, DateCreated, DateModified, IsActive
FROM Users 
WHERE Email LIKE '%@email.com%'
ORDER BY LastName, FirstName;
GO

-- 6. SOFT DELETE (Deactivate User)
UPDATE Users 
SET IsActive = 0, DateModified = GETDATE() 
WHERE Id = (SELECT MAX(Id) FROM Users);
GO

-- 7. GET ACTIVE USERS ONLY
SELECT Id, FirstName, LastName, Email, Phone, DateCreated, DateModified, IsActive
FROM Users 
WHERE IsActive = 1
ORDER BY LastName, FirstName;
GO

-- 8. GET INACTIVE USERS ONLY
SELECT Id, FirstName, LastName, Email, Phone, DateCreated, DateModified, IsActive
FROM Users 
WHERE IsActive = 0
ORDER BY LastName, FirstName;
GO

-- 9. DELETE USER (Hard Delete - Test Delete)
-- Be careful with this - it permanently removes the user
-- DELETE FROM Users WHERE Id = (SELECT MAX(Id) FROM Users);
-- GO

-- 10. COUNT USERS
SELECT 
    COUNT(*) AS TotalUsers,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveUsers,
    SUM(CASE WHEN IsActive = 0 THEN 1 ELSE 0 END) AS InactiveUsers
FROM Users;
GO

-- 11. USERS CREATED TODAY
SELECT Id, FirstName, LastName, Email, Phone, DateCreated, DateModified, IsActive
FROM Users 
WHERE CAST(DateCreated AS DATE) = CAST(GETDATE() AS DATE)
ORDER BY DateCreated DESC;
GO

-- 12. USERS MODIFIED TODAY
SELECT Id, FirstName, LastName, Email, Phone, DateCreated, DateModified, IsActive
FROM Users 
WHERE CAST(DateModified AS DATE) = CAST(GETDATE() AS DATE)
ORDER BY DateModified DESC;
GO

-- 13. VALIDATE EMAIL UNIQUENESS (if needed)
SELECT Email, COUNT(*) AS Count
FROM Users
GROUP BY Email
HAVING COUNT(*) > 1;
GO

-- 14. CLEANUP TEST DATA (Optional - removes test users)
-- DELETE FROM Users WHERE Email LIKE '%test%' OR Email LIKE '%updated%';
-- GO

PRINT 'Test queries completed successfully.';
GO