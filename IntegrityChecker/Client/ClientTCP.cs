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
using IntegrityChecker.Server;

namespace IntegrityChecker.Client
{
    public class ClientTcp
    {
        private TcpClient _client;
        private readonly string _origin;
        private readonly string _ip;
        public ClientTcp(string origin, string ip = "127.0.0.1")
        {
            _origin = origin;
            _ip = ip;
            Init();
        }

        private void Init()
        {
            try
            {
                var port = ServerTcp.Port;
                _client = new TcpClient(_ip, port);
            }
            catch (Exception)
            {
                _client.Close();
                throw;
            }
            ReceiveBackupCommand();
        }

        private void ReceiveBackupCommand()
        {
            var message = NetworkTcp.Receive(_client, Packet.Owner.Client, 10);
            var task = JsonSerializer.Deserialize<Tasks>(message);
            
            if (task.OriginName == _origin)
                NetworkTcp.SendObject(_client, Status.Ok, Packet.Owner.Client, 0);
            else
            {
                NetworkTcp.SendObject(_client, Status.Error, Packet.Owner.Client, 0);
                //client.Close();
                throw new Exception("Backup init failed, exiting...");
            }

            switch (task.Current)
            {
                case Tasks.Task.Original:
                    throw new InvalidDataException("Client cannot be original folder!");
                case Tasks.Task.Backup:
                    Console.WriteLine("Starting backup now!");
                    Console.ReadKey();
                    Backup();
                    break;
                case Tasks.Task.Compare:
                    throw new InvalidDataException("Client cannot do the comparision!");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async void Backup()
        {
            var backup = Backup(_origin);
            if(!backup.IsCompleted)
                Console.WriteLine("Waiting for backup Sha1 generation to finish...");
            var folder = await backup;
            while (true)
            {
                NetworkTcp.SendObject(_client, Status.Ok, Packet.Owner.Client, 1);
                var data = NetworkTcp.Receive(_client, Packet.Owner.Client, 20);
                if (JsonSerializer.Deserialize<Status>(data) == Status.Ok)
                {
                    break;
                }
            }

            Console.WriteLine("Finished Sha1 generation on both clients...");
            ProceedResults(folder);
            
        }

        private static async Task<string> Backup(string origin)
        {
            var folder = new Folder(origin).ExportJson();
            Console.WriteLine("Sha1 generation finished");
            return folder;
        }

        private void ProceedResults(string folder)
        {
            NetworkTcp.SendViaClient(_client, folder, Packet.Owner.Client, 2);
            GetResults();
        }

        private void GetResults()
        {
            Console.Write("Waiting for results....");
            while (true)
            {
                var status = NetworkTcp.Receive(_client, Packet.Owner.Client, 11);
                if (JsonSerializer.Deserialize<Status>(status) == Status.Ok)
                    break;
            }
            
            NetworkTcp.SendObject(_client, Status.Ok, Packet.Owner.Client, 3);
            var data = NetworkTcp.Receive(_client, Packet.Owner.Client, 12);

            var resultSecond = data;
            var result = JsonSerializer.Deserialize<Result>(resultSecond);
            Console.Write($"There was {result.ErrorCount} error(s). \n {result.ErrorMessage}");
            NetworkTcp.Disconnect(_client);
            _client.Close();
        }
    }
}