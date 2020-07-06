using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace IntegrityChecker.Loaders
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

        public Folder(string path ="", bool manual = false)
        {
            _path = path;
            if (manual)
                return;
            if (!Directory.Exists(path))
            {
                throw new ArgumentException("Integrity checker: init failed");
            }
            
            LoadFolder(path);
            Console.WriteLine("All files successfully found");
            Generate();
            Console.WriteLine("All SHA1 were generated");
            
            DirectoryInfo directoryInfo = new DirectoryInfo(_path);
            string name = directoryInfo.Name;
            string fullname = directoryInfo.FullName;
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
            int i = 0;
            int n = _files.Count;
            foreach (var file in _files)
            {
                Console.WriteLine((i+1)+"/"+n+": "+file);
                _sums.Add(new FileSum(file));
                i++;
            }
        }
        //Legacy export format (deprecated)
        public void Export()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(_path);
            string name = directoryInfo.Name;
            string fullname = directoryInfo.FullName;
            string result = String.Empty;
            foreach (var fileSum in _sums)
            {
                result += fileSum.Path.Substring(fullname.Length-name.Length) + "\n" + fileSum.Sum + "\n";
            }

            string filename = $"Export - {name} {DateTime.Today.Date.ToLongDateString()}.sha1";
            File.Create(filename).Close();
            File.WriteAllText(filename, result);
            Console.WriteLine("Exported to "+filename);
        }

        public string ExportJson()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(_path);
            string name = directoryInfo.Name;
            //serializing the current object
            string jsonString = JsonSerializer.Serialize(this);
            
            //Creating a file with json data in it
            string filename = $"Export - {name} {DateTime.Today.Date.ToLongDateString()}.json";
            File.Create(filename).Close();
            File.WriteAllText(filename, jsonString);
            
            Console.WriteLine("Exported to "+filename);
            return jsonString;

        }
    }
}