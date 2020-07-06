using System;
using System.IO;
using IntegrityChecker.DataTypes;
using IntegrityChecker.Loaders;

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
                    Interface();
        }

        public static void Interface()
        {
            const string welcome = @"=====================================================
        Welcome to integrity checker V0.1

        Menu: 1 to export a folder SHA1 
        2 to get the SHA1 value of a file";
            Console.WriteLine(welcome);
            int option = Convert.ToInt32(Console.ReadLine());
            switch (option)
            {
                case 1:
                    Console.Write("Option 1 selected \n"+"Enter the path to export: ");
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
                    Server.Server s = new Server.Server();
                    break;
                case 11:
                    new Client.Client().Init();
                    break;
                default:
                    Console.Clear();
                    Console.WriteLine("Invalid input");
                    Interface();
                    return;
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
    }
}