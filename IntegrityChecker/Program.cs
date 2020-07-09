using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using IntegrityChecker.Checkers;
using IntegrityChecker.Client;
using IntegrityChecker.DataTypes;
using IntegrityChecker.Scheduler;
using IntegrityChecker.Server;

namespace IntegrityChecker
{
    static class Program
    {
        public const Logger.Type type = Logger.Type.Ok;
        public const string version = "V0.7b";
        static void Main(string[] args)
        {
            Interface();
        }

        public static void Interface()
        {
            const string welcome = @"=====================================================
        Welcome to integrity checker V0.6b

Menu: 
1 to export a folder SHA1 
2 to get the SHA1 value of a file
3 to check already exported folders
4 (network) Begin a remote check (server)
5 (network) Connect to a remote check session (only if server is already online)
6 (network) Begin a remote check for a folder with the list of its files only (no hash, faster)
7 (network) Connect to a remote fileList check (faster)
10 Exit the program

        Useful information: the port used to do the remote check is 11000, make sure that it is opened on your desktop or router";
            Console.WriteLine(welcome);
            Logger.Instance.Log(Logger.Type.Ok, "Program initialized!");
            try
            {
                // Settings to support all filenames
                Console.OutputEncoding = Encoding.Unicode;
                Console.InputEncoding = Encoding.Unicode;
                Console.Title = "Integrity Checker " + version;
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
                        new Interface(Tasks.Task.Original);
                        Interface();
                        break;
                    case 5:
                        new Interface(Tasks.Task.Backup);
                        Interface();
                        break;
                    case 6:
                        new Interface(Tasks.Task.CompareFileList);
                        Interface();
                        break;
                    case 7:
                        new Interface(Tasks.Task.FileList);
                        Interface();
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
                throw new Exception("There was an error: \n"+e.Message+"\n Exiting... "+e.StackTrace);
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