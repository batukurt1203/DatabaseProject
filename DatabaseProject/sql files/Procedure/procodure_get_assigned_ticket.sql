CREATE OR ALTER PROCEDURE pro_GET_ASSIGNED_TICKETS
    @EmployeeEmail NVARCHAR(100)
AS
BEGIN
    DECLARE @EmployeeID INT;

    -- 1. E-mail adresinden Çalışanın ID'sini bul (Person tablosundan)
    SELECT @EmployeeID = PersonID 
    FROM PERSON 
    WHERE Email = @EmployeeEmail AND PersonType = 'Employee';

    -- 2. Ticketları Getir
    -- Mantık: Ticket'ın ProductID'si == Employee_Product tablosundaki ProductID
    SELECT 
        t.TicketID,
        p.PName AS ProductName,
        cust.FullName AS CustomerName, -- Müşterinin adı (Person tablosundan)
        t.DateOpened,
        t.Status,
        t.IssueDescription
    FROM SUPPORT_TICKET t
    INNER JOIN PRODUCT_ p ON t.ProductID = p.ProductID
    INNER JOIN PERSON cust ON t.Customer_PersonID = cust.PersonID
    INNER JOIN EMPLOYEE_PRODUCT ep ON t.ProductID = ep.ProductID -- Kritik Eşleşme Burada!
    WHERE ep.EmployeeID = @EmployeeID -- Sadece bu çalışana atanmış ürünler
    ORDER BY t.DateOpened DESC; -- En yeniler üstte
END
GO