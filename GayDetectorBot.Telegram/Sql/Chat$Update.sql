UPDATE ChatInternal SET 
	LastChecked = $LastChecked,
	LastGayUsername = $LastGay
WHERE ChatId = $ChatId