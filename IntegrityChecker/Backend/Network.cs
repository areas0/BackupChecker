using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace IntegrityChecker.Backend
{
    public abstract class Network
    {
        public static string Receive(Socket socket)
        {
            string data = string.Empty;
            while (true)  
            {  
                var bytes = new byte[10000];  
                int bytesRec = socket.Receive(bytes);
                Console.Write("Received");
                data += Encoding.UTF8.GetString(bytes, 0, bytesRec);  
                if (data.IndexOf("<EOF>", StringComparison.Ordinal) > -1)  
                {  
                    break;  
                }  
            }
            Console.WriteLine(data);
            return data.Substring(0, data.Length - 5);
        }

        public static void Send(Socket socket, string data)
        {
            socket.Send(Encoding.UTF8.GetBytes(data));
            byte[] msg = Encoding.ASCII.GetBytes("<EOF>"); 
            socket.Send(msg);
        }

        public static void SerializedSend(Socket socket, object obj)
        {
            socket.Send(JsonSerializer.SerializeToUtf8Bytes(obj));
            byte[] msg = Encoding.ASCII.GetBytes("<EOF>"); 
            socket.Send(msg);
        }
    }
}