using System;
using System.IO;
using System.Text.Json;
using IntegrityChecker.Backend;
using IntegrityChecker.Checkers;
using IntegrityChecker.DataTypes;
using IntegrityChecker.Scheduler;

namespace IntegrityChecker.Client
{
    // FileList: client side
    // Packet ids: 100-101 (client) | 30-31 (server)
    public partial class ClientTcp
    {
        public void GenerateFileList()
        {
            Logger.Instance.Log(Logger.Type.Ok, "Client - GenerateFileList: Started generation");
            
            var folder = new Folder(_originPath, false, true);
            
            Logger.Instance.Log(Logger.Type.Ok, "Client - GenerateFileList: Finished generation, waiting for server");
            //synchronization zone: waits for both client to send an ok
            while (true)
            {
                NetworkTcp.SendObject(_client, Status.Ok, Packet.Owner.Client, 100);
                var message = NetworkTcp.Receive(_client, Packet.Owner.Client, 30);
                if (JsonSerializer.Deserialize<Status>(message) == Status.Ok)
                    break;
                Logger.Instance.Log(Logger.Type.Warning, "Client - GenerateFileList: Waiting for server...");
            }
            Logger.Instance.Log(Logger.Type.Ok, "Client - GenerateFileList: Server finished task");
            SendFileList(folder);
        }

        private void SendFileList(Folder folder)
        {
            Logger.Instance.Log(Logger.Type.Ok, "Client - SendFileList: sending folder");
            NetworkTcp.SendViaClient(_client, folder.ExportJson(), Packet.Owner.Client, 101);
            ReceiveResults();
        }

        private void ReceiveResults()
        {
            Logger.Instance.Log(Logger.Type.Ok, "Client - ReceiveResults: waiting for results...");
            var results = NetworkTcp.Receive(_client, Packet.Owner.Client, 31);
            
            var result = JsonSerializer.Deserialize<Result>(results);
            if (result.ErrorCount == 0)
            {
                Logger.Instance.Log(Logger.Type.Ok, "Client - ReceiveResults: Server returned with 0 errors, ending task...");
                Console.WriteLine("There was no error");
                return;
            }
            Logger.Instance.Log(Logger.Type.Error, $"Client - ReceiveResults: There was {result.ErrorCount} error(s) \n {result.ErrorMessage} (server)");
            Console.WriteLine($"There was {result.ErrorCount} error(s) \n {result.ErrorMessage}");
            
        }
    }
}