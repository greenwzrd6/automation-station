CREATE TABLE Automations
(
    Id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_Automations PRIMARY KEY,

    Name NVARCHAR(255) NOT NULL,
    Enabled BIT NOT NULL DEFAULT 1,
    AutomationTriggerable BIT NOT NULL DEFAULT 0
);

CREATE TABLE AutomationTriggers
(
    Id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_AutomationTriggers PRIMARY KEY,

    AutomationId UNIQUEIDENTIFIER NOT NULL,
    EventType NVARCHAR(255) NOT NULL,
    SourceSystem NVARCHAR(255) NULL,
    ConfigurationJson NVARCHAR(MAX) NULL,

    CONSTRAINT FK_AutomationTriggers_Automations
        FOREIGN KEY (AutomationId)
        REFERENCES Automations(Id)
);

CREATE TABLE AutomationConditions
(
    Id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_AutomationConditions PRIMARY KEY,

    AutomationId UNIQUEIDENTIFIER NOT NULL,
    ConditionType NVARCHAR(255) NOT NULL,
    SourceSystem NVARCHAR(255) NULL,
    ConfigurationJson NVARCHAR(MAX) NULL,

    CONSTRAINT FK_AutomationConditions_Automations
        FOREIGN KEY (AutomationId)
        REFERENCES Automations(Id)
);

CREATE TABLE AutomationActions
(
    Id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_AutomationActions PRIMARY KEY,

    AutomationId UNIQUEIDENTIFIER NOT NULL,
    ActionType NVARCHAR(255) NOT NULL,
    TargetSystem NVARCHAR(255) NULL,
    ConfigurationJson NVARCHAR(MAX) NULL,
    ExecutionOrder INT NOT NULL DEFAULT 0,

    CONSTRAINT FK_AutomationActions_Automations
        FOREIGN KEY (AutomationId)
        REFERENCES Automations(Id)
);

CREATE TABLE AutomationHistory
(
    Id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_AutomationHistory PRIMARY KEY,

    AutomationId UNIQUEIDENTIFIER NOT NULL,
    CausationEventId UNIQUEIDENTIFIER NOT NULL,
    TriggeredAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_AutomationHistory_Automations
        FOREIGN KEY (AutomationId)
        REFERENCES Automations(Id)
);

CREATE TABLE ProcessedMessages
(
    MessageId UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_ProcessedMessages PRIMARY KEY,

    ProcessedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE AutomationExecutions
(
    Id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_AutomationExecutions PRIMARY KEY,

    AutomationId UNIQUEIDENTIFIER NOT NULL,
    EventId UNIQUEIDENTIFIER NOT NULL,
    StartedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_AutomationExecutions_Automations
        FOREIGN KEY (AutomationId)
        REFERENCES Automations(Id),

    CONSTRAINT UQ_AutomationExecutions_AutomationId_EventId
        UNIQUE (AutomationId, EventId)
);

CREATE TABLE CorrelationLoopEvents
(
    Id UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT PK_CorrelationLoopEvents PRIMARY KEY,

    CorrelationId UNIQUEIDENTIFIER NOT NULL,
    EventId UNIQUEIDENTIFIER NOT NULL,
    ReceivedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);