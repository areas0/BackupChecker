using System;
using System.IO;
using System.Text.Json;
using IntegrityChecker.Backend;
using IntegrityChecker.Checkers;
using IntegrityChecker.DataTypes;
using IntegrityChecker.Scheduler;

namespace IntegrityChecker.Client
{
    public partial class ClientTcp
    {
        public void ReceiveBackupCommand()
        {
            var message = NetworkTcp.Receive(_client, Packet.Owner.Client, 10);
            var task = JsonSerializer.Deserialize<Tasks>(message);
            
            if (task.OriginName == _originName)
                NetworkTcp.SendObject(_client, Status.Ok, Packet.Owner.Client, 0);
            else
            {
                NetworkTcp.SendObject(_client, Status.Error, Packet.Owner.Client, 0);
                _client.Close();
                Logger.Instance.Log(Logger.Type.Error, $"ReceiveBackup command failed: origin differs \n Original: {task.OriginName} \n Backup: {_originName}");
                throw new Exception("Backup init failed, exiting...");
            }

            switch (task.Current)
            {
                case Tasks.Task.Original:
                    throw new InvalidDataException("Client must be the backup folder!");
                case Tasks.Task.Backup:
                    Console.WriteLine("Starting backup now!");
                    Console.ReadKey();
                    Logger.Instance.Log(Logger.Type.Ok, $"ReceiveBackup finished, starting backup");
                    Backup();
                    break;
                // this case is a placeholder
                case Tasks.Task.CompareFileList:
                    throw new InvalidDataException("Client cannot do the comparision!");
                case Tasks.Task.FileList:
                    GenerateFileList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Backup()
        {
            var folder = Backup(_originPath);
            //if(!backup.IsCompleted)
            //    Console.WriteLine("Waiting for backup Sha1 generation to finish...");
            //var folder = await backup;
            while (true)
            {
                NetworkTcp.SendObject(_client, Status.Ok, Packet.Owner.Client, 1);
                var data = NetworkTcp.Receive(_client, Packet.Owner.Client, 20);
                if (JsonSerializer.Deserialize<Status>(data) == Status.Ok)
                {
                    break;
                }
            }
            Logger.Instance.Log(Logger.Type.Ok, "Backup: finished on both clients");
            Console.WriteLine("Finished Sha1 generation on both clients...");
            ProceedResults(folder);
            
        }

        private static string Backup(string origin)
        {
            var folder = new Folder(origin).ExportJson();
            Console.WriteLine("Sha1 generation finished");
            Logger.Instance.Log(Logger.Type.Ok, $"Client - Backup: finished folder generation");
            return folder;
        }

        private void ProceedResults(string folder)
        {
            Logger.Instance.Log(Logger.Type.Ok, "Client: Sending folder");
            NetworkTcp.SendViaClient(_client, folder, Packet.Owner.Client, 2);
            GetResults();
        }

        private void GetResults()
        {
            Logger.Instance.Log(Logger.Type.Ok, "Client - GetResults: waiting for results");
            Console.Write("Waiting for results....");
            while (true)
            {
                var status = NetworkTcp.Receive(_client, Packet.Owner.Client, 11);
                if (JsonSerializer.Deserialize<Status>(status) == Status.Ok)
                    break;
            }
            
            NetworkTcp.SendObject(_client, Status.Ok, Packet.Owner.Client, 3);
            var data = NetworkTcp.Receive(_client, Packet.Owner.Client, 12);
            
            Logger.Instance.Log(Logger.Type.Ok, "Client - GetResults: received results");
            
            var resultSecond = data;
            var result = JsonSerializer.Deserialize<Result>(resultSecond);

            var message = $"There was {result.ErrorCount} error(s). \n {result.ErrorMessage}";
            Logger.Instance.Log(Logger.Type.Warning, message);
            Console.Write(message);
            NetworkTcp.Disconnect(_client);
            _client.Close();
            Logger.Instance.CheckOut();
        }
    }
}