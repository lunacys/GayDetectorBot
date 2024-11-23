using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace GayDetectorBot.Data
{
    public class DataContext
    {
        public static readonly string SchemaFilename = "Schema";

        private readonly string _connectionString;

        public string DbName { get; }

        public DataContext(string connectionString, string dbName)
        {
            _connectionString = connectionString;
            DbName = dbName;
        }

        public SqliteConnection CreateConnection() => new SqliteConnection(_connectionString);

        internal void Initialize(bool forceDrop = false)
        {
            Console.WriteLine("  [DB]: Initializing User Repository");

            if (forceDrop && File.Exists(DbName))
                File.Delete(DbName);

            if (!File.Exists(DbName))
            {
                Console.WriteLine("  [DB]: Database doesn't exist, initializing...");

                using var conn = CreateConnection();
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = SqlReader.Load(SchemaFilename);

                using (var reader = cmd.ExecuteReader())
                { }

                Console.WriteLine("  [DB]: Database initialization done");
            }
            else
            {
                Console.WriteLine("  [DB]: Database already exists, skipping initialization");
            }
        }
    }
}