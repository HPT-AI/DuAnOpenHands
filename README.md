# DuAnOpenHands - User & QuadraticResults CRUD Application - C# WinForms

A complete CRUD (Create, Read, Update, Delete) application for managing users and quadratic equation results, built with C# WinForms and ADO.NET with SQL Server.

## Features

### User Management
- **Complete CRUD Operations**: Create, Read, Update, Delete users
- **Search Functionality**: Search users by name or email
- **Data Validation**: Form validation for required fields
- **Soft Delete**: Option to deactivate users instead of hard delete

### Quadratic Results Management
- **Quadratic Calculator**: Calculate roots of quadratic equations
- **Complete CRUD Operations**: Create, Read, Update, Delete quadratic results
- **Mathematical Validation**: Automatic calculation of discriminant and roots
- **User Association**: Link results to specific users
- **Search & Statistics**: Search results and view calculation statistics

### Technical Features
- **Modern UI**: Clean WinForms interface with DataGridView
- **Error Handling**: Comprehensive error handling and user feedback
- **SQL Injection Protection**: Parameterized queries for security
- **Database Relationships**: Foreign key relationships between entities

## Project Structure

```
UserCRUD/
├── Models/
│   ├── User.cs                      # User entity model
│   └── QuadraticResult.cs          # QuadraticResult entity model
├── DAL/
│   ├── UserDAL.cs                  # User Data Access Layer
│   └── QuadraticResultDAL.cs       # QuadraticResult Data Access Layer
├── Forms/
│   ├── MainForm.cs                 # Main WinForms UI for Users
│   └── QuadraticResultForm.cs      # QuadraticResults calculator & management
├── SQL/
│   ├── CreateDatabase.sql          # Database creation script (both tables)
│   ├── TestQueries.sql             # User test queries
│   └── QuadraticResultsTestQueries.sql # QuadraticResults test queries
├── App.config                      # Configuration file with connection string
├── Program.cs                      # Application entry point
├── UserCRUD.csproj                # Project file
└── README.md                      # This file
```

## Prerequisites

- .NET 6.0 or later
- SQL Server (LocalDB, Express, or full version)
- Visual Studio 2022 or Visual Studio Code with C# extension

## Setup Instructions

### 1. Database Setup

1. Open SQL Server Management Studio (SSMS) or use sqlcmd
2. Run the script in `SQL/CreateDatabase.sql` to create the database and table
3. Optionally, run `SQL/TestQueries.sql` to test the database operations

### 2. Connection String Configuration

Update the connection string in `App.config` to match your SQL Server instance:

```xml
<connectionStrings>
    <add name="DefaultConnection" 
         connectionString="Server=localhost;Database=UserCRUDDB;Integrated Security=true;TrustServerCertificate=true;" 
         providerName="System.Data.SqlClient" />
</connectionStrings>
```

For SQL Server Authentication, use:
```xml
<add name="DefaultConnection" 
     connectionString="Server=localhost;Database=UserCRUDDB;User Id=your_username;Password=your_password;TrustServerCertificate=true;" 
     providerName="System.Data.SqlClient" />
```

### 3. Build and Run

