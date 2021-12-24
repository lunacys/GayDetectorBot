CREATE TABLE CustomCommands (
	CommandId INTEGER PRIMARY KEY,
	GuildId INTEGER NOT NULL,
	UserAddedId INTEGER NOT NULL,
	CommandPrefix TEXT NOT NULL,
	CommandContent TEXT NOT NULL
);

CREATE TABLE Participants (
	ParticipantId INTEGER PRIMARY KEY,
	GuildId INTEGER NOT NULL,
	UserId INTEGER NOT NULL,
	StartedAt INTEGER NOT NULL,
	IsRemoved INTEGER
);

CREATE TABLE GuildInternal (
	GuildInternalId INTEGER PRIMARY KEY,
	GuildId INTEGER NOT NULL,
	LastGayUserId INTEGER NULL,
	LastChecked INTEGER NULL
);

CREATE TABLE Gays (
	GayId INTEGER PRIMARY KEY,
	DateTimestamp INTEGER NOT NULL,
	ParticipantId INTEGER,
	FOREIGN KEY(ParticipantId) REFERENCES Participants(ParticipantId)
);