CREATE TABLE BalanceBoardReadings (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TimeStamp DATETIME NOT NULL,
    TopLeft FLOAT NOT NULL,
    TopRight FLOAT NOT NULL,
    BottomLeft FLOAT NOT NULL,
    BottomRight FLOAT NOT NULL,
    Total FLOAT NOT NULL
);