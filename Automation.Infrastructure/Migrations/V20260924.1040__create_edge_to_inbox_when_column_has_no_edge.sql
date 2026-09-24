DECLARE @AutomationId UNIQUEIDENTIFIER =
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaab';

-- AUTOMATION
INSERT INTO Automations
(
    Id,
    Name,
    Enabled
)
VALUES
(
    @AutomationId,
    'Connect column without edge to Inbox',
    1
);

-- WHEN
INSERT INTO AutomationTriggers
(
    Id,
    AutomationId,
    EventType,
    SourceSystem
)
VALUES
(
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbc',
    @AutomationId,
    'ColumnHasNoEdge',
    'PlacementBackend'
);

-- THEN
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
    'dddddddd-dddd-dddd-dddd-ddddddddddde',
    @AutomationId,
    'CreateColumnEdge',
    'PlacementBackend',
    '{
        "toColumnId": "22222222-2222-2222-2222-222222222220"
    }',
    0
);