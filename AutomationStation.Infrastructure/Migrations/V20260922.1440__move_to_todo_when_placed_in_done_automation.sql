DECLARE @AutomationId UNIQUEIDENTIFIER =
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
DECLARE @AutomationId2 UNIQUEIDENTIFIER =
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaab';
DECLARE @AutomationId3 UNIQUEIDENTIFIER =
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaac';

-- AUTOMATION
INSERT INTO Automations
(
    Id,
    Name,
    Enabled,
    AutomationTriggerable
)
VALUES
(
    @AutomationId,
    'Send card to Todo when dropped in Released',
    1,
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
    'Kanban'
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
    'Kanban',
    '{
        "field": "payload.columnId",
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
    'Kanban',
    '{
        "boardId": "11111111-1111-1111-1111-111111111111",
        "columnId": "22222222-2222-2222-2222-222222222222"
    }',
    0
);

-- AUTOMATION
INSERT INTO Automations
(
    Id,
    Name,
    Enabled,
    AutomationTriggerable
)
VALUES
(
    @AutomationId2,
    'Send entity to Doing when dropped in Todo',
    1,
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
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbc',
    @AutomationId2,
    'PlacementCreated',
    'Kanban'
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
    'cccccccc-cccc-cccc-cccc-cccccccccccd',
    @AutomationId2,
    'ColumnEquals',
    'Kanban',
    '{
        "field": "payload.columnId",
        "operator": "Equals",
        "value": "22222222-2222-2222-2222-222222222222"
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
    'dddddddd-dddd-dddd-dddd-ddddddddddde',
    @AutomationId2,
    'CreatePlacement',
    'Kanban',
    '{
        "boardId": "11111111-1111-1111-1111-111111111111",
        "columnId": "22222222-2222-2222-2222-222222222223"
    }',
    0
);

-- AUTOMATION
INSERT INTO Automations
(
    Id,
    Name,
    Enabled,
    AutomationTriggerable
)
VALUES
(
    @AutomationId3,
    'Send entity to Released when dropped in Doing',
    1,
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
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbd',
    @AutomationId3,
    'PlacementCreated',
    'Kanban'
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
    'cccccccc-cccc-cccc-cccc-ccccccccccce',
    @AutomationId3,
    'ColumnEquals',
    'Kanban',
    '{
        "field": "payload.columnId",
        "operator": "Equals",
        "value": "22222222-2222-2222-2222-222222222223"
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
    'dddddddd-dddd-dddd-dddd-dddddddddddf',
    @AutomationId3,
    'CreatePlacement',
    'Kanban',
    '{
        "boardId": "11111111-1111-1111-1111-111111111111",
        "columnId": "22222222-2222-2222-2222-222222222232"
    }',
    0
);
