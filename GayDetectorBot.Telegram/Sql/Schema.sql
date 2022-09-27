CREATE TABLE CustomCommands (
	CommandId INTEGER PRIMARY KEY,
	ChatId INTEGER NOT NULL,
	UserAddedName TEXT NOT NULL,
	CommandPrefix TEXT NOT NULL,
	CommandContent TEXT NOT NULL
);

CREATE TABLE Participants (
	ParticipantId INTEGER PRIMARY KEY,
	ChatId INTEGER NOT NULL,
	Username TEXT NOT NULL,
	StartedAt INTEGER NOT NULL,
	IsRemoved INTEGER,
	FirstName TEXT,
	LastName TEXT
);

CREATE TABLE ChatInternal (
	ChatInternalId INTEGER PRIMARY KEY,
	ChatId INTEGER NOT NULL,
	LastGayUsername TEXT NULL,
	LastChecked INTEGER NULL
);

CREATE TABLE Gays (
	GayId INTEGER PRIMARY KEY,
	DateTimestamp INTEGER NOT NULL,
	ParticipantId INTEGER,
	FOREIGN KEY(ParticipantId) REFERENCES Participants(ParticipantId)
);