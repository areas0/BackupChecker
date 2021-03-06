﻿using IntegrityChecker.Checkers;

namespace IntegrityChecker.DataTypes
{
    //Type used to load files via JSON formatted files because constructor without parameter is required
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ManualFileSum
    {
        private string _path;
        private string _sum;

        public string Path
        {
            get => _path;
            set => _path = value;
        }

        public string Sum
        {
            get => _sum;
            set => _sum = value;
        }

        public FileSum Export()
        {
            return new FileSum(_path, _sum, true);
        }
    }
}