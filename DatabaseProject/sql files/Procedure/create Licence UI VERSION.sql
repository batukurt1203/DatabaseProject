GO
-- Creates a licence with automatic start date and fee calculation
CREATE PROCEDURE pro_CREATE_LICENCE_UI
    @LicenceTermPar int,
    @ProductNamePar nvarchar(50),
    @CompanyNamePar nvarchar(50)
AS
BEGIN
    DECLARE @fee money
    DECLARE @calculatedStartDate datetime

    -- Checks if the company already has a licence for the product
    IF EXISTS (
        SELECT 1
        FROM LICENCE li
        INNER JOIN PRODUCT_ pr ON li.ProductID = pr.ProductID
        INNER JOIN Company co ON co.CompanyID = li.CompanyID
        WHERE pr.PName = @ProductNamePar 
          AND co.CompanyName = @CompanyNamePar
    )
BEGIN
        -- Sets start date to the end date of the latest licence
        SET @calculatedStartDate = (
            SELECT MAX(li.EndDate)
            FROM LICENCE li
            INNER JOIN PRODUCT_ pr ON li.ProductID = pr.ProductID
            INNER JOIN Company co ON co.CompanyID = li.CompanyID
            WHERE pr.PName = @ProductNamePar 
              AND co.CompanyName = @CompanyNamePar
        )
END
ELSE
BEGIN
        -- Sets start date to current date if no previous licence exists
        SET @calculatedStartDate = GETDATE()
END

    -- Calculates licence fee based on licence term
    SET @fee = CAST(@LicenceTermPar * 300 AS MONEY)

    -- Calls main licence creation procedure
    EXEC dbo.pro_CREATE_LICENCE 
        @LicenceTermPar, 
        @fee, 
        @calculatedStartDate, 
        @ProductNamePar, 
        @CompanyNamePar;
END
GO

-- Example usage
-- EXEC dbo.pro_CREATE_LICENCE_UI 150, 'IRONIC-OSGB', 'Enerjisa Uretim'
