USE AKADEMEDYA
GO

-- View showing products with their versions
CREATE VIEW VIEW_ALL_PRODUCT_AND_VERSIONS
AS
SELECT p.PName, pv.VersionID, pv.PVDate, pv.PVDescription
FROM PRODUCT_VERSION pv
         INNER JOIN PRODUCT_ p ON pv.ProductID = p.ProductID
    GO

-- View showing employees and the products they work on
CREATE VIEW VIEW_EMPLOYEE_AND_PRODUCT_NAME AS
SELECT per.FullName, pp.PName
FROM EMPLOYEE e
         INNER JOIN PERSON per ON e.EmployeeID = per.PersonID
         LEFT OUTER JOIN EMPLOYEE_PRODUCT ep ON e.EmployeeID = ep.EmployeeID
         INNER JOIN PRODUCT_ pp ON pp.ProductID = ep.ProductID
    GO

-- Ends all licences for a specific product
CREATE PROCEDURE END_ALL_LICENCES_FOR_PRODUCT
    @PRODUCTID INT
AS
BEGIN
UPDATE lcn
SET lcn.LicenceTerm = 0
    FROM LICENCE lcn
WHERE lcn.ProductID = @PRODUCTID
END;
GO

-- Creates a new employee and assigns a manager
CREATE PROCEDURE pro_CreateEmployee
    @FullNamePar nvarchar(50),
    @HireDatePar Date,
    @EmailPar varchar(100),
    @PasswordPar VARCHAR(16),
    @ManagerMail varchar(100)
AS
BEGIN
INSERT INTO PERSON(FullName,HireDate,Email,PASSWORD_, PERSONTYPE)
VALUES (@FullNamePar,@HireDatePar,@EmailPar, @PasswordPar, 'employee')

    INSERT INTO EMPLOYEE(EmployeeID,ManagerID)
VALUES (
    (SELECT p.PersonID FROM PERSON p WHERE p.Email = @EmailPar),
    (SELECT m.PersonID FROM PERSON m WHERE m.Email = @ManagerMail)
    )
END
GO

-- Creates a new customer and links it to a company
CREATE PROCEDURE pro_CreateCustomer
    @FullNamePar nvarchar(50),
    @HireDatePar Date,
    @EmailPar varchar(100),
    @PasswordPar VARCHAR(16),
    @CompanyName nvarchar(50)
AS
BEGIN
INSERT INTO PERSON(FullName,HireDate,Email,PASSWORD_, PERSONTYPE)
VALUES (@FullNamePar,@HireDatePar,@EmailPar, @PasswordPar, 'customer')

    INSERT INTO CUSTOMER(CustomerID,CompanyID)
VALUES (
    (SELECT p.PersonID FROM PERSON p WHERE p.Email = @EmailPar),
    (SELECT c.CompanyID FROM Company c WHERE c.CompanyName = @CompanyName)
    )
END
GO

-- Creates a bug report for a product version
CREATE PROCEDURE pro_CREATE_BUG_REPORT
    @messagePar nvarchar(500),
    @fdatePar date,
    @productnamePar nvarchar(50),
    @companynamePar nvarchar(50),
    @versionIDPar decimal(5,2)
AS
BEGIN
INSERT INTO FEEDBACK(FeedbackDate,Message_,ProductID, CompanyID, Feedback_Type)
VALUES (
           @fdatePar,
           @messagePar,
           (SELECT p.ProductID FROM PRODUCT_ p WHERE p.PName = @productnamePar),
           (SELECT c.CompanyID FROM Company c WHERE c.CompanyName = @companynamePar),
           'bug report'
       )

    INSERT INTO BUGREPORT(FeedbackID,VersionID,ProductID)
VALUES (
    (SELECT MAX(f.FeedbackID) FROM FEEDBACK f),
    @versionIDPar,
    (SELECT p.ProductID FROM PRODUCT_ p WHERE p.PName = @productnamePar)
    )
END
GO

-- Creates a feature request with rating
CREATE PROCEDURE pro_CREATE_FEATURE_REQUEST
    @messagePar nvarchar(500),
    @fdatePar date,
    @productnamePar nvarchar(50),
    @companynamePar nvarchar(50),
    @ratingPar int
AS
BEGIN
INSERT INTO FEEDBACK(FeedbackDate,Message_,ProductID, CompanyID, Feedback_Type)
VALUES (
           @fdatePar,
           @messagePar,
           (SELECT p.ProductID FROM PRODUCT_ p WHERE p.PName = @productnamePar),
           (SELECT c.CompanyID FROM Company c WHERE c.CompanyName = @companynamePar),
           'feature request'
       )

    INSERT INTO FEATUREREQUEST(FeedbackID,Rating)
VALUES (
    (SELECT MAX(f.FeedbackID) FROM FEEDBACK f),
    @ratingPar
    )
END
GO

-- Creates a licence and generates an invoice
CREATE PROCEDURE pro_CREATE_LICENCE
    @LicenceTermPar int,
    @FeePar money,
    @StartDatePar datetime,
    @ProductNamePar nvarchar(50),
    @CompanyNamePar nvarchar(50)
AS
BEGIN
INSERT INTO INVOICE(Fee) VALUES (@FeePar)

    INSERT INTO LICENCE(LicenceTerm,StartDate,InvoiceID,ProductID,CompanyID)
VALUES (
    @LicenceTermPar,
    @StartDatePar,
    (SELECT MAX(i.InvoiceID) FROM INVOICE i),
    (SELECT p.ProductID FROM PRODUCT_ p WHERE p.PName = @ProductNamePar),
    (SELECT c.CompanyID FROM COMPANY c WHERE c.CompanyName = @CompanyNamePar)
    )
END
