using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using IntegrityChecker.DataTypes;

namespace IntegrityChecker.Backend
{
    public abstract class Network
    {
        public static string Receive(Socket socket, bool debug = false)
        {
            string data = string.Empty;
            while (true)  
            {  
                var bytes = new byte[65536];  
                int bytesRec = socket.Receive(bytes);
                data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                // if (data == "0<EOF>" && debug)
                // {
                //     data = string.Empty;
                // }
                //
                // if (debug) 
                //     data += "";
                if (data.IndexOf("<EOF>") > -1)
                    break;
            }
            Console.WriteLine(data.Substring(0, data.Length-5 ));
            return Seperate(data, debug);
            return data.Substring(0, data.Length - 5);
        }
        //TODO: rework to include TCP instead of socket client
        public static string Seperate(string data, bool debug = false)
        {
            string finalData = data;
            while (finalData.IndexOf("<EOF>") > -1)
            {
                string data1 = finalData.Substring(0, finalData.IndexOf("<EOF>"));
                string data2 = finalData.Substring(finalData.IndexOf("<EOF>") + 5);
                int i = 0;
                Console.WriteLine("Seperate");
                try
                {
                    JsonSerializer.Deserialize<Result>(data1);
                    i++;
                    JsonSerializer.Deserialize<Result>(Seperate(data2));
                }
                catch (Exception)
                {
                }
                finalData = finalData.Substring(0, finalData.IndexOf("<EOF>"));
            }
            Console.WriteLine($"Final data: {finalData}, original data: {data}");
            return finalData;
        }

        public static void Send(Socket socket, string data)
        {
            socket.Send(Encoding.UTF8.GetBytes(data));
            byte[] msg = Encoding.UTF8.GetBytes("<EOF>"); 
            socket.Send(msg);
        }

        public static void SerializedSend(Socket socket, object obj)
        {
            socket.Send(JsonSerializer.SerializeToUtf8Bytes(obj));
            byte[] msg = Encoding.UTF8.GetBytes("<EOF>"); 
            socket.Send(msg);
        }

        public static void EmptyBuffer(Socket socket)
        {
            string data = string.Empty;
            while (true)  
            {  
                var bytes = new byte[65536];  
                int bytesRec = socket.Receive(bytes);
                data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                // if (data == "0<EOF>" && debug)
                // {
                //     data = string.Empty;
                // }
                //
                // if (debug) 
                //     data += "";
                if (data.IndexOf("<EOF>") > -1)
                    break;
            }
            Console.WriteLine(data);
        }
    }
}