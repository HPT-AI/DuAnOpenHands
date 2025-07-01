-- Create Database Script for User CRUD Application
-- Run this script to create the database and table structure

-- Create the database (if it doesn't exist)
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'UserCRUDDB')
BEGIN
    CREATE DATABASE UserCRUDDB;
END
GO

-- Use the database
USE UserCRUDDB;
GO

-- Create Users table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FirstName NVARCHAR(50) NOT NULL,
        LastName NVARCHAR(50) NOT NULL,
        Email NVARCHAR(100) NOT NULL,
        Phone NVARCHAR(20) NULL,
        PasswordHash NVARCHAR(255) NULL,
        Salt NVARCHAR(255) NULL,
        DateCreated DATETIME2 NOT NULL DEFAULT GETDATE(),
        DateModified DATETIME2 NULL,
        LastLoginDate DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        LoginAttempts INT NOT NULL DEFAULT 0,
        LockedUntil DATETIME2 NULL,
        Role INT NOT NULL DEFAULT 1
    );
END
ELSE
BEGIN
    -- Add new columns if they don't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'PasswordHash')
        ALTER TABLE Users ADD PasswordHash NVARCHAR(255) NULL;
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Salt')
        ALTER TABLE Users ADD Salt NVARCHAR(255) NULL;
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'LastLoginDate')
        ALTER TABLE Users ADD LastLoginDate DATETIME2 NULL;
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'LoginAttempts')
        ALTER TABLE Users ADD LoginAttempts INT NOT NULL DEFAULT 0;
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'LockedUntil')
        ALTER TABLE Users ADD LockedUntil DATETIME2 NULL;
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Role')
        ALTER TABLE Users ADD Role INT NOT NULL DEFAULT 1;
END
GO

-- Create indexes for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Users_Email ON Users (Email);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_LastName_FirstName')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Users_LastName_FirstName ON Users (LastName, FirstName);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_IsActive')
BEGIN
    CREATE NONCLUSTERED INDEX IX_Users_IsActive ON Users (IsActive);
END
GO

-- Create QuadraticResults table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='QuadraticResults' AND xtype='U')
BEGIN
    CREATE TABLE QuadraticResults (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        A FLOAT NOT NULL,
        B FLOAT NOT NULL,
        C FLOAT NOT NULL,
        Root1 FLOAT NULL,
        Root2 FLOAT NULL,
        Discriminant FLOAT NOT NULL,
        HasRealRoots BIT NOT NULL DEFAULT 0,
        CalculatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        UserId INT NULL,
        Notes NVARCHAR(500) NULL,
        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
    );
END
GO

-- Create indexes for QuadraticResults table
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_QuadraticResults_UserId')
BEGIN
    CREATE NONCLUSTERED INDEX IX_QuadraticResults_UserId ON QuadraticResults (UserId);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_QuadraticResults_CalculatedDate')
BEGIN
    CREATE NONCLUSTERED INDEX IX_QuadraticResults_CalculatedDate ON QuadraticResults (CalculatedDate);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_QuadraticResults_HasRealRoots')
BEGIN
    CREATE NONCLUSTERED INDEX IX_QuadraticResults_HasRealRoots ON QuadraticResults (HasRealRoots);
END
GO

-- Insert sample data with authentication fields
IF NOT EXISTS (SELECT * FROM Users)
BEGIN
    INSERT INTO Users (FirstName, LastName, Email, Phone, IsActive, LoginAttempts) VALUES
    ('John', 'Doe', 'john.doe@email.com', '555-0101', 1, 0),
    ('Jane', 'Smith', 'jane.smith@email.com', '555-0102', 1, 0),
    ('Bob', 'Johnson', 'bob.johnson@email.com', '555-0103', 1, 0),
    ('Alice', 'Williams', 'alice.williams@email.com', '555-0104', 1, 0),
    ('Charlie', 'Brown', 'charlie.brown@email.com', '555-0105', 0, 0),
    ('Diana', 'Davis', 'diana.davis@email.com', '555-0106', 1, 0),
    ('Edward', 'Miller', 'edward.miller@email.com', '555-0107', 1, 0),
    ('Fiona', 'Wilson', 'fiona.wilson@email.com', '555-0108', 1, 0),
    ('George', 'Moore', 'george.moore@email.com', '555-0109', 0, 0),
    ('Helen', 'Taylor', 'helen.taylor@email.com', '555-0110', 1, 0);
    
    PRINT 'Sample users created. Use the application to set passwords for testing.';
    PRINT 'Note: Users without passwords cannot log in until passwords are set.';
END
GO

-- Insert sample QuadraticResults data
IF NOT EXISTS (SELECT * FROM QuadraticResults)
BEGIN
    INSERT INTO QuadraticResults (A, B, C, Root1, Root2, Discriminant, HasRealRoots, UserId, Notes) VALUES
    (1, -5, 6, 3, 2, 1, 1, 1, 'Simple quadratic: x² - 5x + 6 = 0'),
    (1, 0, -4, 2, -2, 16, 1, 2, 'Difference of squares: x² - 4 = 0'),
    (1, -2, 1, 1, 1, 0, 1, 3, 'Perfect square: (x - 1)² = 0'),
    (1, 0, 1, NULL, NULL, -4, 0, 4, 'No real roots: x² + 1 = 0'),
    (2, -4, 2, 1.414, 0.586, 0, 1, 5, '2x² - 4x + 2 = 0'),
    (1, -3, 2, 2, 1, 1, 1, NULL, 'x² - 3x + 2 = 0'),
    (3, 2, -1, 0.333, -1, 16, 1, 6, '3x² + 2x - 1 = 0'),
    (1, 4, 4, -2, -2, 0, 1, 7, 'Perfect square: (x + 2)² = 0'),
    (2, 1, -3, 1, -1.5, 25, 1, 8, '2x² + x - 3 = 0'),
    (1, 2, 5, NULL, NULL, -16, 0, 9, 'No real roots: x² + 2x + 5 = 0');
END
GO

PRINT 'Database and tables created successfully with sample data.';
GO