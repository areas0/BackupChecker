using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using IntegrityChecker.Client;
using IntegrityChecker.DataTypes;
using IntegrityChecker.Loaders;
using IntegrityChecker.Server;

namespace IntegrityChecker
{
    class Program
    {
        static void Main(string[] args)
        {
            //new Folder(@"D:\Anime\Clannad [BluRay&DVD x264-Hi10P FLAC]").Export();
            //Folder f = Loader.LoadViaFile(@"C:\Users\Shadow\RiderProjects\IntegrityChecker\IntegrityChecker\bin\Debug\netcoreapp3.1\Export.md5");
            //Console.WriteLine(File.ReadAllText(@"C:\Users\Shadow\RiderProjects\IntegrityChecker\IntegrityChecker\bin\Debug\netcoreapp3.1\Export.md5") 
                              //== File.ReadAllText(@"C:\Users\Shadow\RiderProjects\IntegrityChecker\IntegrityChecker\bin\Debug\netcoreapp3.1\backup.md5"));
            //Checker(File.ReadAllText(@"C:\Users\Shadow\RiderProjects\IntegrityChecker\IntegrityChecker\bin\Debug\netcoreapp3.1\Export.md5") 
                    //,File.ReadAllText(@"C:\Users\Shadow\RiderProjects\IntegrityChecker\IntegrityChecker\bin\Debug\netcoreapp3.1\backup.md5"));
                    // int mode = Convert.ToInt32(Console.ReadLine());
                    // switch (mode)
                    // {

                    //     
                    // }
                    //Interface();
                    //Console.WriteLine(JsonSerializer.Serialize(new List<string>() {"0", ""}));
                    //Folder f = null;
         
            //Loader.LoadJson(File.ReadAllText(@"C:\Users\Shadow\RiderProjects\IntegrityChecker\BackupChecker\IntegrityChecker\bin\Debug\netcoreapp3.1\Export - [Nemuri] Violet Evergarden ヴァイオレット・エヴァーガーデン (2018-2020) [FLAC] lundi 6 juillet 2020.json"), ref f);
            string a = Console.ReadLine();
            Console.Clear();
            switch (a)
            {
                case "0":
                    new ServerTcp("").Init();
                    break;
                case "1":
                    new ClientTcp("").Init();
                    break;
            }
        }

        public static void Interface()
        {
            const string welcome = @"=====================================================
        Welcome to integrity checker V0.1

        Menu: 1 to export a folder SHA1 
        2 to get the SHA1 value of a file";
            Console.WriteLine(welcome);
            try
            {
                int option = Convert.ToInt32(Console.ReadLine());
                switch (option)
                {
                    case 1:
                        Console.Write("Option 1 selected \n" + "Enter the path to export: ");
                        string path = Console.ReadLine();
                        new Folder(path).ExportJson();
                        break;
                    case 2:
                        Console.Write("Option 2 selected, please enter file's path: ");
                        string file = Console.ReadLine();
                        Console.WriteLine(new FileSum(file).Sum);
                        break;
                    case 3:
                        Console.Write("Option 3, enter the original exported file's path: ");
                        string path1 = Console.ReadLine();
                        Console.Write("Enter the new exported file: ");
                        string path2 = Console.ReadLine();
                        new Checker(path1, path2).CheckFolders();
                        break;
                    case 10:
                        Console.WriteLine("Server mode activated! Enter the path to work on please: ");
                        string origin = Console.ReadLine();
                        if (!Directory.Exists(origin))
                        {
                            //break;
                        }
                        Server.Server s = new Server.Server(origin);
                        break;
                    case 11:
                        Console.WriteLine("Client mode activated! Enter the path to work on please: ");
                        string originBackup = Console.ReadLine();
                        if (!Directory.Exists(originBackup))
                        {
                            //break;
                        }
                        Client.Client client = new Client.Client(originBackup);
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
                throw new Exception("There was an error: \n"+e.Message);
            }

            Console.ReadKey();
        }

        public static void Checker(string file, string sec)
        {
            int line = 0;
            for (int i = 0; i < file.Length; i++)
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

        public static void Cmanual()
        {
            string path_legacy = @"C:\Users\Shadow\RiderProjects\IntegrityChecker\BackupChecker\IntegrityChecker\bin\Release\netcoreapp3.1\Export - Clannad [BluRay&DVD x264-Hi10P FLAC] mardi 7 juillet 2020.sha1";
            string path_json = @"C:\Users\Shadow\RiderProjects\IntegrityChecker\BackupChecker\IntegrityChecker\bin\Release\netcoreapp3.1\Export - Clannad [BluRay&DVD x264-Hi10P FLAC] mardi 7 juillet 2020.json";
            Folder folder = Loader.LoadViaFile(path_legacy);
            Folder original = null;
            Loader.LoadJson(File.ReadAllText(path_json), ref original);
            Loaders.Checker ch = new Checker("","", true);
            ch.BackupFolder = folder;
            ch.OriginalFolder = original;
            ch.CheckFolders();
        }
    }
}