using System;
using System.IO;
using IntegrityChecker.DataTypes;

namespace IntegrityChecker.Loaders
{
    public class Checker
    {
        private Folder _originalFolder = null;

        public Folder OriginalFolder
        {
            get => _originalFolder;
            set => _originalFolder = value;
        }

        private string _backup;
        private string Original;
        private Folder _backupFolder = null;
        private int _errors = 0;

        public int Errors => _errors;

        public Folder BackupFolder
        {
            get => _backupFolder;
            set => _backupFolder = value;
        }

        public Checker(string original, string backup, bool manual = false)
        {
            // setting up paths (unused)
            Original = original;
            _backup = backup;
            
            //manual load case used for server purposes
            if (manual)
                return;
            //Loading the json files
            var jsonString = File.ReadAllText(original);
            var jsonString2 = File.ReadAllText(_backup);
            // Deserialize the folders 
            Loader.LoadJson(jsonString, ref _originalFolder);
            Loader.LoadJson(jsonString2, ref _backupFolder);
        }

        public string CheckFolders()
        {
            // if there is a missing file, we search which one is missing and return it as the error
            if (_backupFolder.Sums.Count != _originalFolder.Sums.Count)
            {
                Console.Error.WriteLine("Failure: {0} missing file(s)", _originalFolder.Sums.Count-_backupFolder.Sums.Count);
                var message = MissingFile();
                return $"Failure: {_originalFolder.Sums.Count - _backupFolder.Sums.Count} missing file(s) \n {message}";
            }

            var failures = "";
            var sumsCount = _backupFolder.Sums.Count;
            for (var i = 0; i < sumsCount; i++)
            {
                Console.Clear();
                var completion = (int) ((i + 1) /(float) sumsCount * 100f);
                Console.WriteLine("Generating report...");
                Console.WriteLine($"Completion: {completion}%");
                
                if ( _backupFolder.Sums[i].Sum != _originalFolder.Sums[i].Sum)
                {
                    failures += "Sum: \n" +
                                "New: " + _backupFolder.Sums[i].Path +" "+_backupFolder.Sums[i].Sum+
                                " \nOriginal : " + _originalFolder.Sums[i].Path+" "+_originalFolder.Sums[i].Sum+"\n";
                    _errors++;
                }
                
                if (_backupFolder.Sums[i].Path != _originalFolder.Sums[i].Path)
                {
                    failures += "Path: \n" +
                                "New: " + _backupFolder.Sums[i].Path +" "+_backupFolder.Sums[i].Sum+
                                " \nOriginal : " + _originalFolder.Sums[i].Path+" "+_originalFolder.Sums[i].Sum+"\n";
                    _errors++;
                }
            }

            if (failures != string.Empty)
            {
                Console.WriteLine("Check succeeded, {0} error(s)", _errors);
                Console.WriteLine(failures);
                return failures;
            }
            Console.WriteLine("Check succeeded, 0 error found.");
            return failures;
        }

        private string MissingFile()
        {
            var message = "";
            for (var i = 0; i < _originalFolder.Sums.Count; i++)
            {
                try
                {
                    if (_backupFolder.Sums[i].Path != _originalFolder.Sums[i].Path)
                    {
                        var found = false;
                        for (var j = 0; j < _backupFolder.Sums.Count; j++)
                        {
                            if (_originalFolder.Sums[i].Path == _backupFolder.Sums[j].Path)
                            {
                                found = true;
                            }
                        }

                        if (found)
                            continue;
                        _errors++;
                        Console.WriteLine("Missing file : {0}", _originalFolder.Sums[i].Path);
                        message += $"Missing file: {_originalFolder.Sums[i].Path} \n";
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return message;
        }
    }
}