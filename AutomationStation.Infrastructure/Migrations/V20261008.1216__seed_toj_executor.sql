DECLARE @TojExecutorId UNIQUEIDENTIFIER =
    'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee';

INSERT INTO AutomationExecutors
(
    Id,
    CompanyId,
    TargetSystem,
    Name,
    Enabled
)
VALUES
(
    @TojExecutorId,
    3,
    N'TojSystem',
    N'TOJ automation executor – company 3',
    1
);

UPDATE AutomationActions
SET AutomationExecutorId = @TojExecutorId
WHERE Id IN
(
    'dddddddd-dddd-dddd-dddd-dddddddddde0',
    'dddddddd-dddd-dddd-dddd-dddddddddde1'
);