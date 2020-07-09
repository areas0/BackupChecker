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
    public partial class ClientTcp
    {
        private TcpClient _client;
        private readonly string _originName;
        private readonly string _originPath;
        private readonly string _ip;
        public ClientTcp(string origin, string ip = "127.0.0.1")
        {
            if(!Directory.Exists(origin))
                throw new ArgumentException("Client initialization failed: folder doesn't exist!");
            var directoryOrigin = new DirectoryInfo(origin);
            _originPath = origin;
            _originName = directoryOrigin.Name;
            _ip = ip;
            Init();
        }
        //Initialize a client by trying to connect to the specified _ip
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
                throw new ArgumentException("Client init failed, please check your parameters and network \n"+e);
            }
            Logger.Instance.Log(Logger.Type.Ok, "Client initialization finished");
            //ReceiveBackupCommand();
        }
    }
}