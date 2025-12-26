GO
-- Automatically marks a person as manager when assigned to an employee
CREATE TRIGGER trg_CHECK_MANAGER
    ON EMPLOYEE
    AFTER INSERT, UPDATE
                      AS
BEGIN
    DECLARE @selectedManager int

    SET @selectedManager = (
        SELECT i.ManagerID 
        FROM inserted i
    )

UPDATE PERSON
SET PERSONTYPE = 'manager'
    FROM PERSON p
WHERE p.PersonID = @selectedManager
END

-- Used for testing employee records
-- SELECT * FROM EMPLOYEE
