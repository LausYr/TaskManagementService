# TaskManagementService
WSL2:
Убедись, что Docker Desktop настроен на WSL2.

git clone <URL_репозитория>
cd <папка_проекта>

docker-compose up --build

API http://localhost:5000/

Уведомления, логировние и трасировка в логах контейнера.


CREATE FUNCTION dbo.GetDailyPayments
(
    @ClientId BIGINT,
    @StartDate DATE,
    @EndDate DATE
)
RETURNS TABLE
AS
RETURN
(
    WITH DateRange AS (
        SELECT 
            DATEADD(DAY, Numbers.n, @StartDate) AS PaymentDate
        FROM (
            SELECT TOP (
                CASE 
                    WHEN @StartDate > @EndDate THEN 0 
                    ELSE DATEDIFF(DAY, @StartDate, @EndDate) + 1 
                END
            )
                ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) - 1 AS n
            FROM 
                master.dbo.spt_values
            WHERE 
                type = 'P'
        ) AS Numbers
        WHERE 
            @StartDate <= @EndDate
    )
    SELECT 
        DR.PaymentDate,
        ISNULL(SUM(CP.Amount), 0) AS TotalAmount
    FROM 
        DateRange DR
        LEFT JOIN ClientPayments CP 
            ON DR.PaymentDate = CAST(CP.Dt AS DATE) 
            AND CP.ClientId = @ClientId
    GROUP BY 
        DR.PaymentDate
);
