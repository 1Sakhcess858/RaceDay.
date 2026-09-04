-- ============================================
-- RaceDay Database Schema
-- ============================================

-- Drop database if it exists
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'RaceDayDB')
BEGIN
    ALTER DATABASE RaceDayDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RaceDayDB;
END
GO

-- Create database
CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO

-- ============================================
-- 1. USER Table
-- ============================================
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(255) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- ============================================
-- 2. ORGANISER Table
-- ============================================
CREATE TABLE Organisers (
    OrganiserId INT PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    OrganiserName NVARCHAR(255) NOT NULL,
    CONSTRAINT FK_Organiser_User FOREIGN KEY (OrganiserId) 
        REFERENCES Users(UserId) ON DELETE CASCADE
);
GO

-- ============================================
-- 3. PARTICIPANT Table
-- ============================================
CREATE TABLE Participants (
    ParticipantId INT PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    DateOfBirth DATE,
    ProfilePictureURL NVARCHAR(500),
    CONSTRAINT FK_Participant_User FOREIGN KEY (ParticipantId) 
        REFERENCES Users(UserId) ON DELETE CASCADE
);
GO

-- ============================================
-- 4. EVENT Table
-- ============================================
CREATE TABLE Events (
    EventId INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserId INT NOT NULL,
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    EventDate DATETIME NOT NULL,
    Location NVARCHAR(255) NOT NULL,
    Distance DECIMAL(5,2) NOT NULL,
    EventType NVARCHAR(50) NOT NULL,
    BannerImageURL NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Event_Organiser FOREIGN KEY (OrganiserId) 
        REFERENCES Organisers(OrganiserId) ON DELETE CASCADE,
    CONSTRAINT CHK_EventType CHECK (EventType IN ('Run', 'Walk', 'Cycle'))
);
GO

-- ============================================
-- 5. CATEGORY Table
-- ============================================
CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255),
    CONSTRAINT FK_Category_Event FOREIGN KEY (EventId) 
        REFERENCES Events(EventId) ON DELETE CASCADE
);
GO

-- ============================================
-- 6. ENROLMENT Table
-- ============================================
CREATE TABLE Enrolments (
    EnrolmentId INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantId INT NOT NULL,
    EventId INT NOT NULL,
    CategoryId INT NOT NULL,
    EnrolmentDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(50) DEFAULT 'Pending',
    CONSTRAINT FK_Enrolment_Participant FOREIGN KEY (ParticipantId) 
        REFERENCES Participants(ParticipantId) ON DELETE CASCADE,
    CONSTRAINT FK_Enrolment_Event FOREIGN KEY (EventId) 
        REFERENCES Events(EventId) ON DELETE CASCADE,
    CONSTRAINT FK_Enrolment_Category FOREIGN KEY (CategoryId) 
        REFERENCES Categories(CategoryId) ON DELETE CASCADE,
    CONSTRAINT CHK_Status CHECK (Status IN ('Pending', 'Confirmed', 'Completed')),
    CONSTRAINT UQ_Enrolment UNIQUE (ParticipantId, EventId)
);
GO

-- ============================================
-- 7. RESULT Table
-- ============================================
CREATE TABLE Results (
    ResultId INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentId INT NOT NULL UNIQUE,
    FinishTime TIME,
    FinishingPosition INT,
    CONSTRAINT FK_Result_Enrolment FOREIGN KEY (EnrolmentId) 
        REFERENCES Enrolments(EnrolmentId) ON DELETE CASCADE
);
GO

-- ============================================
-- SEED DATA
-- ============================================

INSERT INTO Users (Email, PasswordHash) VALUES 
    ('john.doe@durbanrunning.co.za', 'hashed_password_123'),
    ('jane.smith@capetowncyclists.co.za', 'hashed_password_456'),
    ('alice.wonder@gmail.com', 'hashed_password_789'),
    ('bob.builder@yahoo.com', 'hashed_password_321');
GO

INSERT INTO Organisers (OrganiserId, FirstName, LastName, OrganiserName) VALUES 
    (1, 'John', 'Doe', 'Durban Running Club'),
    (2, 'Jane', 'Smith', 'Cape Town Cycle Tours');
GO

INSERT INTO Participants (ParticipantId, FirstName, LastName, DateOfBirth, ProfilePictureURL) VALUES 
    (3, 'Alice', 'Wonder', '1990-05-15', NULL),
    (4, 'Bob', 'Builder', '1985-10-20', NULL);
GO

INSERT INTO Events (OrganiserId, Name, Description, EventDate, Location, Distance, EventType, BannerImageURL) VALUES 
    (1, 'Durban 10km Challenge', 'A scenic run along the Durban beachfront.', '2026-09-15 07:00:00', 'Durban Beachfront', 10.00, 'Run', NULL),
    (1, 'Durban Marathon', 'The premier running event in KwaZulu-Natal.', '2026-10-20 06:00:00', 'Durban City Hall', 42.20, 'Run', NULL),
    (2, 'Cape Town Cycle Tour', 'The world''s largest timed cycling event.', '2026-03-13 08:00:00', 'Cape Town Stadium', 109.00, 'Cycle', NULL);
GO

INSERT INTO Categories (EventId, Name, Description) VALUES 
    (1, 'Under 20', 'For athletes aged 19 and under'),
    (1, 'Senior (20-39)', 'For athletes aged 20 to 39'),
    (1, 'Master (40+)', 'For athletes aged 40 and above'),
    (2, 'Under 23', 'For athletes aged 22 and under'),
    (2, 'Open (23-39)', 'For athletes aged 23 to 39'),
    (2, 'Veteran (40+)', 'For athletes aged 40 and above'),
    (3, 'Under 18', 'For cyclists aged 17 and under'),
    (3, 'Elite (18-29)', 'For competitive cyclists aged 18 to 29'),
    (3, 'Master (30-49)', 'For cyclists aged 30 to 49');
GO

INSERT INTO Enrolments (ParticipantId, EventId, CategoryId, Status) VALUES 
    (3, 1, 2, 'Confirmed'),
    (3, 2, 5, 'Pending'),
    (4, 3, 8, 'Confirmed'),
    (4, 1, 3, 'Pending');
GO

INSERT INTO Results (EnrolmentId, FinishTime, FinishingPosition) VALUES 
    (1, '00:42:30', 47),
    (3, '03:15:45', 125);
GO

-- Verify data
SELECT 'Users' AS TableName, COUNT(*) AS RecordCount FROM Users
UNION ALL SELECT 'Organisers', COUNT(*) FROM Organisers
UNION ALL SELECT 'Participants', COUNT(*) FROM Participants
UNION ALL SELECT 'Events', COUNT(*) FROM Events
UNION ALL SELECT 'Categories', COUNT(*) FROM Categories
UNION ALL SELECT 'Enrolments', COUNT(*) FROM Enrolments
UNION ALL SELECT 'Results', COUNT(*) FROM Results;
GO