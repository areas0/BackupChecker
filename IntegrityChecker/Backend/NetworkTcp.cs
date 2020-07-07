using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IntegrityChecker.DataTypes;

namespace IntegrityChecker.Backend
{
    public static class NetworkTcp
    {
        public static string Receive(TcpClient client, Packet.Owner owner, int expectedId)
        {
            // maximum size of message, large size is chosen for the moment
            var bytes = new byte[1048576];
            while (true)
            {
                // Get a stream object for reading and writing
                var stream = client.GetStream();

                var i = 0;

                // Loop to receive all the data sent by the client.
                //(i = stream.Read(bytes, 0, bytes.Length)) != 0
                while (true)
                {
                    // Translate data bytes to a ASCII string.
                    i = stream.Read(bytes, 0, bytes.Length);
                    var data = Encoding.UTF8.GetString(bytes, 0, i);
                    var packet = FindPacket(data, expectedId);
                    if(packet is null)
                        continue;
                    
                    if (packet.OwnerT != owner)
                        return packet.Message;
                }
            }
        }

        private static Packet FindPacket(string message, int id)
        {
            var messages = new List<string>();
            var data = "";
            foreach (var t in message)
            {
                if (t != '\0')
                    data += t;
                else
                {
                    messages.Add(data);
                    data = "";
                }
            }

            Packet last = null;
            foreach (var t in messages)
            {
                var packet = JsonSerializer.Deserialize<Packet>(t);
                if (packet.Id == id) 
                    last = packet;
            }
            //Console.WriteLine($"Last: {last?.Id}");
            return last;
        }
        

        public static void SendViaClient(TcpClient client, string message, Packet.Owner owner, int id)
        {
            var packet = new Packet(){Id = id, Message = message, OwnerT = owner};
            var data =Encoding.UTF8.GetBytes( JsonSerializer.Serialize(packet)+"\0");

            // Get a client stream for reading and writing.
            //  Stream stream = client.GetStream();

            var stream = client.GetStream();

            // Send the message to the connected TcpServer.
            stream.Write(data, 0, data.Length);
            //Console.WriteLine("Sent: {0}", JsonSerializer.Serialize(packet));
        }

        public static void SendObject(TcpClient client, object obj, Packet.Owner owner, int id)
        {
            SendViaClient(client, JsonSerializer.Serialize(obj), owner, id);
        }

/*
        public static async Task<string> ReceiveAsync(TcpClient client)
        {
            string data = String.Empty;
            byte[] bytes = new byte[4096];
            Stream stream = client.GetStream();
            int i= 0;
            Task<int> b = stream.ReadAsync(bytes, 0, i);
            await b;
            //Console.WriteLine(Encoding.UTF8.GetString(bytes));
            return Encoding.UTF8.GetString(bytes);
        }
*/

        public static void Disconnect(TcpClient client)
        {
            client.GetStream().Close();
        }
    }
}