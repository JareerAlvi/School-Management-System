-- Create Database
CREATE DATABASE  SchoolManagementSystem;
USE SchoolManagementSystem;
CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL
);

CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    RoleID INT,
    ProfilePicture NVARCHAR(255),
    LastLogin DATETIME,
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);

CREATE TABLE Students (
    StudentID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT,
    FullName NVARCHAR(100),
    DateOfBirth DATE,
    Gender NVARCHAR(10),
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    Address NVARCHAR(255),
    GuardianInfo NVARCHAR(255),
    AdmissionDate DATE,
    ClassID INT,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

CREATE TABLE Teachers (
    TeacherID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT,
    FullName NVARCHAR(100),
    Qualification NVARCHAR(100),
    ContactInfo NVARCHAR(255),
    HireDate DATE,
    Salary MONEY,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

CREATE TABLE Classes (
    ClassID INT IDENTITY(1,1) PRIMARY KEY,
    ClassName NVARCHAR(50),
    RoomNumber NVARCHAR(20),
    MaxCapacity INT
);

CREATE TABLE Subjects (
    SubjectID INT IDENTITY(1,1) PRIMARY KEY,
    SubjectName NVARCHAR(100)
);

CREATE TABLE ClassSubjects (
    ClassSubjectID INT IDENTITY(1,1) PRIMARY KEY,
    ClassID INT,
    SubjectID INT,
    FOREIGN KEY (ClassID) REFERENCES Classes(ClassID),
    FOREIGN KEY (SubjectID) REFERENCES Subjects(SubjectID)
);

CREATE TABLE TeacherSubjects (
    TeacherSubjectID INT IDENTITY(1,1) PRIMARY KEY,
    TeacherID INT,
    SubjectID INT,
    FOREIGN KEY (TeacherID) REFERENCES Teachers(TeacherID),
    FOREIGN KEY (SubjectID) REFERENCES Subjects(SubjectID)
);

CREATE TABLE Attendance (
    AttendanceID INT IDENTITY(1,1) PRIMARY KEY,
    Date DATE,
    ClassID INT,
    StudentID INT,
    Status NVARCHAR(10),
    TeacherID INT,
    FOREIGN KEY (ClassID) REFERENCES Classes(ClassID),
    FOREIGN KEY (StudentID) REFERENCES Students(StudentID),
    FOREIGN KEY (TeacherID) REFERENCES Teachers(TeacherID)
);

CREATE TABLE Grades (
    GradeID INT IDENTITY(1,1) PRIMARY KEY,
    StudentID INT,
    SubjectID INT,
    TeacherID INT,
    Marks INT,
    ExamType NVARCHAR(50),
    GradeDate DATE,
    FOREIGN KEY (StudentID) REFERENCES Students(StudentID),
    FOREIGN KEY (SubjectID) REFERENCES Subjects(SubjectID),
    FOREIGN KEY (TeacherID) REFERENCES Teachers(TeacherID)
);

CREATE TABLE Fees (
    FeeID INT IDENTITY(1,1) PRIMARY KEY,
    StudentID INT,
    FeeAmount MONEY,
    PaymentStatus NVARCHAR(20),
    DueDate DATE,
    PaymentDate DATE,
    PaymentMethod NVARCHAR(50),
    FOREIGN KEY (StudentID) REFERENCES Students(StudentID)
);

CREATE TABLE Notices (
    NoticeID INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(100),
    Description NVARCHAR(MAX),
    Audience NVARCHAR(50), -- 'All', 'Class', 'Role'
    AudienceValue NVARCHAR(50),
    PostedBy INT,
    PostDate DATE,
    ExpiryDate DATE,
    FOREIGN KEY (PostedBy) REFERENCES Users(UserID)
);

CREATE TABLE Messages (
    MessageID INT IDENTITY(1,1) PRIMARY KEY,
    SenderID INT,
    ReceiverID INT,
    MessageBody NVARCHAR(MAX),
    Attachment NVARCHAR(255),
    Timestamp DATETIME,
    ReadStatus BIT,
    FOREIGN KEY (SenderID) REFERENCES Users(UserID),
    FOREIGN KEY (ReceiverID) REFERENCES Users(UserID)
);

CREATE TABLE DailyAnalytics (
    AnalyticsID INT IDENTITY(1,1) PRIMARY KEY,
    Date DATE,
    TotalStudents INT,
    TotalTeachers INT,
    TotalParents INT,
    AttendancePercentage FLOAT,
    FeeCollected MONEY,
    GradeAverage FLOAT
);

