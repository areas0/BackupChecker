using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using IntegrityChecker.Backend;
using IntegrityChecker.DataTypes;
using IntegrityChecker.Loaders;
using IntegrityChecker.Scheduler;

namespace IntegrityChecker.Server
{
    public class ServerTcp
    {
        public const int Port = 11000;
        private string _ip;
        private readonly string _origin;
        private TcpListener _server;
        private TcpClient _client;
        public ServerTcp(string origin, string ip="127.0.0.1")
        {
            _origin = origin;
            _ip = ip;
            Init();
        }
        
        // Setup the server and launches the backup process
        private void Init()
        {
            _server = null;
            try
            {
                // Converts a string into an ip address for the program to launch the Tcp listener
                // WARNING: if it is wrong the whole process will fail!
                // ip = IPAddress.Parse(_ip);
                _server = new TcpListener(IPAddress.Any, Port);

                _server.Start();
                while (true)
                {
                    Console.Write("Waiting for a connection... ");
                    _client = _server.AcceptTcpClient();
                    Console.WriteLine("Connected!");
                    break;
                }
                // Starts backup after the connection
                SendBackupCommand();
            }
            catch (SocketException e)
            {
                _client?.Close();
                throw e;
            }
        }

        private void SendBackupCommand()
        {
            var task = new Tasks {OriginName = _origin, Current = Tasks.Task.Backup};
            
            // SEND: Packet of task (id 10)
            NetworkTcp.SendObject(_client, task, Packet.Owner.Server, 10);
            // RECEIVE: Packet of status (id 0)
            var data = NetworkTcp.Receive(_client, Packet.Owner.Server, 0);
            var status = JsonSerializer.Deserialize<Status>(data);
            
            if (status == Status.Error) // stop the process if there is something on client not working
                throw new Exception("Backup init failed, exiting...");

            Console.WriteLine("Starting Sha1 generation for the current folder after you press a key...");
            Console.ReadKey();
            ExecuteBackup();
        }

        private async void ExecuteBackup()
        {
            var backup = Backup(_origin);
            if(!backup.IsCompleted)
                Console.WriteLine("Waiting for backup Sha1 generation to finish...");
            var folder = await backup;
            //PACKETS priority high: confirm backup finished on both client
            // SEND: Status packet (id20) (synchronize with client)
            NetworkTcp.SendObject(_client, Status.Ok, Packet.Owner.Server, 20);
            // RECEIVE: Status packet (id 1) (verifies that client has finished)
            var currentStatus = JsonSerializer.Deserialize<Status>(NetworkTcp.Receive(_client, Packet.Owner.Server, 1));
            if (currentStatus == Status.Ok)
            {
                ReceiveResult(folder);
            }
            else
                throw new Exception("Backup failed, exiting...");
        }
        //TODO: rework without async because not needed anymore
        private static async Task<string> Backup(string origin)
        {
            var folder = new Folder(origin).ExportJson();
            Console.WriteLine("Sha1 generation finished");
            return folder;
        }

        private void ReceiveResult(string folder)
        {
            // RECEIVE: Folder packet (id2) (will allow to rebuild the folder via Loader.LoadJson
            var data = NetworkTcp.Receive(_client, Packet.Owner.Server, 2);
            File.WriteAllText("test.json", data);
            // Rebuilding both folders from json output
            Folder f = null;
            Loader.LoadJson(data, ref f);
            
            Folder original = null;
            Loader.LoadJson(folder, ref original);
            // sending the folders to a checker which will compute missing files &/ errors
            var checker = new Checker(_origin, _origin, true){BackupFolder = f, OriginalFolder = original};
            var result = checker.CheckFolders();
            var errors = checker.Errors;
            // SEND: Status Ok packet to warn client that the process is done (id11)
            NetworkTcp.SendObject(_client, Status.Ok, Packet.Owner.Server, 11);
            
            // Waiting for client to be ready
            while (true)
            {
                var status = NetworkTcp.Receive(_client, Packet.Owner.Server, 3);
                if (JsonSerializer.Deserialize<Status>(status) == Status.Ok)
                    break;
            }
            SendResults(errors, result);

        }

        private void SendResults(int errors, string result)
        {
            // errors and result are sent via json using a parameterless constructor class (see documentation on JsonSerializer.Deserialize)
            var resultF = new Result {ErrorCount = errors, ErrorMessage = result};
            NetworkTcp.SendObject(_client, resultF, Packet.Owner.Server, 12);
            
            //POST PROCESS | Make sure that client receives the packet
            Console.ReadKey();
            // Disconnect all clients
            NetworkTcp.Disconnect(_client);
            _client.Close();
            Program.Interface();
        }
    }
}