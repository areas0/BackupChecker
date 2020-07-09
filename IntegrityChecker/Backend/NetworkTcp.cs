using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IntegrityChecker.DataTypes;
using IntegrityChecker.Scheduler;

namespace IntegrityChecker.Backend
{
    public static class NetworkTcp
    {
        public static string Receive(TcpClient client, Packet.Owner owner, int expectedId)
        {
            var data = "";
            Logger.Instance.Log(Logger.Type.Ok, $"Receiver started: owner: {owner} expectedId: {expectedId}");
            try
            {
                // maximum size of message, large size is chosen for the moment
                var bytes = new byte[1048576*2*2*2*2*2];
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
                        data = Encoding.UTF8.GetString(bytes, 0, i);
                        var packet = FindPacket(data, expectedId);
                        if (packet is null)
                            continue;

                        if (packet.OwnerT != owner)
                        {
                            Logger.Instance.Log(Logger.Type.Ok, $"Received a packet owner: {packet.OwnerT} Id: {packet.Id} data: {packet.Message}");
                            return packet.Message;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Log(Logger.Type.Error, $"Receive: owner: {owner} expectedId: {expectedId} data: {data}");
                throw new Exception("Receiver failed to receive data, last piece of data : "+data+"\n"+e.Message);
            }
        }

        private static Packet FindPacket(string message, int id)
        {
            Logger.Instance.Log(Logger.Type.Ok, $"Current stream: {message} with id {id}");
            var messages = new List<string>();
            //var data = "";
            // foreach (var t in message)
            // {
            //     if (t != '\0')
            //         data += t;
            //     else
            //     {
            //         messages.Add(data);
            //         data = "";
            //     }
            // }
            for (var i = 0; i < message.Length; i++)
            {
                var j = i;
                // if LastIndexOf returns -1 it means that there are no more packet in the message
                if (message.LastIndexOf('\0') == -1)
                    break;
                i = message.LastIndexOf('\0');
                messages.Add(message.Substring(j, i-j));
            }
            Logger.Instance.Log(Logger.Type.Ok,$"Current data {messages[0]}");

            Packet last = null;
            foreach (var t in messages)
            {
                var packet = JsonSerializer.Deserialize<Packet>(t);
                if (packet.Id == id) 
                    last = packet;
            }
            //Console.WriteLine($"Last: {last?.Id}");
            Logger.Instance.Log(Logger.Type.Ok,
                $"Find packet: returned value is {(last is null ? "Empty" : last.Message)} " +
                $"id: {(last is null ? "None" : Convert.ToString(last.Id))}");
            return last;
        }
        

        public static void SendViaClient(TcpClient client, string message, Packet.Owner owner, int id)
        {
            try
            {
                Logger.Instance.Log(Logger.Type.Ok, $"SendObject started, owner: {owner} Id: {id} Message: {message}");
                var packet = new Packet() {Id = id, Message = message, OwnerT = owner};
                var data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(packet) + "\0"); // \0 is used as a message delimiter 

                // Get a client stream for reading and writing.

                var stream = client.GetStream();

                // Send the message to the connected TcpServer.
                stream.Write(data, 0, data.Length);
                //Console.WriteLine("Sent: {0}", JsonSerializer.Serialize(packet));
            }
            catch (Exception e)
            {
                Logger.Instance.Log(Logger.Type.Error, $"SendViaClient failed, owner: {owner} Id: {id} Message: {message}");
                throw new Exception("Sender failed \n "+e);
            }
        }

        public static void SendObject(TcpClient client, object obj, Packet.Owner owner, int id)
        {
            try
            {
                var message = JsonSerializer.Serialize(obj);
                SendViaClient(client, message, owner, id);
                Logger.Instance.Log(Logger.Type.Ok, $"SendObject succeeded, owner: {owner} Id: {id} Message: {message}");
            }
            catch (Exception e)
            {
                Logger.Instance.Log(Logger.Type.Error, $"SendObject failed: Exception raised: {e.StackTrace} {e.Message}");
                throw;
            }
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
            Logger.Instance.Log(Logger.Type.Ok, "Successfully disconnected client");
        }
    }
}