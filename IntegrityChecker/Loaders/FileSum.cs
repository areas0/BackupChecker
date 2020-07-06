using System;
using System.IO;
using System.Security.Cryptography;

namespace IntegrityChecker.Loaders
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
            
            this._path = path;
            this._sum = hash;
            if (manual)
                return;
            if (File.Exists(path))
                _sum = CalculateSHA1();
            else
                throw new ArgumentException("File does not exist");
        }

        private string CalculateMD5()
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(_path);
            var hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private string CalculateSHA1()
        {
            using (FileStream stream = File.OpenRead(_path))
            {
                using (SHA1Managed sha = new SHA1Managed())
                {
                    byte[] checksum = sha.ComputeHash(stream);
                    string sendCheckSum = BitConverter.ToString(checksum)
                        .Replace("-", string.Empty);
                    return sendCheckSum;
                }
            }
        }
    }
}