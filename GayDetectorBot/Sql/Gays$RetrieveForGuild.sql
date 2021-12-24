SELECT 
	Gays.GayId,
	Gays.DateTimestamp,
	Gays.ParticipantId,

	Participants.GuildId,
	Participants.UserId,
	Participants.StartedAt,
	Participants.IsRemoved
FROM Gays 
	INNER JOIN Participants ON Participants.ParticipantId = Gays.ParticipantId
WHERE GuildId = $GuildId