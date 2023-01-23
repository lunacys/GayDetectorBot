using GayDetectorBot.Telegram.Models;

namespace GayDetectorBot.Telegram.Data.Repos
{
    public class ChatRepository
    {
        private readonly DataContext _context;

        public ChatRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> ChatExists(long chatId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Chat$GetLastChecked");
            cmd.Parameters.AddWithValue("$ChatId", chatId);

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


        public async Task<DateTimeOffset?> ChatLastChecked(long chatId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Chat$GetLastChecked");
            cmd.Parameters.AddWithValue("$ChatId", chatId);

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

        public async Task<string?> GetLastGay(long chatId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Chat$GetLastGay");
            cmd.Parameters.AddWithValue("$ChatId", chatId);

            string? lastGay = null;

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
                        var i = reader.GetString(0);

                        lastGay = i;
                    }
                }
            }

            return lastGay;
        }

        public async Task ChatUpdate(long chatId, string username)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Chat$Update");
            cmd.Parameters.AddWithValue("$ChatId", chatId);
            cmd.Parameters.AddWithValue("$LastChecked", DateTimeOffset.Now.ToUnixTimeSeconds());
            cmd.Parameters.AddWithValue("$LastGay", username);

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                { }
            }
        }

        public async Task ChatAdd(long chatId, DateTimeOffset? dateTime, string lastGayUsername)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Chat$Add");
            cmd.Parameters.AddWithValue("$ChatId", chatId);
            cmd.Parameters.AddWithValue("$LastChecked", dateTime?.ToUnixTimeSeconds());
            cmd.Parameters.AddWithValue("$LastGayUsername", lastGayUsername);

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                { }
            }
        }

        public async Task<IEnumerable<ChatInternal>> RetrieveAll()
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Chat$RetrieveAll");

            var result = new List<ChatInternal>();

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32(0);
                    var chatId = reader.GetInt64(1);
                    var lastGay = reader.IsDBNull(2) ? null : reader.GetString(2);
                    long? lastTime = reader.IsDBNull(3) ? null : reader.GetInt64(3);

                    result.Add(new ChatInternal
                    {
                        ChatInternalId = id,
                        ChatId = chatId,
                        LastGayUsername = lastGay,
                        LastChecked = lastTime.HasValue ? DateTimeOffset.FromUnixTimeSeconds(lastTime.Value).DateTime : null
                    });
                }
            }

            return result;
        }
    }
}