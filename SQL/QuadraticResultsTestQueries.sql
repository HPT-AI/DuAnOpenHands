-- Test Queries for QuadraticResults CRUD Application
-- Use these queries to test the database operations manually

USE UserCRUDDB;
GO

-- 1. SELECT ALL QUADRATIC RESULTS
SELECT qr.Id, qr.A, qr.B, qr.C, qr.Root1, qr.Root2, qr.Discriminant, 
       qr.HasRealRoots, qr.CalculatedDate, qr.UserId, qr.Notes,
       u.FirstName + ' ' + u.LastName AS UserName
FROM QuadraticResults qr
LEFT JOIN Users u ON qr.UserId = u.Id
ORDER BY qr.CalculatedDate DESC;
GO

-- 2. SELECT QUADRATIC RESULT BY ID
SELECT qr.Id, qr.A, qr.B, qr.C, qr.Root1, qr.Root2, qr.Discriminant, 
       qr.HasRealRoots, qr.CalculatedDate, qr.UserId, qr.Notes,
       u.FirstName + ' ' + u.LastName AS UserName
FROM QuadraticResults qr
LEFT JOIN Users u ON qr.UserId = u.Id
WHERE qr.Id = 1;
GO

-- 3. INSERT NEW QUADRATIC RESULT (Test Create)
DECLARE @A FLOAT = 1;
DECLARE @B FLOAT = -7;
DECLARE @C FLOAT = 12;
DECLARE @Root1 FLOAT = 4;
DECLARE @Root2 FLOAT = 3;
DECLARE @Discriminant FLOAT = 1; -- b² - 4ac = 49 - 48 = 1
DECLARE @HasRealRoots BIT = 1;
DECLARE @UserId INT = 1;
DECLARE @Notes NVARCHAR(500) = 'Test equation: x² - 7x + 12 = 0';

INSERT INTO QuadraticResults (A, B, C, Root1, Root2, Discriminant, HasRealRoots, CalculatedDate, UserId, Notes)
VALUES (@A, @B, @C, @Root1, @Root2, @Discriminant, @HasRealRoots, GETDATE(), @UserId, @Notes);
GO

-- Get the newly created result ID
SELECT SCOPE_IDENTITY() AS NewResultId;
GO

-- 4. UPDATE QUADRATIC RESULT (Test Update)
UPDATE QuadraticResults 
SET A = 2, 
    B = -8, 
    C = 6,
    Root1 = 3,
    Root2 = 1,
    Discriminant = 16, -- b² - 4ac = 64 - 48 = 16
    HasRealRoots = 1,
    Notes = 'Updated equation: 2x² - 8x + 6 = 0'
WHERE Id = (SELECT MAX(Id) FROM QuadraticResults);
GO

-- 5. SEARCH QUADRATIC RESULTS (Test Search functionality)
-- Search by notes
SELECT qr.Id, qr.A, qr.B, qr.C, qr.Root1, qr.Root2, qr.Discriminant, 
       qr.HasRealRoots, qr.CalculatedDate, qr.UserId, qr.Notes,
       u.FirstName + ' ' + u.LastName AS UserName
FROM QuadraticResults qr
LEFT JOIN Users u ON qr.UserId = u.Id
WHERE qr.Notes LIKE '%square%'
ORDER BY qr.CalculatedDate DESC;
GO

-- Search by user name
SELECT qr.Id, qr.A, qr.B, qr.C, qr.Root1, qr.Root2, qr.Discriminant, 
       qr.HasRealRoots, qr.CalculatedDate, qr.UserId, qr.Notes,
       u.FirstName + ' ' + u.LastName AS UserName
FROM QuadraticResults qr
LEFT JOIN Users u ON qr.UserId = u.Id
WHERE u.FirstName LIKE '%John%' OR u.LastName LIKE '%John%'
ORDER BY qr.CalculatedDate DESC;
GO

-- 6. GET QUADRATIC RESULTS BY USER ID
SELECT qr.Id, qr.A, qr.B, qr.C, qr.Root1, qr.Root2, qr.Discriminant, 
       qr.HasRealRoots, qr.CalculatedDate, qr.UserId, qr.Notes,
       u.FirstName + ' ' + u.LastName AS UserName
FROM QuadraticResults qr
LEFT JOIN Users u ON qr.UserId = u.Id
WHERE qr.UserId = 1
ORDER BY qr.CalculatedDate DESC;
GO

-- 7. GET RESULTS WITH REAL ROOTS ONLY
SELECT qr.Id, qr.A, qr.B, qr.C, qr.Root1, qr.Root2, qr.Discriminant, 
       qr.HasRealRoots, qr.CalculatedDate, qr.UserId, qr.Notes,
       u.FirstName + ' ' + u.LastName AS UserName
FROM QuadraticResults qr
LEFT JOIN Users u ON qr.UserId = u.Id
WHERE qr.HasRealRoots = 1
ORDER BY qr.CalculatedDate DESC;
GO

