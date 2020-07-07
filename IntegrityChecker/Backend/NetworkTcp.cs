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
    public class NetworkTcp
    {
        public enum Type
        {
            Server,
            Listener
        }

        public static string Receive(TcpClient client, Packet.Owner owner, int expectedId)
        {
            string data = String.Empty;
            byte[] bytes = new byte[1048576];
            while (true)
            {

                data = null;

                // Get a stream object for reading and writing
                NetworkStream stream = client.GetStream();

                int i = 0;

                // Loop to receive all the data sent by the client.
                //(i = stream.Read(bytes, 0, bytes.Length)) != 0
                while (true)
                {
                    // Translate data bytes to a ASCII string.
                    //Console.WriteLine($"Expected: {expectedId}");
                    i = stream.Read(bytes, 0, bytes.Length);
                    data = Encoding.UTF8.GetString(bytes, 0, i);
                    //Console.WriteLine($"Received: {data} {i}");
                    Packet packet = FindPacket(data, expectedId);
                    if(packet is null)
                        continue;
                    //Console.WriteLine($"After selection: {packet.Message.Length} {packet.Id}");
                    //Packet packet = JsonSerializer.Deserialize<Packet>(data);
                    if (packet.OwnerT != owner)
                        return packet.Message;
                }
                throw new Exception();
                return "";
            }
        }

        public static Packet FindPacket(string message, int id)
        {
            List<string> messages = new List<string>();
            string data = "";
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
            for (int i = 0; i < messages.Count; i++)
            {
                Packet packet = JsonSerializer.Deserialize<Packet>(messages[i]);
                if (packet.Id == id)
                {
                    last = packet;
                }
            }
            //Console.WriteLine($"Last: {last?.Id}");
            return last;
        }
        

        public static void SendViaClient(TcpClient client, string message, Packet.Owner owner, int id)
        {
            Packet packet = new Packet(){Id = id, Message = message, OwnerT = owner};
            Byte[] data =Encoding.UTF8.GetBytes( JsonSerializer.Serialize(packet)+"\0");

            // Get a client stream for reading and writing.
            //  Stream stream = client.GetStream();

            NetworkStream stream = client.GetStream();

            // Send the message to the connected TcpServer.
            stream.Write(data, 0, data.Length);
            //Console.WriteLine("Sent: {0}", JsonSerializer.Serialize(packet));
        }

        public static void SendObject(TcpClient client, object obj, Packet.Owner owner, int id)
        {
            SendViaClient(client, JsonSerializer.Serialize(obj), owner, id);
        }

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

        public static void Disconnect(TcpClient client)
        {
            client.GetStream().Close();
        }
    }
}