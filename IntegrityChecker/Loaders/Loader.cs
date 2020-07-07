using System.IO;
using System.Text.Json;
using IntegrityChecker.DataTypes;

namespace IntegrityChecker.Loaders
{
    public static class Loader
    {
        public static Folder LoadViaFile(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            var content = File.ReadAllText(path);
            var folder = new Folder(path, true);
            for (var i = 0; i < content.Length; i++)
            {
                var n = i;
                while (content[i] != '\n')
                {
                    i++;
                }

                var pathfile = content.Substring(n, i-n);
                n = i+1;
                i++;
                while (content[i] != '\n')
                {
                    i++;
                }

                var hash = content.Substring(n, i - n);
                folder.Sums.Add(new FileSum(pathfile, hash, true));
            }

            return folder;
        }

        public static void LoadJson(string jsonString, ref Folder folder)
        {
            folder = JsonSerializer.Deserialize<ManualFolder>(jsonString).Export();
        }
    }
}