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
    public partial class ServerTcp
    {
        // Change it to fit your needs
        public const int Port = 11000;
        
        // General information about the folder the program is working on
        private readonly string _originName;
        private readonly string _originPath;
        
        // Server classes objects
        private TcpListener _server;
        private TcpClient _client;
        public ServerTcp(string origin)
        {
            Logger.Instance.Log(Logger.Type.Ok, "Server - Constructor: starting the server");
            if (!Directory.Exists(origin))
            {
                Logger.Instance.Log(Logger.Type.Error, $"Server - Constructor: Directory doesn't exist, path: {origin}");
                throw new ArgumentException("Server initialization failed: folder doesn't exist!");
            }
            var directoryOrigin = new DirectoryInfo(origin);
            _originPath = origin;
            _originName = directoryOrigin.Name;
            Init();
        }

        private static string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ips = String.Empty;
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ips += ip.ToString() +", ";
                }
            }

            return ips;
        }
        
        // Setup the server and launches the backup process
        private void Init()
        {
            var ip = GetLocalIpAddress();
            Logger.Instance.Log(Logger.Type.Ok, $"Server - Init: Started initialization of server, waiting for clients ip: {ip}");
            _server = null;
            try
            {
                // Converts a string into an ip address for the program to launch the Tcp listener
                // WARNING: if it is wrong the whole process will fail!
                _server = new TcpListener(IPAddress.Any, Port);

                _server.Start();
                while (true)
                {
                    Console.WriteLine($"IP(s): {ip}");
                    Console.Write("Waiting for a connection... ");
                    _client = _server.AcceptTcpClient();
                    Console.WriteLine("Connected!");
                    break;
                }
                Logger.Instance.Log(Logger.Type.Ok,$"Server - Init: Connected!");
                // Starts backup after the connection
                //StartBackupCommand();
            }
            catch (SocketException)
            {
                Logger.Instance.Log(Logger.Type.Error, "Server - Init: failed to initialize the server, check your network settings");
                _client?.Close();
                throw;
            }
        }
    }
}