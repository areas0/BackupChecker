using System;
using System.IO;

namespace IntegrityChecker
{
    public sealed class Logger
    {
        public enum Type
        {
            Ok,
            Warning,
            Error
        }
        private string _path;
        private string _data;
        private Type _debugLevel;
        private bool _initFailed;
        private static Logger instance = null;
        private static readonly object padlock = new object();
        public static Logger Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Logger(Directory.GetCurrentDirectory(), Program.type);
                    }
                    return instance;
                }
            }
        }
        public Logger(string path, Type debugLevel = Type.Ok)
        {
            _path = path+"\\last.log";
            try
            {
                if (!File.Exists(_path))
                {
                    File.Create(_path).Close();
                }
                else
                {
                    File.Move(_path, path + "\\old.log");
                }
            }
            catch (Exception)
            {
                _initFailed = true;
            }

            _debugLevel = debugLevel;
        }

        public void Log(Type type, string message, Exception exception = null)
        {
            var hm = DateTime.Now;
            string finalData = $"<{hm.ToString()}>";
            if (type < _debugLevel)
                return;
            switch (type)
            {
                case Type.Ok:
                    finalData += " Pass : " + message;
                    break;
                case Type.Warning:
                    finalData += " Warning: " + message;
                    break;
                case Type.Error:
                    finalData += " Error: " + message;
                    if (!(exception is null))
                    {
                        finalData += "\n " + exception;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            _data += finalData + "\n";

        }

        public void CheckOut()
        {
            if (!_initFailed)
            {
                File.WriteAllText(_path,_data);
                return;
            }
            Console.WriteLine("Log write failed, here you can find the log \n"+ _data);
        }
    }
}