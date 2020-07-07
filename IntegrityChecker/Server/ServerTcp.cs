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
        private string _origin = @"D:\Anime\[TheFantastics] Violet Evergarden  - VOSTFR (Bluray x264 10bits 1080p FLAC)\[Nemuri] Violet Evergarden ヴァイオレット・エヴァーガーデン (2018-2020) [FLAC]";
        private TcpListener _server;
        private TcpClient _client;
        public ServerTcp(string origin)
        {
            //_origin = origin;
        }

        public void Init()
        {
            _server = null;
            try
            {
                IPAddress ip = IPAddress.Parse("127.0.0.1");
                _server = new TcpListener(ip, Port);

                _server.Start();
                while (true)
                {
                    Console.Write("Waiting for a connection... ");
                    _client = _server.AcceptTcpClient();
                    Console.WriteLine("Connected!");
                    break;
                }
            }
            catch (SocketException e)
            {
                throw;
            }
            finally
            {
                //do
            }
            SendBackupCommand();
        }
        public void SendBackupCommand()
        {
            Tasks task = new Tasks {OriginName = _origin, Current = Tasks.Task.Backup};
            // SEND: Packet of task
            NetworkTcp.SendObject(_client, task, Packet.Owner.Server, 10);
            Console.WriteLine(_client.Connected);
            var data = NetworkTcp.Receive(_client, Packet.Owner.Server, 0);
            Status status = JsonSerializer.Deserialize<Status>(data);
            
            if (status == Status.Error)
                throw new Exception("Backup init failed, exiting...");

            Console.WriteLine("Starting Sha1 generation for the current folder!");
            Console.ReadKey();
            ExecuteBackup();
        }
        public async void ExecuteBackup()
        {
            var backup = Backup(_origin);
            if(!backup.IsCompleted)
                Console.WriteLine("Waiting for backup Sha1 generation to finish...");
            string folder = await backup;
            //NetworkTcp.SendObject(_client, Status.Ok);
            NetworkTcp.SendObject(_client, Status.Ok, Packet.Owner.Server, 20);
            Status currentStatus = JsonSerializer.Deserialize<Status>(NetworkTcp.Receive(_client, Packet.Owner.Server, 1));
            if (currentStatus == Status.Ok)
            {
                ReceiveResult(folder);
            }
            //ReceiveResult(folder);
        }
        public static async Task<string> Backup(string origin)
        {
            string folder = new Folder(origin).ExportJson();
            Console.WriteLine("Sha1 generation finished");
            return folder;
        }
        public void ReceiveResult(string folder)
        {
            var data = NetworkTcp.Receive(_client, Packet.Owner.Server, 2);
            File.WriteAllText("test.json", data);
            
            Folder f = null;
            Loader.LoadJson(data, ref f);
            
            Folder original = null;
            Loader.LoadJson(folder, ref original);
            
            Checker checker = new Checker(_origin, _origin, true){BackupFolder = f, OriginalFolder = original};
            string result = checker.CheckFolders();
            int errors = checker.Errors;
            
            NetworkTcp.SendObject(_client, Status.Ok, Packet.Owner.Server, 11);
            
            while (true)
            {
                string status = NetworkTcp.Receive(_client, Packet.Owner.Server, 3);
                if (JsonSerializer.Deserialize<Status>(status) == Status.Ok)
                    break;
            }
            SendResults(errors, result);

        }

        public void SendResults(int errors, string result)
        {
            Console.WriteLine(JsonSerializer.Serialize(new Result {ErrorCount = errors, ErrorMessage = result}));
            Result resultF = new Result {ErrorCount = errors, ErrorMessage = result};
            NetworkTcp.SendObject(_client, resultF, Packet.Owner.Server, 12);
            Console.ReadKey();
        }
    }
}