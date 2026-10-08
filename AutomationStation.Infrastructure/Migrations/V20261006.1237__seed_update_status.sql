DECLARE @ReleasedAutomationId UNIQUEIDENTIFIER =
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaad';

DECLARE @TodoAutomationId UNIQUEIDENTIFIER =
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaae';


-- =====================================================
-- RELEASED -> COMPLETED
-- =====================================================

INSERT INTO Automations
(
    Id,
    Name,
    Enabled,
    AutomationTriggerable
)
VALUES
(
    @ReleasedAutomationId,
    N'Set TOJ task to Completed when placed in Released',
    1,
    1
);

INSERT INTO AutomationTriggers
(
    Id,
    AutomationId,
    EventType,
    SourceSystem
)
VALUES
(
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbe',
    @ReleasedAutomationId,
    N'PlacementCreated',
    N'Kanban'
);

INSERT INTO AutomationConditions
(
    Id,
    AutomationId,
    ConditionType,
    SourceSystem,
    ConfigurationJson
)
VALUES
(
    'cccccccc-cccc-cccc-cccc-cccccccccccf',
    @ReleasedAutomationId,
    N'ColumnEquals',
    N'Kanban',
    N'{
        "field": "payload.columnId",
        "operator": "Equals",
        "value": "22222222-2222-2222-2222-222222222232"
    }'
);

INSERT INTO AutomationActions
(
    Id,
    AutomationId,
    ActionType,
    TargetSystem,
    ConfigurationJson,
    ExecutionOrder
)
VALUES
(
    'dddddddd-dddd-dddd-dddd-dddddddddde0',
    @ReleasedAutomationId,
    N'UpdateTaskStatus',
    N'TojSystem',
    N'{
        "statusId": "3"
    }',
    0
);


-- =====================================================
-- TODO -> ACTIVE
-- =====================================================

INSERT INTO Automations
(
    Id,
    Name,
    Enabled,
    AutomationTriggerable
)
VALUES
(
    @TodoAutomationId,
    N'Set TOJ task to Active when placed in Todo',
    1,
    1
);

INSERT INTO AutomationTriggers
(
    Id,
    AutomationId,
    EventType,
    SourceSystem
)
VALUES
(
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbf',
    @TodoAutomationId,
    N'PlacementCreated',
    N'Kanban'
);

INSERT INTO AutomationConditions
(
    Id,
    AutomationId,
    ConditionType,
    SourceSystem,
    ConfigurationJson
)
VALUES
(
    'cccccccc-cccc-cccc-cccc-ccccccccccd0',
    @TodoAutomationId,
    N'ColumnEquals',
    N'Kanban',
    N'{
        "field": "payload.columnId",
        "operator": "Equals",
        "value": "22222222-2222-2222-2222-222222222222"
    }'
);

INSERT INTO AutomationActions
(
    Id,
    AutomationId,
    ActionType,
    TargetSystem,
    ConfigurationJson,
    ExecutionOrder
)
VALUES
(
    'dddddddd-dddd-dddd-dddd-dddddddddde1',
    @TodoAutomationId,
    N'UpdateTaskStatus',
    N'TojSystem',
    N'{
        "statusId": "1"
    }',
    0
);