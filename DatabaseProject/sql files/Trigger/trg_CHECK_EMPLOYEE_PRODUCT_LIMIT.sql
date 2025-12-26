-- Prevents an employee from being assigned to more than two products
CREATE TRIGGER trg_CHECK_EMPLOYEE_PRODUCT_LIMIT
    ON EMPLOYEE_PRODUCT
    AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON; -- Prevents extra result messages

    DECLARE @EmployeeID INT;
    DECLARE @ProductID INT;

    -- Gets the inserted employee and product
SELECT @EmployeeID = EmployeeID, @ProductID = ProductID
FROM INSERTED;

-- Checks if employee exceeds product assignment limit
IF (SELECT COUNT(*)
    FROM EMPLOYEE_PRODUCT
    WHERE EmployeeID = @EmployeeID) >= 3
BEGIN
        PRINT 'An employee cannot work on more than two products.';
ROLLBACK TRANSACTION; -- Cancels the insert operation
END
END;
