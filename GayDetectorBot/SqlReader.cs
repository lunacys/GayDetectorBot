using System.Collections.Generic;
using System.IO;

namespace GayDetectorBot
{
    public static class SqlReader
    {
        private static readonly Dictionary<string, string> LoadedFiles = new ();

        public static string Load(string filename)
        {
            if (!LoadedFiles.ContainsKey(filename))
            {
                using var sr = new StreamReader(Path.Combine("Sql", filename + ".sql"));

                var data = sr.ReadToEnd();

                LoadedFiles[filename] = data;
            }

            return LoadedFiles[filename];
        }
    }
}