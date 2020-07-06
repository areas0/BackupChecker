using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IntegrityChecker.DataTypes;
using IntegrityChecker.Loaders;
using IntegrityChecker.Scheduler;

namespace IntegrityChecker.Server
{
    public class Server
    {
        private Socket _listener;
        private Socket _handler;
        public string origin = @"D:\Anime\[TheFantastics] Violet Evergarden  - VOSTFR (Bluray x264 10bits 1080p FLAC)\[Nemuri] Violet Evergarden ヴァイオレット・エヴァーガーデン (2018-2020) [FLAC]";
        public Server()
        {
            Init();
        }
        private void Init()
        {
            IPHostEntry host = Dns.GetHostEntry("localhost");  
            IPAddress ipAddress = host.AddressList[0];  
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);    
        
  
            try {   
  
                // Create a Socket that will use Tcp protocol      
                _listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);  
                // A Socket must be associated with an endpoint using the Bind method  
                _listener.Bind(localEndPoint);  
                // Specify how many requests a Socket can listen before it gives Server busy response.  
                // We will listen 10 requests at a time  
                _listener.Listen(10);  
  
                Console.WriteLine("Waiting for a connection...");  
                _handler = _listener.Accept();  
  
                // Incoming data from the client.    
                /*string data = null;  
                byte[] bytes = null;  
  
                while (true)  
                {  
                    bytes = new byte[10000];  
                    int bytesRec = _handler.Receive(bytes);  
                    data += Encoding.UTF8.GetString(bytes, 0, bytesRec);  
                    if (data.IndexOf("<EOF>", StringComparison.Ordinal) > -1)  
                    {  
                        break;  
                    }  
                }
                File.Create("file.json").Close();
                File.WriteAllText("file.json", data.Substring(0, data.Length-5));
  
                Console.WriteLine("Text received : {0}", data);  
  
                byte[] msg = Encoding.UTF8.GetBytes(data);  
                _handler.Send(msg);  
                _handler.Shutdown(SocketShutdown.Both);  
                _handler.Close();  */
            }  
            catch (Exception e)  
            {  
                Console.WriteLine(e.ToString());  
            }  
  
            Console.WriteLine("\n Press any key to continue...");  
            Console.ReadKey();
            SendBackupCommand();
        }

        public void SendBackupCommand()
        {
            Tasks task = new Tasks(){OriginName = origin, Current = Tasks.Task.Backup};
            Backend.Network.SerializedSend(_handler, task);
            var data = Backend.Network.Receive(_handler);
            Status status = JsonSerializer.Deserialize<Status>(data);
            if (status == Status.Error)
            {
                throw new Exception("Backup init failed, exiting...");
            }
            
            Console.WriteLine("Starting Sha1 generation for the current folder!");
            ExecuteBackup();
        }

        public async void ExecuteBackup()
        {
            var backup = Backup(origin);
            bool finished = false;
            while (!finished || !backup.IsCompleted)
            {
                Console.WriteLine("In loop");
                if (!backup.IsCompleted)
                {
                    Backend.Network.SerializedSend(_handler, Status.Waiting);
                }
                else
                {
                    Backend.Network.SerializedSend(_handler,Status.Ok);
                }
                
                var data = Backend.Network.Receive(_handler);
                Console.WriteLine(JsonSerializer.Deserialize<Status>(data));
                if (JsonSerializer.Deserialize<Status>(data) == Status.Ok)
                {
                    finished = true;
                }

                await Task.Delay(100);
            }
            Console.WriteLine("Outside");
            string folder = await backup;
            
            Backend.Network.SerializedSend(_handler, Status.Ok);
            ReceiveResult(folder);
        }

        public void ReceiveResult(string folder)
        {
            var data = Backend.Network.Receive(_handler);
            File.WriteAllText("test.json", data);
            Folder f = null;
            Loader.LoadJson(data, ref f);
            
            Folder original = null;
            Loader.LoadJson(folder, ref original);
            Checker checker = new Checker(origin, origin, true){BackupFolder = f, OriginalFolder = original};
            string result = checker.CheckFolders();
            int errors = checker.Errors;
            string resultWithErrors = $"There was {errors} error(s) \n {result}";
            SendResults(errors, result+"b");

        }

        public void SendResults(int errors, string result)
        {
            Console.WriteLine((string) JsonSerializer.Serialize(new Result(){ErrorCount = errors, ErrorMessage = result}));
            Result resultF = new Result() {ErrorCount = errors, ErrorMessage = result};
            Backend.Network.SerializedSend(_handler, resultF);
        }

        public static async Task<string> Backup(string origin)
        {
            string folder = new Folder(origin).ExportJson();
            Console.WriteLine("Sha1 generation finished");
            return folder;
        }
    }
}