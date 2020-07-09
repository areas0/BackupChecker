using System;
using System.IO;
using IntegrityChecker.Client;
using IntegrityChecker.Server;

namespace IntegrityChecker.Scheduler
{
    public class Interface
    {
        public string Origin;

        public Interface(Tasks.Task task)
        {
            // if (!File.Exists("config.ini"))
            // {
            //     File.Create("config.ini").Close();
            //     Console.WriteLine("Enter origin path");
            //     Origin = Console.ReadLine();
            //     File.WriteAllText("config.ini", Origin);
            // }
            // else
            // {
            //     Origin = File.ReadAllText("config.ini");
            // }

            string originBackup;
            string remoteIp;
            ClientTcp clientTcp;
            switch (task)
            {
                case Tasks.Task.Original:
                    Console.WriteLine("Server mode activated! Enter the path to work on please: ");
                    var origin = Console.ReadLine();
                    if (!Directory.Exists(origin))
                    {
                        //break;
                    }
                    //begin server
                    var s = new ServerTcp(origin);
                    //start backup for both
                    s.StartBackupCommand();
                    break;
                case Tasks.Task.Backup:
                    //begin client
                    Console.WriteLine("Client mode activated! Enter the path to work on please: ");
                    originBackup = Console.ReadLine();
                    if (!Directory.Exists(originBackup))
                    {
                        //break;
                    }
                    Console.WriteLine("Enter an ip to connect to (default is localhost) :");
                    remoteIp = Console.ReadLine();
                    clientTcp = new ClientTcp(originBackup, remoteIp);
                    clientTcp.ReceiveBackupCommand();
                    break;
                case Tasks.Task.CompareFileList:
                    Console.WriteLine("Client mode activated! Enter the path to work on please: ");
                    originBackup = Console.ReadLine();
                    if (!Directory.Exists(originBackup))
                    {
                        //break;
                    }
                    var server = new ServerTcp(originBackup);
                    server.GenerateFileList();
                    break;
                case Tasks.Task.FileList:
                    //begin client
                    Console.WriteLine("Client mode activated! Enter the path to work on please: ");
                    originBackup = Console.ReadLine();
                    if (!Directory.Exists(originBackup))
                    {
                        //break;
                    }
                    Console.WriteLine("Enter an ip to connect to (default is localhost) :");
                    remoteIp = Console.ReadLine();
                    clientTcp = new ClientTcp(originBackup, remoteIp);
                    clientTcp.GenerateFileList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(task), task, null);
            }
        }
    }
}