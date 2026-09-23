DECLARE @AutomationId UNIQUEIDENTIFIER =
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';

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
    'Send entity to To Do',
    1
);

-- WHEN: Which event starts the automation?
INSERT INTO AutomationTriggers
(
    Id,
    AutomationId,
    EventType,
    SourceSystem
)
VALUES
(
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
    @AutomationId,
    'PlacementCreated',
    'PlacementBackend'
);

-- CONDITIONS: What must be true?
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
    'cccccccc-cccc-cccc-cccc-cccccccccccc',
    @AutomationId,
    'ColumnEquals',
    'PlacementBackend',
    '{
        "field": "columnId",
        "operator": "Equals",
        "value": "22222222-2222-2222-2222-222222222232"
    }'
);

-- THEN: What should happen?
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
    'dddddddd-dddd-dddd-dddd-dddddddddddd',
    @AutomationId,
    'CreatePlacement',
    'PlacementBackend',
    '{
        "boardId": "11111111-1111-1111-1111-111111111111",
        "columnId": "22222222-2222-2222-2222-222222222222"
    }',
    0
);