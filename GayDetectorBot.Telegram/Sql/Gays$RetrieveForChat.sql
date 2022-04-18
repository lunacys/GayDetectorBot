SELECT 
	Gays.GayId,
	Gays.DateTimestamp,
	Gays.ParticipantId,

	Participants.ChatId,
	Participants.Username,
	Participants.StartedAt,
	Participants.IsRemoved
FROM Gays 
	INNER JOIN Participants ON Participants.ParticipantId = Gays.ParticipantId
WHERE ChatId = $ChatId