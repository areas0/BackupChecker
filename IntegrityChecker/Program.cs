using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using IntegrityChecker.Checkers;
using IntegrityChecker.Client;
using IntegrityChecker.DataTypes;
using IntegrityChecker.Server;

namespace IntegrityChecker
{
    static class Program
    {
        public const Logger.Type type = Logger.Type.Ok;
        static void Main(string[] args)
        {
            Interface();
        }

        public static void Interface()
        {
            const string welcome = @"=====================================================
        Welcome to integrity checker V0.6b

        Menu: 1 to export a folder SHA1 
        2 to get the SHA1 value of a file
        3 to check already exported folders
        4 (network) Begin a remote check (server)
        5 (network) Connect to a remote check session (only if server is already online)
        10 Exit the program

        Useful information: the port used to do the remote check is 11000, make sure that it is opened on your desktop or router";
            Console.WriteLine(welcome);
            Logger.Instance.Log(Logger.Type.Ok, "Program initialized!");
            try
            {
                // Settings to support all filenames
                Console.OutputEncoding = Encoding.Unicode;
                Console.InputEncoding = Encoding.Unicode;
                
                var option = Convert.ToInt32(Console.ReadLine());
                switch (option)
                {
                    case 1:
                        Console.Write("Option 1 selected \n" + "Enter the path to export: ");
                        var path = Console.ReadLine();
                        new Folder(path).ExportJson();
                        break;
                    case 2:
                        Console.Write("Option 2 selected, please enter file's path: ");
                        var file = Console.ReadLine();
                        Console.WriteLine(new FileSum(file).Sum);
                        break;
                    case 3:
                        Console.Write("Option 3, enter the original exported file's path: ");
                        var path1 = Console.ReadLine();
                        Console.Write("Enter the new exported file: ");
                        var path2 = Console.ReadLine();
                        new Checker(path1, path2).CheckFolders();
                        break;
                    case 4:
                        Console.WriteLine("Server mode activated! Enter the path to work on please: ");
                        var origin = Console.ReadLine();
                        if (!Directory.Exists(origin))
                        {
                            //break;
                        }
                        //Console.WriteLine("Enter an ip to connect to (default is localhost) :");
                        //var ip = Console.ReadLine();
                        var s = new ServerTcp(origin);
                        break;
                    case 5:
                        Console.WriteLine("Client mode activated! Enter the path to work on please: ");
                        var originBackup = Console.ReadLine();
                        if (!Directory.Exists(originBackup))
                        {
                            //break;
                        }
                        Console.WriteLine("Enter an ip to connect to (default is localhost) :");
                        var remoteIp = Console.ReadLine();
                        var clientTcp = new ClientTcp(originBackup, remoteIp);
                        break;
                    case 10:
                        Logger.Instance.CheckOut();
                        Environment.Exit(0);
                        break;
                    default:
                        Console.Clear();
                        Console.WriteLine("Invalid input");
                        Interface();
                        return;
                }
            }
            catch (FormatException)
            {
                Console.Clear();
                Console.WriteLine("Invalid input");
                Interface();
            }
            catch (Exception e)
            {
                Logger.Instance.CheckOut();
                throw new Exception("There was an error: \n"+e.Message+"\n Exiting...");
                //throw;
            }

            Console.ReadKey();
        }

        public static void Checker(string file, string sec)
        {
            var line = 0;
            for (var i = 0; i < file.Length; i++)
            {
                line++;
                while (file[i] != '\n')
                {
                    if (file[i] != sec[i])
                    {
                        Console.WriteLine("Fault at line {0} {1}", line, i);
                    }
                    i++;
                }
            }
        }
    }
}