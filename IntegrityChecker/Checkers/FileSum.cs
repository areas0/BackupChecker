using System;
using System.IO;
using System.Security.Cryptography;

namespace IntegrityChecker.Checkers
{
    public class FileSum
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

        public FileSum(string path, string hash = "", bool manual = false)
        {
            
            _path = path;
            _sum = hash;
            if (manual)
                return;
            if (File.Exists(path))
                _sum = CalculateSha256();
            else
            {
                Logger.Instance.Log(Logger.Type.Error, $"FileSum constructor: missing file at {path}");
                throw new ArgumentException("File does not exist");
            }
        }

/*    Unused function
        private string CalculateMd5()
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(_path);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
*/

/*
        private string CalculateSha1()
        {
            using var stream = File.OpenRead(_path);
            using var sha = new SHA1Managed();
            
            var checksum = sha.ComputeHash(stream);
            var sendCheckSum = BitConverter.ToString(checksum)
                .Replace("-", string.Empty);
            return sendCheckSum;
        }
*/

        private string CalculateSha256()
        {
            using var stream = File.OpenRead(_path);
            using var sha = new SHA256Managed();
            var checksum = sha.ComputeHash(stream);
            var sendChecksum = BitConverter.ToString(checksum).Replace("-", string.Empty);
            return sendChecksum;

        }
    }
}