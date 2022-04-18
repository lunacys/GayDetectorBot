UPDATE Participants
SET IsRemoved = 1
WHERE ChatId = $ChatId AND Username = $Username