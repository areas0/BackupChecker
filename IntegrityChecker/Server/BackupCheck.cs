using System;
using System.IO;
using System.Text.Json;
using IntegrityChecker.Backend;
using IntegrityChecker.Checkers;
using IntegrityChecker.DataTypes;
using IntegrityChecker.Scheduler;

namespace IntegrityChecker.Server
{
    // BackupChecker: Server side
    // Packet ids: 0-3 (client) | 10-20 (server)
    public partial class ServerTcp
    {
        public void StartBackupCommand()
        {
            Logger.Instance.Log(Logger.Type.Ok, "Server - SendBackup: starting to send tasks");
            var task = new Tasks {OriginName = _originName, Current = Tasks.Task.Backup};
            
            // SEND: Packet of task (id 10)
            _client.SendObject(task, Packet.Owner.Server, 10);
            
            // RECEIVE: Packet of status (id 0)
            var data = _client.Receive(Packet.Owner.Server, 0);
            var status = JsonSerializer.Deserialize<Status>(data);

            if (status == Status.Error) // stop the process if there is something on client not working
            {
                Logger.Instance.Log(Logger.Type.Error, "Server - SendBackup: " +
                                                       "there was an error on the other client");
                throw new Exception("Backup init failed, exiting...");
            } 

            Console.WriteLine("Starting Sha1 generation for the current folder after you press a key...");
            Console.ReadKey();
            ExecuteBackup();
        }

        private void ExecuteBackup()
        {
            var folder = Backup(_originPath);
            
            //PACKETS priority high: confirm backup finished on both client
            // SEND: Status packet (id20) (synchronize with client)
            _client.SendObject(Status.Ok, Packet.Owner.Server, 20);
            // RECEIVE: Status packet (id 1) (verifies that client has finished)
            var currentStatus = JsonSerializer.Deserialize<Status>(_client.Receive(Packet.Owner.Server, 1));
            if (currentStatus == Status.Ok)
            {
                Logger.Instance.Log(Logger.Type.Ok, "Server - ExecuteBackup: finished on both client correctly");
                ReceiveResults(folder);
            }
            else
            {
                Logger.Instance.Log(Logger.Type.Error, "Server - ExecuteBackup: failed on the other client");
                throw new Exception("Backup failed, exiting...");
            }
        }
        
        private static string Backup(string origin)
        {
            var folder = new Folder(origin).ExportJson();
            Console.WriteLine("Sha1 generation finished");
            Logger.Instance.Log(Logger.Type.Ok,"Server - Backup: finished folder generation");
            return folder;
        }

        private void ReceiveResults(string folder)
        {
            Logger.Instance.Log(Logger.Type.Ok, "Server - ReceiveResults: waiting for results");
            
            // RECEIVE: Folder packet (id2) (will allow to rebuild the folder via Loader.LoadJson
            var data = _client.Receive(Packet.Owner.Server, 2);
            
            Logger.Instance.Log(Logger.Type.Ok, "Server - ReceiveResults: results received");
            try
            {
                File.WriteAllText("test.json", data);
            }
            catch (Exception)
            {
                Logger.Instance.Log(Logger.Type.Error, "Server - ReceiveResults: couldn't write test.json received in the current folder");
                //ignore
            }
            // Rebuilding both folders from json output
            Folder f = null;
            Loader.LoadJson(data, ref f);
            
            Folder original = null;
            Loader.LoadJson(folder, ref original);

            Logger.Instance.Log(Logger.Type.Ok, "Server - ReceiveResults: loaded successfully, now starting checker");
            
            // sending the folders to a checker which will compute missing files &/ errors
            var checker = new Checker(_originPath, _originPath, true)
            {
                BackupFolder = f, OriginalFolder = original
            };
            
            var result = checker.CheckFolders();
            var errors = checker.Errors;
            
            // SEND: Status Ok packet to warn client that the process is done (id11)
            _client.SendObject(Status.Ok, Packet.Owner.Server, 11);
            
            // Waiting for client to be ready
            while (true)
            {
                var status = _client.Receive(Packet.Owner.Server, 3);
                if (JsonSerializer.Deserialize<Status>(status) == Status.Ok)
                    break;
            }
            SendResults(errors, result);

        }

        private void SendResults(int errors, string result)
        {
            Logger.Instance.Log(Logger.Type.Ok, "Server - SendResults: sending results");
            // errors and result are sent via json using a parameterless constructor class (see documentation on JsonSerializer.Deserialize)
            var resultF = new Result {ErrorCount = errors, ErrorMessage = result};
            _client.SendObject(resultF, Packet.Owner.Server, 12);
            
            //POST PROCESS | Make sure that client receives the packet
            Console.ReadKey();
            Logger.Instance.Log(Logger.Type.Ok, "Server - SendResults: finished job");
            // Disconnect all clients
            NetworkTcp.Disconnect(_client);
            _client.Close();
        }
    }
}