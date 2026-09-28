CREATE TABLE Automations
(
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Enabled BIT NOT NULL DEFAULT 1
);

CREATE TABLE AutomationTriggers
(
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
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
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
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
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
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
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    AutomationId UNIQUEIDENTIFIER NOT NULL,
    CausationEventId UNIQUEIDENTIFIER NOT NULL,
    TriggeredAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT FK_AutomationHistory_Automations
        FOREIGN KEY (AutomationId)
        REFERENCES Automations(Id)
);