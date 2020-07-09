using System;
using System.Text.Json;
using IntegrityChecker.Backend;
using IntegrityChecker.Checkers;
using IntegrityChecker.DataTypes;
using IntegrityChecker.Scheduler;

namespace IntegrityChecker.Server
{
    // Partial class: only functions to check a folder with its path, no hash in it
    // Packet Ids: 30-31 (server) | 100-101 (client)
    public partial class ServerTcp
    {
        public void GenerateFileList()
        {
            Logger.Instance.Log(Logger.Type.Ok, "Server - GenerateFileList: Started fileList generation");
            
            var folder = new Folder(_originPath, false, true);
            
            Logger.Instance.Log(Logger.Type.Ok, "Server - GenerateFileList: Finished fileList generation, waiting for client ot finish");
            // Client synchronization on both sides
            while (true)
            {
                NetworkTcp.SendObject(_client, Status.Ok, Packet.Owner.Server, 30);
                var message = NetworkTcp.Receive(_client, Packet.Owner.Server, 100);
                if (JsonSerializer.Deserialize<Status>(message) == Status.Ok)
                    break;
                if(JsonSerializer.Deserialize<Status>(message) == Status.Error)
                    throw new Exception("There was an error");
            }
            Logger.Instance.Log(Logger.Type.Ok, "Server - GenerateFileList: Finished on both clients");
            ReceiveFileList(folder);
        }

        private void ReceiveFileList(Folder currentFolder)
        {
            Logger.Instance.Log(Logger.Type.Ok, "Server - ReceiveFileList: Started receive the list");
            
            var backupFolder = NetworkTcp.Receive(_client, Packet.Owner.Server, 101);
            
            Logger.Instance.Log(Logger.Type.Ok, "Server - ReceiveFileList: Received the list");
            
            Folder backup = null;
            try
            {
                Loader.LoadJson(backupFolder, ref backup);
            }
            catch (Exception e)
            {
                Logger.Instance.Log(Logger.Type.Error, "Server - ReceiveFileList: received incorrect data \n"+ e.Message+ "\n"+ e.StackTrace);
                throw;
            }
            // Creating a manual checker to read the folder list
            var checker = new Checker(_originPath, _originPath, true)
            {
                OriginalFolder = currentFolder, BackupFolder = backup
            };
            checker.Message = checker.MissingFile();
            Logger.Instance.Log(Logger.Type.Ok, "Server - ReceiveFileList: Successfully checked the folder, sending results...");
            SendResults(checker);
        }

        private void SendResults(Checker checker)
        {

            var result = new Result()
            {
                ErrorCount = checker.Errors,
                ErrorMessage = checker.Message
            };
            Logger.Instance.Log(Logger.Type.Ok, "Server - SendResults: sending results...");
            NetworkTcp.SendObject(_client, result, Packet.Owner.Server, 31);
            DisplayResults(checker);
        }

        private static void DisplayResults(Checker checker)
        {
            Logger.Instance.Log(Logger.Type.Ok, "Server - DisplayResults: displaying");
            if (checker.Errors == 0)
            {
                Console.WriteLine("There was no error. Your folder is complete.");
                return;
            }
            Console.WriteLine($"There was {checker.Errors} error(s) \n {checker.Message}");
        }
    }
}