using System;
using System.Collections.Generic;
using IntegrityChecker.Loaders;

namespace IntegrityChecker.DataTypes
{
    //Type used to load files via JSON formatted files because constructor without parameter is required
    public abstract class ManualFolder
    {
        private List<ManualFileSum> _sums;
        private string _path;
        private List<string> _files = new List<string>();

        public List<ManualFileSum> Sums
        {
            get => _sums;
            set => _sums = value;
        }

        public string Path
        {
            get => _path;
            set => _path = value;
        }

        public List<string> Files
        {
            get => _files;
            set => _files = value;
        }

        public Folder Export()
        {
            var exported = new List<FileSum>();
            foreach (var fileSum in _sums)
            {
                exported.Add(fileSum.Export());
            }
            var folder = new Folder(_path, true) {Sums = exported, Files = _files};
            return folder;
        }
    }
}