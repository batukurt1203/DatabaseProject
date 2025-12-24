CREATE DATABASE CompanyDatabase
GO

USE CompanyDatabase
GO


CREATE TABLE Person (
                        PersonID int IDENTITY(1,1) PRIMARY KEY,
                        Name nvarchar(100) NOT NULL,
                        HireDate Date NOT NULL,


                        Email varchar(100) UNIQUE,
                        PASSWORD_ VARCHAR(16),
                        PERSONTYPE VARCHAR(20)
);

CREATE TABLE Customer(
                         CustomerID int IDENTITY(1,1) PRIMARY KEY,
                         Name nvarchar(100) NOT NULL,

                         City nvarchar(50),
                         Street nvarchar(100),
                         ZIPCODE varchar(20),
                         PhoneNumber varchar(20),
                         IndustrySector nvarchar(50)
);


CREATE TABLE Product (
                         ProductID int IDENTITY(1,1) PRIMARY KEY,
                         Name nvarchar(100) NOT NULL,
                         Description nvarchar(500),
                         Status nvarchar(50)
);


CREATE TABLE Manager (
                         PersonID int PRIMARY KEY,
                         FOREIGN KEY (PersonID) REFERENCES Person(PersonID)
);

CREATE TABLE Employee (
                          PersonID int PRIMARY KEY,
                          Manager_PersonID int NOT NULL,

                          FOREIGN KEY (PersonID) REFERENCES Person(PersonID),
                          FOREIGN KEY (Manager_PersonID) REFERENCES Manager(PersonID)
);

CREATE TABLE ManagerCustomerAssignment (
                                           AssignmentID int IDENTITY(1,1) PRIMARY KEY,
                                           Manager_PersonID int NOT NULL,
                                           CustomerID int NOT NULL,

                                           FOREIGN KEY (Manager_PersonID) REFERENCES Manager(PersonID),
                                           FOREIGN KEY (CustomerID) REFERENCES Customer(CustomerID)
);



CREATE TABLE EmployeeProductAssignment (
                                           AssignmentID int IDENTITY(1,1) PRIMARY KEY,
                                           Employee_PersonID int NOT NULL,
                                           ProductID int NOT NULL,

                                           FOREIGN KEY (Employee_PersonID) REFERENCES Employee(PersonID),
                                           FOREIGN KEY (ProductID) REFERENCES Product(ProductID)
);

CREATE TABLE ProductVersion(
                               VersionID varchar(20) NOT NULL,
                               ProductID int NOT NULL,
                               ReleaseDate datetime DEFAULT GETDATE(),
                               Description nvarchar(500),


                               PRIMARY KEY(VersionID, ProductID),
                               FOREIGN KEY(ProductID) REFERENCES Product(ProductID)
);


CREATE TABLE Feedback(
                         FeedbackID int IDENTITY(1,1) PRIMARY KEY,
                         Date date DEFAULT GETDATE(),
                         Comment nvarchar(MAX),
                         Rating int CHECK (Rating >= 1 AND Rating <= 5),

                         ProductID int NOT NULL,
                         CustomerID int NOT NULL,

                         FOREIGN KEY (ProductID) REFERENCES Product(ProductID),
                         FOREIGN KEY (CustomerID) REFERENCES Customer(CustomerID)
);


CREATE TABLE Licence(
                        LicenceID int IDENTITY(200000,1) PRIMARY KEY,
                        StartDate datetime DEFAULT GETDATE(),
                        LicenceTerm int CHECK(LicenceTerm > 0) NOT NULL,

                        EndDate AS DATEADD(DAY, LicenceTerm, StartDate) PERSISTED,

                        NumberOfComputer int,
                        Price decimal(18,2),

                        ProductID int NOT NULL,
                        CustomerID int NOT NULL,

                        FOREIGN KEY (ProductID) REFERENCES Product(ProductID),
                        FOREIGN KEY (CustomerID) REFERENCES Customer(CustomerID)
);


CREATE TABLE SupportTicket (
                               TicketID int IDENTITY(1,1) PRIMARY KEY,
                               DateOpened datetime DEFAULT GETDATE(),
                               DateClosed datetime,
                               Status nvarchar(50) DEFAULT 'Open',
                               IssueDescription nvarchar(MAX),

                               ProductID int NOT NULL,
                               CustomerID int NOT NULL,
                               Employee_PersonID int,

                               FOREIGN KEY (ProductID) REFERENCES Product(ProductID),
                               FOREIGN KEY (CustomerID) REFERENCES Customer(CustomerID),
                               FOREIGN KEY (Employee_PersonID) REFERENCES Employee(PersonID)
);

CREATE INDEX IX_Customer_Name ON Customer (Name);
CREATE INDEX IX_Licence_Customer ON Licence (CustomerID);