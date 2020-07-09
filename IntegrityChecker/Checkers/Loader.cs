using System;
using System.IO;
using System.Text.Json;
using IntegrityChecker.DataTypes;

namespace IntegrityChecker.Checkers
{
    public static class Loader
    {
        // Deprecated load via file with old format
/*
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
*/
        // Load via json format the folder
        public static void LoadJson(string jsonString, ref Folder folder)
        {
            try
            {
                folder = JsonSerializer.Deserialize<ManualFolder>(jsonString).Export();
            }
            catch (Exception e)
            {
                Logger.Instance.Log(Logger.Type.Error, $"LoadJson failed: data failure: \n {jsonString} \n {e.Message} \n {e.StackTrace} ");
                throw;
            }

            Logger.Instance.Log(Logger.Type.Ok, "Successfully loaded the folder");
        }
    }
}