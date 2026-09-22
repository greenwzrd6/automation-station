INSERT INTO Automations
(
    Id,
    Name,
    Enabled
)
VALUES
(
    '...',
    'Move card to "Todo" when dropped in "Done"',
    1
);

INSERT INTO AutomationTriggers
(
    Id,
    AutomationId,
    EventType,
    SourceSystem,
    ConfigurationJson
)
VALUES
(
    '...',
    '...',
    'CardDropped',
    'backend-lia',
    '{
        "columnId": "@DoneColumnId1"
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
    '...',
    '...',
    'MoveCard',
    'backend-lia',
    '{
        "targetColumnId": "@TodoColumnId1"
    }',
    0
);