```bash
# Restore packages
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

Or open the project in Visual Studio and press F5 to run.

## Data Access Layer Methods

### UserDAL Class Methods

The `UserDAL` class implements the following CRUD operations:

#### Core CRUD Methods
- **`CreateUser(User user)`**: Creates a new user and returns the new user ID
- **`GetUserById(int id)`**: Retrieves a user by their ID
- **`GetAllUsers()`**: Retrieves all users from the database
- **`UpdateUser(User user)`**: Updates an existing user
- **`DeleteUser(int id)`**: Permanently deletes a user from the database

#### Additional Methods
- **`DeactivateUser(int id)`**: Soft delete - marks user as inactive
- **`SearchUsers(string searchTerm)`**: Searches users by name or email
- **`TestConnection()`**: Tests the database connection

### QuadraticResultDAL Class Methods

The `QuadraticResultDAL` class implements the following CRUD operations:

#### Core CRUD Methods
- **`CreateQuadraticResult(QuadraticResult result)`**: Creates a new result and returns the ID
- **`GetQuadraticResultById(int id)`**: Retrieves a result by its ID
- **`GetAllQuadraticResults()`**: Retrieves all results from the database
- **`UpdateQuadraticResult(QuadraticResult result)`**: Updates an existing result
- **`DeleteQuadraticResult(int id)`**: Permanently deletes a result from the database

#### Additional Methods
- **`GetQuadraticResultsByUserId(int userId)`**: Gets results for a specific user
- **`SearchQuadraticResults(string searchTerm)`**: Searches results by notes or user
- **`GetQuadraticResultsStatistics()`**: Returns calculation statistics
- **`TestConnection()`**: Tests the database connection

## Model Properties

### User Model
```csharp
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? DateModified { get; set; }
    public bool IsActive { get; set; }
    public string FullName => $"{FirstName} {LastName}";
}
```

### QuadraticResult Model
```csharp
public class QuadraticResult
{
    public int Id { get; set; }
    public double A { get; set; }           // Coefficient A
    public double B { get; set; }           // Coefficient B  
    public double C { get; set; }           // Coefficient C
    public double? Root1 { get; set; }      // First root (if real)
    public double? Root2 { get; set; }      // Second root (if real)
    public double Discriminant { get; set; } // b² - 4ac
    public bool HasRealRoots { get; set; }  // True if discriminant >= 0
    public DateTime CalculatedDate { get; set; }
    public int? UserId { get; set; }        // Foreign key to Users
    public string? UserName { get; set; }   // For display
    public string? Notes { get; set; }      // Optional notes
    
    // Methods
    public void CalculateRoots();           // Calculates roots and discriminant
    public string GetEquationString();      // Returns formatted equation
    public string GetRootsString();         // Returns formatted roots
}
```

## Security Features

- **Parameterized Queries**: All SQL queries use parameters to prevent SQL injection
- **Input Validation**: Form validation for required fields
- **Error Handling**: Comprehensive exception handling with user-friendly messages
- **Connection Management**: Proper disposal of database connections

## Usage Examples

### Creating a User
```csharp
var userDAL = new UserDAL();
var user = new User("John", "Doe", "john.doe@email.com", "555-0101");
int newUserId = userDAL.CreateUser(user);
```

### Retrieving Users
```csharp
// Get all users
var allUsers = userDAL.GetAllUsers();

// Get user by ID
var user = userDAL.GetUserById(1);

// Search users
var searchResults = userDAL.SearchUsers("john");
```

### Updating a User
```csharp
var user = userDAL.GetUserById(1);
if (user != null)
{
    user.Email = "newemail@example.com";
    bool success = userDAL.UpdateUser(user);
}
```

### Deleting a User
```csharp
// Hard delete
bool deleted = userDAL.DeleteUser(1);

// Soft delete (deactivate)
bool deactivated = userDAL.DeactivateUser(1);
```

## Database Schema

### Users Table
```sql
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NULL,
    DateCreated DATETIME2 NOT NULL DEFAULT GETDATE(),
    DateModified DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1
);
```

### QuadraticResults Table
```sql
CREATE TABLE QuadraticResults (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    A FLOAT NOT NULL,                    -- Coefficient A
    B FLOAT NOT NULL,                    -- Coefficient B
    C FLOAT NOT NULL,                    -- Coefficient C
    Root1 FLOAT NULL,                    -- First root (if real)
    Root2 FLOAT NULL,                    -- Second root (if real)
    Discriminant FLOAT NOT NULL,         -- b² - 4ac
    HasRealRoots BIT NOT NULL DEFAULT 0, -- True if real roots exist
    CalculatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    UserId INT NULL,                     -- Foreign key to Users
    Notes NVARCHAR(500) NULL,           -- Optional notes
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
);
```

## Error Handling

The application includes comprehensive error handling:

- Database connection errors
- SQL execution errors
- Validation errors
- User-friendly error messages
- Status updates in the UI

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is provided as-is for educational and demonstration purposes.
