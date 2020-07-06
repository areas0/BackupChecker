using System;
using System.IO;

namespace IntegrityChecker.Scheduler
{
    public class Interface
    {
        public string Origin;

        public Interface(Tasks.Task task)
        {
            if (!File.Exists("config.ini"))
            {
                File.Create("config.ini").Close();
                Console.WriteLine("Enter origin path");
                Origin = Console.ReadLine();
                File.WriteAllText("config.ini", Origin);
            }
            else
            {
                Origin = File.ReadAllText("config.ini");
            }

            switch (task)
            {
                case Tasks.Task.Original:
                    //begin server
                    //start backup for both
                    break;
                case Tasks.Task.Backup:
                    //begin client
                    break;
                case Tasks.Task.Compare:
                    //Send result
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(task), task, null);
            }
        }
    }
}