-- 8. GET RESULTS WITH COMPLEX ROOTS ONLY
SELECT qr.Id, qr.A, qr.B, qr.C, qr.Root1, qr.Root2, qr.Discriminant, 
       qr.HasRealRoots, qr.CalculatedDate, qr.UserId, qr.Notes,
       u.FirstName + ' ' + u.LastName AS UserName
FROM QuadraticResults qr
LEFT JOIN Users u ON qr.UserId = u.Id
WHERE qr.HasRealRoots = 0
ORDER BY qr.CalculatedDate DESC;
GO

-- 9. DELETE QUADRATIC RESULT (Hard Delete - Test Delete)
-- Be careful with this - it permanently removes the result
-- DELETE FROM QuadraticResults WHERE Id = (SELECT MAX(Id) FROM QuadraticResults);
-- GO

-- 10. STATISTICS QUERIES
-- Count results by type
SELECT 
    COUNT(*) AS TotalResults,
    SUM(CASE WHEN HasRealRoots = 1 THEN 1 ELSE 0 END) AS ResultsWithRealRoots,
    SUM(CASE WHEN HasRealRoots = 0 THEN 1 ELSE 0 END) AS ResultsWithComplexRoots,
    AVG(Discriminant) AS AverageDiscriminant,
    MIN(CalculatedDate) AS EarliestCalculation,
    MAX(CalculatedDate) AS LatestCalculation
FROM QuadraticResults;
GO

-- 11. RESULTS CALCULATED TODAY
SELECT qr.Id, qr.A, qr.B, qr.C, qr.Root1, qr.Root2, qr.Discriminant, 
       qr.HasRealRoots, qr.CalculatedDate, qr.UserId, qr.Notes,
       u.FirstName + ' ' + u.LastName AS UserName
FROM QuadraticResults qr
LEFT JOIN Users u ON qr.UserId = u.Id
WHERE CAST(qr.CalculatedDate AS DATE) = CAST(GETDATE() AS DATE)
ORDER BY qr.CalculatedDate DESC;
GO

-- 12. RESULTS BY DISCRIMINANT RANGE
-- Positive discriminant (two distinct real roots)
SELECT COUNT(*) AS TwoDistinctRealRoots
FROM QuadraticResults 
WHERE Discriminant > 0;
GO

-- Zero discriminant (one repeated real root)
SELECT COUNT(*) AS OneRepeatedRealRoot
FROM QuadraticResults 
WHERE Discriminant = 0;
GO

-- Negative discriminant (complex roots)
SELECT COUNT(*) AS ComplexRoots
FROM QuadraticResults 
WHERE Discriminant < 0;
GO

-- 13. RESULTS BY USER WITH COUNTS
SELECT 
    u.Id,
    u.FirstName + ' ' + u.LastName AS UserName,
    COUNT(qr.Id) AS ResultCount,
    SUM(CASE WHEN qr.HasRealRoots = 1 THEN 1 ELSE 0 END) AS RealRootsCount,
    SUM(CASE WHEN qr.HasRealRoots = 0 THEN 1 ELSE 0 END) AS ComplexRootsCount
FROM Users u
LEFT JOIN QuadraticResults qr ON u.Id = qr.UserId
WHERE u.IsActive = 1
GROUP BY u.Id, u.FirstName, u.LastName
ORDER BY ResultCount DESC;
GO

-- 14. MOST COMMON COEFFICIENT VALUES
SELECT TOP 5
    A AS CoefficientA,
    COUNT(*) AS Frequency
FROM QuadraticResults
GROUP BY A
ORDER BY Frequency DESC;
GO

SELECT TOP 5
    B AS CoefficientB,
    COUNT(*) AS Frequency
FROM QuadraticResults
GROUP BY B
ORDER BY Frequency DESC;
GO

SELECT TOP 5
    C AS CoefficientC,
    COUNT(*) AS Frequency
FROM QuadraticResults
GROUP BY C
ORDER BY Frequency DESC;
GO

-- 15. VALIDATE QUADRATIC CALCULATIONS
-- Check if stored roots are correct by recalculating
SELECT 
    Id,
    A, B, C,
    Root1, Root2,
    Discriminant,
    -- Recalculate discriminant
    (B * B) - (4 * A * C) AS CalculatedDiscriminant,
    -- Check if discriminant matches
    CASE 
        WHEN ABS(Discriminant - ((B * B) - (4 * A * C))) < 0.0001 THEN 'Correct'
        ELSE 'Incorrect'
    END AS DiscriminantCheck
FROM QuadraticResults
WHERE A <> 0; -- Only for true quadratic equations
GO

-- 16. CLEANUP TEST DATA (Optional - removes test results)
-- DELETE FROM QuadraticResults WHERE Notes LIKE '%test%' OR Notes LIKE '%Test%';
-- GO

PRINT 'QuadraticResults test queries completed successfully.';
GO