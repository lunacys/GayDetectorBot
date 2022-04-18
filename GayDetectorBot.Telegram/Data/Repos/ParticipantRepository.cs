using GayDetectorBot.Telegram.Models;

namespace GayDetectorBot.Telegram.Data.Repos
{
    public class ParticipantRepository
    {
        private readonly DataContext _context;

        public ParticipantRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> IsStartedForUser(string username, long chatId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Participant$Retrieve");
            cmd.Parameters.AddWithValue("$ChatId", chatId);
            cmd.Parameters.AddWithValue("$Username", username);

            int? commandId = null;

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    commandId = reader.GetInt32(0);
                }
            }

            return commandId.HasValue;
        }

        public async Task AddUser(string username, long chatId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Participant$Add");
            cmd.Parameters.AddWithValue("$ChatId", chatId);
            cmd.Parameters.AddWithValue("$Username", username);
            cmd.Parameters.AddWithValue("$StartedAt", DateTimeOffset.Now.ToUnixTimeSeconds());

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                { }
            }
        }

        public async Task RemoveUser(long chatId, string username)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Participant$Remove");
            cmd.Parameters.AddWithValue("$ChatId", chatId);
            cmd.Parameters.AddWithValue("$Username", username);

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                { }
            }
        }

        public async Task<IEnumerable<Participant>> RetrieveParticipants(long chatId)
        {
            await using var conn = _context.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = SqlReader.Load("Participant$RetrieveForChat");

            cmd.Parameters.AddWithValue("$ChatId", chatId);

            var result = new List<Participant>();

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    var partId = reader.GetInt32(0);
                    var chId = reader.GetInt64(1);
                    var username = reader.GetString(2);
                    var startedAt = reader.GetInt64(3);
                    bool isRemoved = false;
                    if (!reader.IsDBNull(4))
                        isRemoved = reader.GetBoolean(4);

                    result.Add(new Participant()
                    {
                        ParticipantId = partId,
                        ChatId = chId,
                        Username = username,
                        StartedAt = DateTimeOffset.FromUnixTimeSeconds(startedAt).DateTime,
                        IsRemoved = isRemoved
                    });
                }
            }

            return result;
        }
    }
}