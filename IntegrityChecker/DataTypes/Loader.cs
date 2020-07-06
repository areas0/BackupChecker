using System.IO;
using System.Text.Json;
using IntegrityChecker.Loaders;

namespace IntegrityChecker.DataTypes
{
    public class Loader
    {
        public static Folder LoadViaFile(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            string content = File.ReadAllText(path);
            Folder folder = new Folder(path, true);
            for (int i = 0; i < content.Length; i++)
            {
                int n = i;
                while (content[i] != '\n')
                {
                    i++;
                }

                string pathfile = content.Substring(n, i-n);
                n = i+1;
                i++;
                while (content[i] != '\n')
                {
                    i++;
                }

                string hash = content.Substring(n, i - n);
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