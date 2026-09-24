DECLARE @AutomationId UNIQUEIDENTIFIER =
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaac';

INSERT INTO Automations
(
    Id,
    Name,
    Enabled
)
VALUES
(
    @AutomationId,
    'Show random word in specific column',
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
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbd',
    @AutomationId,
    'PlacementCreated',
    'PlacementBackend'
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
    'cccccccc-cccc-cccc-cccc-cccccccccccd',
    @AutomationId,
    'ColumnEquals',
    'PlacementBackend',
    '{
        "field": "columnId",
        "operator": "Equals",
        "value": "22222222-2222-2222-2222-222222222223"
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
    'dddddddd-dddd-dddd-dddd-dddddddddddf',
    @AutomationId,
    'GetRandomWord',
    'WordApi',
    '{}',
    0
);