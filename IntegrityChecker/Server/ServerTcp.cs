using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using IntegrityChecker.Backend;
using IntegrityChecker.Checkers;
using IntegrityChecker.DataTypes;
using IntegrityChecker.Scheduler;

namespace IntegrityChecker.Server
{
    public class ServerTcp
    {
        public const int Port = 11000;
        private readonly string _originName;
        private readonly string _originPath;
        private TcpListener _server;
        private TcpClient _client;
        public ServerTcp(string origin, string ip="127.0.0.1")
        {
            Logger.Instance.Log(Logger.Type.Ok, "Server - Constructor: starting the server");
            if (!Directory.Exists(origin))
            {
                Logger.Instance.Log(Logger.Type.Error, $"Server - Constructor: Directory doesn't exist, path: {origin}");
                throw new ArgumentException("Server initialization failed: folder doesn't exist!");
            }
            DirectoryInfo directoryOrigin = new DirectoryInfo(origin);
            _originPath = origin;
            _originName = directoryOrigin.Name;
            Init();
        }
        
        // Setup the server and launches the backup process
        private void Init()
        {
            Logger.Instance.Log(Logger.Type.Ok, "Server - Init: Started initialization of server, waiting for clients");
            _server = null;
            try
            {
                // Converts a string into an ip address for the program to launch the Tcp listener
                // WARNING: if it is wrong the whole process will fail!
                _server = new TcpListener(IPAddress.Any, Port);

                _server.Start();
                while (true)
                {
                    Console.Write("Waiting for a connection... ");
                    _client = _server.AcceptTcpClient();
                    Console.WriteLine("Connected!");
                    break;
                }
                Logger.Instance.Log(Logger.Type.Ok,$"Server - Init: Connected!");
                // Starts backup after the connection
                SendBackupCommand();
            }
            catch (SocketException)
            {
                Logger.Instance.Log(Logger.Type.Error, "Server - Init: failed to initialize the server, check your network settings");
                _client?.Close();
                throw;
            }
        }

        private void SendBackupCommand()
        {
            Logger.Instance.Log(Logger.Type.Ok, "Server - SendBackup: starting to send tasks");
            var task = new Tasks {OriginName = _originName, Current = Tasks.Task.Backup};
            
            // SEND: Packet of task (id 10)
            NetworkTcp.SendObject(_client, task, Packet.Owner.Server, 10);
            // RECEIVE: Packet of status (id 0)
            var data = NetworkTcp.Receive(_client, Packet.Owner.Server, 0);
            var status = JsonSerializer.Deserialize<Status>(data);

            if (status == Status.Error) // stop the process if there is something on client not working
            {
                Logger.Instance.Log(Logger.Type.Error, "Server - SendBackup: there was an error on the other client");
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
            NetworkTcp.SendObject(_client, Status.Ok, Packet.Owner.Server, 20);
            // RECEIVE: Status packet (id 1) (verifies that client has finished)
            var currentStatus = JsonSerializer.Deserialize<Status>(NetworkTcp.Receive(_client, Packet.Owner.Server, 1));
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
            var data = NetworkTcp.Receive(_client, Packet.Owner.Server, 2);
            Logger.Instance.Log(Logger.Type.Ok, "Server - ReceiveResults: results received");
            try
            {
                File.WriteAllText("test.json", data);
            }
            catch (Exception)
            {
                Logger.Instance.Log(Logger.Type.Error, "Server - ReceiveResults: couldn't write to test.json received folder");
                //ignore
            }
            // Rebuilding both folders from json output
            Folder f = null;
            Loader.LoadJson(data, ref f);
            
            Folder original = null;
            Loader.LoadJson(folder, ref original);

            Logger.Instance.Log(Logger.Type.Ok, "Server - ReceiveResults: loaded successfully, now starting checker");
            // sending the folders to a checker which will compute missing files &/ errors
            var checker = new Checker(_originPath, _originPath, true){BackupFolder = f, OriginalFolder = original};
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
            Logger.Instance.Log(Logger.Type.Ok, "Server - SendResults: sending results");
            // errors and result are sent via json using a parameterless constructor class (see documentation on JsonSerializer.Deserialize)
            var resultF = new Result {ErrorCount = errors, ErrorMessage = result};
            NetworkTcp.SendObject(_client, resultF, Packet.Owner.Server, 12);
            
            //POST PROCESS | Make sure that client receives the packet
            Console.ReadKey();
            Logger.Instance.Log(Logger.Type.Ok, "Server - SendResults: finished job");
            // Disconnect all clients
            NetworkTcp.Disconnect(_client);
            _client.Close();
            Program.Interface();
        }
    }
}