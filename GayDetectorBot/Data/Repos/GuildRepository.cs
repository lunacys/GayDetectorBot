using System;
using System.Threading.Tasks;

namespace GayDetectorBot.Data.Repos
{
    public class GuildRepository
    {
        private readonly DataContext _context;

        public GuildRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> GuildExists(ulong guildId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Guild$GetLastChecked");
            cmd.Parameters.AddWithValue("$GuildId", guildId);

            long? lastChecked = null;

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    lastChecked = reader.GetInt64(0);
                }
            }

            return lastChecked.HasValue;
        }


        public async Task<DateTimeOffset?> GuildLastChecked(ulong guildId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Guild$GetLastChecked");
            cmd.Parameters.AddWithValue("$GuildId", guildId);

            DateTimeOffset? lastChecked = null;

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    var i = reader.GetInt64(0);

                    lastChecked = DateTimeOffset.FromUnixTimeSeconds(i).ToLocalTime();
                }
            }

            return lastChecked;
        }

        public async Task<ulong?> GetLastGay(ulong guildId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Guild$GetLastGay");
            cmd.Parameters.AddWithValue("$GuildId", guildId);

            ulong? lastGay = null;

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    if (reader.IsDBNull(0))
                    {
                        lastGay = null;
                    }
                    else
                    {
                        var i = reader.GetDecimal(0);

                        lastGay = (ulong)i;
                    }
                }
            }

            return lastGay;
        }

        public async Task GuildUpdate(ulong guildId, ulong userId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Guild$Update");
            cmd.Parameters.AddWithValue("$GuildId", guildId);
            cmd.Parameters.AddWithValue("$LastChecked", DateTimeOffset.Now.ToUnixTimeSeconds());
            cmd.Parameters.AddWithValue("$LastGay", userId);


            /*
             * 	LastChecked = $LastChecked,
	LastGayUserId = $LastGay
WHERE GuildId = $GuildId
             */

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                { }
            }
        }

        public async Task GuildAdd(ulong guildId, DateTimeOffset? dateTime, ulong lastGayUserId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Guild$Add");
            cmd.Parameters.AddWithValue("$GuildId", guildId);
            cmd.Parameters.AddWithValue("$LastChecked", dateTime?.ToUnixTimeSeconds());
            cmd.Parameters.AddWithValue("$LastGayUserId", lastGayUserId);

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                { }
            }
        }
    }
}