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
using IntegrityChecker.Server;

namespace IntegrityChecker.Client
{
    public class ClientTcp
    {
        private TcpClient _client;
        private readonly string _originName;
        private readonly string _originPath;
        private readonly string _ip;
        public ClientTcp(string origin, string ip = "127.0.0.1")
        {
            if(!Directory.Exists(origin))
                throw new ArgumentException("Client initialization failed: folder doesn't exist!");
            DirectoryInfo directoryOrigin = new DirectoryInfo(origin);
            _originPath = origin;
            _originName = directoryOrigin.Name;
            _ip = ip;
            Init();
        }

        private void Init()
        {
            Logger.Instance.Log(Logger.Type.Ok, "Client initialization started");
            try
            {
                var port = ServerTcp.Port;
                _client = new TcpClient(_ip, port);
            }
            catch (Exception e)
            {
                _client?.Close();
                Logger.Instance.Log(Logger.Type.Error, $"Client initialization failed: ip {_ip} \n {e.Message} \n {e.StackTrace}");
                throw new Exception("Client init failed, please check your parameters and network \n"+e);
            }
            Logger.Instance.Log(Logger.Type.Ok, "Client initialization finished");
            ReceiveBackupCommand();
        }

        private void ReceiveBackupCommand()
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
                case Tasks.Task.Compare:
                    throw new InvalidDataException("Client cannot do the comparision!");
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
            Console.ReadKey();
        }
    }
}