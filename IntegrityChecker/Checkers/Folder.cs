using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace IntegrityChecker.Checkers
{
    public class Folder
    {
        private string _path;
        private List<string> _files = new List<string>();
        private List<FileSum> _sums = new List<FileSum>();

        public List<FileSum> Sums
        {
            get => _sums;
            set => _sums = value;
        }

        public List<string> Files
        {
            get => _files;
            set => _files = value;
        }

        public string Path => _path;

        public Folder(string path ="", bool manual = false, bool onlyPath = false)
        {
            _path = path;
            // In case of manual load, we don't initialize all the fields
            if (manual)
                return;
            
            if (!Directory.Exists(path))
            {
                Logger.Instance.Log(Logger.Type.Error, $"Folder constructor: 404 directory not found at {path}");
                throw new ArgumentException("Integrity checker: init failed");
            }
            
            Logger.Instance.Log(Logger.Type.Ok, "Folder: started loading the folder");
            LoadFolder(path);
            
            Logger.Instance.Log(Logger.Type.Ok, "Folder: Files found");
            // FileList specific use: don't compute the hashes
            if (onlyPath)
                return;
            
            Console.WriteLine("All files successfully found, starting generation of hashes");
            Generate();
            Logger.Instance.Log(Logger.Type.Ok, "Folder: generation finished");
            Console.WriteLine("All SHA256 were generated");
            
            var directoryInfo = new DirectoryInfo(_path);
            var name = directoryInfo.Name;
            var fullname = directoryInfo.FullName;
            foreach (var fileSum in _sums)
            {
                fileSum.Path = fileSum.Path.Substring(fullname.Length - name.Length);
            }
        }
        // Finds recursively all files & all folders in the current _path
        private void LoadFolder(string path)
        {
            foreach (var file in new DirectoryInfo(path).EnumerateFiles())
            {
                _files.Add(file.FullName);
            }

            foreach (var dir in new DirectoryInfo(path).EnumerateDirectories())
            {
                LoadFolder(dir.FullName);
            }
        }

        // Generate: loads all files in the folder and generates its hash
        private void Generate()
        {
            var i = 0;
            var n = _files.Count;
            foreach (var file in _files)
            {
                Console.Clear();
                //Console.WriteLine((i+1)+"/"+n+": "+file);
                var completion = (int) ((i + 1) /(float) n * 100f);
                Console.WriteLine($"Completion: {completion}% ({i+1}/{n})");
                Console.WriteLine($"Current file: {file}");
                _sums.Add(new FileSum(file));
                i++;
            }
        }
        //Legacy export format (deprecated)
        public void Export()
        {
            var directoryInfo = new DirectoryInfo(_path);
            var name = directoryInfo.Name;
            var fullname = directoryInfo.FullName;
            var result = String.Empty;
            foreach (var fileSum in _sums)
            {
                result += fileSum.Path.Substring(fullname.Length-name.Length) + "\n" + fileSum.Sum + "\n";
            }

            var filename = $"Export - {name} {DateTime.Today.Date.ToLongDateString()}.sha1";
            File.Create(filename).Close();
            File.WriteAllText(filename, result);
            Console.WriteLine("Exported to "+filename);
        }

        public string ExportJson()
        {
            var directoryInfo = new DirectoryInfo(_path);
            var name = directoryInfo.Name;
            //serializing the current object
            var jsonString = JsonSerializer.Serialize(this);
            
            //Creating a file with json data in it
            var filename = $"Export - {name} {DateTime.Today.Date.ToLongDateString()}.json";
            try
            {
                //trying to write the folder exported to a file
                File.Create(filename).Close();
                File.WriteAllText(filename, jsonString);
            }
            catch (Exception e)
            {
                Logger.Instance.Log(Logger.Type.Error, $"ExportJson failed: file writing failed, make " +
                                                       $"sure you have the appropriate rights. \n {e.Message} " +
                                                       $"\n {e.StackTrace}");
                throw;
            }
            Logger.Instance.Log(Logger.Type.Ok, $"ExportJson: successfully exported to {filename}");
            Console.WriteLine("Exported to "+filename);
            return jsonString;

        }
    }
}