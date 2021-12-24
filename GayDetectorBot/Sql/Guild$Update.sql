UPDATE GuildInternal SET 
	LastChecked = $LastChecked,
	LastGayUserId = $LastGay
WHERE GuildId = $GuildId