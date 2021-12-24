UPDATE Participants
SET IsRemoved = 1
WHERE GuildId = $GuildId AND UserId = $UserId