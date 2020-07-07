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
        private string _origin;
        private string _ip;
        public ClientTcp(string origin, string ip = "127.0.0.1")
        {
            _origin = origin;
            _ip = ip;
            Init();
        }

        public void Init()
        {
            try
            {
                int port = ServerTcp.Port;
                _client = new TcpClient(_ip, port);
            }
            catch (Exception e)
            {
                _client.Close();
                throw;
            }
            ReceiveBackupCommand();
        }
        
        public void ReceiveBackupCommand()
        {
            var message = NetworkTcp.Receive(_client, Packet.Owner.Client, 10);
            Tasks task = JsonSerializer.Deserialize<Tasks>(message);
            
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
                    break;
                case Tasks.Task.Backup:
                    Console.WriteLine("Starting backup now!");
                    Console.ReadKey();
                    Backup();
                    break;
                case Tasks.Task.Compare:
                    throw new InvalidDataException("Client cannot do the comparision!");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public async void Backup()
        {
            var backup = Backup(_origin);
            if(!backup.IsCompleted)
                Console.WriteLine("Waiting for backup Sha1 generation to finish...");
            string folder = await backup;
            while (true)
            {
                NetworkTcp.SendObject(_client, Status.Ok, Packet.Owner.Client, 1);
                string data = NetworkTcp.Receive(_client, Packet.Owner.Client, 20);
                if (JsonSerializer.Deserialize<Status>(data) == Status.Ok)
                {
                    break;
                }
            }

            Console.WriteLine("Finished Sha1 generation on both clients...");
            ProceedResults(folder);
            
        }
        public static async Task<string> Backup(string origin)
        {
            string folder = new Folder(origin).ExportJson();
            Console.WriteLine("Sha1 generation finished");
            return folder;
        }
        public async void ProceedResults(string folder)
        {
            //Console.WriteLine($"Send results {folder}");
            NetworkTcp.SendViaClient(_client, folder, Packet.Owner.Client, 2);
            GetResults();
        }

        public async void GetResults()
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

            string resultb = data;
            Result result = JsonSerializer.Deserialize<Result>(resultb);
            Console.Write($"There was {result.ErrorCount} error(s). \n {result.ErrorMessage}");
            NetworkTcp.Disconnect(_client);
            _client.Close();
            //string[] result = new[] {"0", ""};
            if (true)
            {
                // Console.WriteLine(str);
            }
            else
            {
                Console.Write("Check passed successfully");
            }
        }
    }
}