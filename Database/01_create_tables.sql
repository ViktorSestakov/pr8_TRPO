-- Практическая работа № 8.
-- Выполнять в существующей БД, к которой выданы права на сервере sql.ects.
-- В SSMS предварительно выберите нужную БД (не запускайте скрипт в master).
-- Сначала создаётся физическая схема, ПОТОМ модели генерируются через EF Core.

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        Login NVARCHAR(100) NOT NULL,
        Role NVARCHAR(20) NOT NULL CONSTRAINT DF_Users_Role DEFAULT N'User',
        PasswordHash NVARCHAR(512) NOT NULL,
        CONSTRAINT UQ_Users_Login UNIQUE (Login),
        CONSTRAINT CK_Users_Role CHECK (Role IN (N'User', N'Admin'))
    );
END;
GO

IF OBJECT_ID(N'dbo.Tasks', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tasks
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Tasks PRIMARY KEY,
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000) NOT NULL,
        OwnerLogin NVARCHAR(100) NOT NULL,
        IsCompleted BIT NOT NULL CONSTRAINT DF_Tasks_IsCompleted DEFAULT (0)
    );
    CREATE INDEX IX_Tasks_OwnerLogin ON dbo.Tasks (OwnerLogin);
END;
GO
