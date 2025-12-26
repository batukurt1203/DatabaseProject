DROP TABLE IF EXISTS SUPPORT_TICKET; -- Varsa eskisini sil
GO

CREATE TABLE SUPPORT_TICKET (
    TicketID INT PRIMARY KEY IDENTITY(1,1),
    ProductID INT NOT NULL,
    Customer_PersonID INT NOT NULL, -- Foreign Key to PERSON table
    Employee_PersonID INT NULL,     -- Foreign Key to PERSON table (Destek veren çalışan)
    DateOpened DATETIME DEFAULT GETDATE(),
    DateClosed DATETIME NULL,
    Status NVARCHAR(50) DEFAULT 'Open',
    IssueDescription NVARCHAR(MAX),
    
    -- Ürün tablosuna bağlantı
    CONSTRAINT FK_Ticket_Product FOREIGN KEY (ProductID) REFERENCES PRODUCT_(ProductID),
    
    -- Müşteri için PERSON tablosuna bağlantı (Customer tablosuna değil!)
    CONSTRAINT FK_Ticket_CustomerPerson FOREIGN KEY (Customer_PersonID) REFERENCES PERSON(PersonID),
    
    -- Çalışan için de PERSON tablosuna bağlantı
    CONSTRAINT FK_Ticket_EmployeePerson FOREIGN KEY (Employee_PersonID) REFERENCES PERSON(PersonID)
);
GO

CREATE OR ALTER PROCEDURE pro_CREATE_SUPPORT_TICKET
    @CustomerEmail NVARCHAR(100),
    @ProductName NVARCHAR(100),
    @IssueDescription NVARCHAR(MAX)
AS
BEGIN
    DECLARE @PersonID INT;
    DECLARE @ProductID INT;

    -- 1. E-mail adresine sahip VE Tipi 'Customer' olan kişinin PersonID'sini bul
    -- Not: PersonType sütun isminiz farklıysa (örn: UserType, Type vb.) burayı güncelleyin.
    SELECT @PersonID = PersonID 
    FROM PERSON 
    WHERE Email = @CustomerEmail 
      AND PersonType = 'Customer'; -- Sadece Müşterileri bul

    -- 2. Ürün ID'sini bul
    SELECT @ProductID = ProductID 
    FROM PRODUCT_ 
    WHERE PName = @ProductName;

    -- 3. Kayıt İşlemi
    IF (@PersonID IS NOT NULL AND @ProductID IS NOT NULL)
    BEGIN
        INSERT INTO SUPPORT_TICKET (ProductID, Customer_PersonID, Employee_PersonID, DateOpened, Status, IssueDescription)
        VALUES (@ProductID, @PersonID, NULL, GETDATE(), 'Open', @IssueDescription);
    END
    ELSE
    BEGIN
        -- Hata durumunda bilgi ver (C# tarafında catch bloğuna düşer)
        DECLARE @ErrorMsg NVARCHAR(200) = '';
        IF @PersonID IS NULL SET @ErrorMsg = 'Bu email adresine sahip bir müşteri bulunamadı.';
        IF @ProductID IS NULL SET @ErrorMsg = @ErrorMsg + ' Belirtilen ürün bulunamadı.';
        
        RAISERROR (@ErrorMsg, 16, 1);
    END
END
GO