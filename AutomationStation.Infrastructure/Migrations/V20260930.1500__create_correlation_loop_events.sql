CREATE TABLE CorrelationLoopEvents (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    CorrelationId UNIQUEIDENTIFIER NOT NULL,
    EventId UNIQUEIDENTIFIER NOT NULL,
    ReceivedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE INDEX IX_CorrelationLoopEvents_CorrelationId_ReceivedAt
ON CorrelationLoopEvents(CorrelationId, ReceivedAt);

CREATE UNIQUE INDEX UX_CorrelationLoopEvents_CorrelationId_EventId
ON CorrelationLoopEvents(CorrelationId, EventId);
