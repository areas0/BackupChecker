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
            Original = original;
            _backup = backup;
            if (manual)
                return;
            string jsonString = File.ReadAllText(original);
            string jsonString2 = File.ReadAllText(_backup);
            Loader.LoadJson(jsonString, ref _originalFolder);
            Loader.LoadJson(jsonString2, ref _backupFolder);
        }

        public string CheckFolders()
        {
            if (_backupFolder.Sums.Count != _originalFolder.Sums.Count)
            {
                Console.Error.WriteLine("Failure: {0} missing file(s)", _originalFolder.Sums.Count-_backupFolder.Sums.Count);
                string message = MissingFile();
                return $"Failure: {_originalFolder.Sums.Count - _backupFolder.Sums.Count} missing file(s) \n {message}";
            }

            string failures = "";
            var sumsCount = _backupFolder.Sums.Count;
            for (int i = 0; i < sumsCount; i++)
            {
                Console.WriteLine((i+1)+"/"+sumsCount);
                //_checkedFolder.Sums[i].Path != _originalFolder.Sums[i].Path ||
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
            string message = "";
            for (int i = 0; i < _originalFolder.Sums.Count; i++)
            {
                try
                {
                    if (_backupFolder.Sums[i].Path != _originalFolder.Sums[i].Path)
                    {
                        bool found = false;
                        for (int j = 0; j < _backupFolder.Sums.Count; j++)
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
                    continue;
                }
            }

            return message;
        }
    }
}