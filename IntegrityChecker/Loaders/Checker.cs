using System;
using IntegrityChecker.DataTypes;

namespace IntegrityChecker.Loaders
{
    public class Checker
    {
        private string _original;
        private Folder _originalFolder = null;
        private string _backup;
        private Folder _backupFolder = null;

        public Checker(string original, string backup)
        {
            _original = original;
            _backup = backup;
            Loader.LoadJson(original, ref _originalFolder);
            Loader.LoadJson(backup, ref _backupFolder);
        }

        public bool CheckFolders()
        {
            int errors = 0;
            if (_backupFolder.Sums.Count != _originalFolder.Sums.Count)
            {
                Console.Error.WriteLine("Failure: {0} missing file(s)", _originalFolder.Sums.Count-_backupFolder.Sums.Count);
                MissingFile();
                return false;
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
                    errors++;
                }
                if (_backupFolder.Sums[i].Path != _originalFolder.Sums[i].Path)
                {
                    failures += "Path: \n" +
                                "New: " + _backupFolder.Sums[i].Path +" "+_backupFolder.Sums[i].Sum+
                                " \nOriginal : " + _originalFolder.Sums[i].Path+" "+_originalFolder.Sums[i].Sum+"\n";
                    errors++;
                }
            }

            if (failures != string.Empty)
            {
                Console.WriteLine("Check succeeded, {0} error(s)", errors);
                Console.WriteLine(failures);
                return false;
            }
            Console.WriteLine("Check succeeded, 0 error found.");
            return true;
        }

        private void MissingFile()
        {
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
                        Console.WriteLine("Missing file : {0}", _originalFolder.Sums[i].Path);
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }
    }
}