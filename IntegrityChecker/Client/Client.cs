﻿using System;
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

namespace IntegrityChecker.Client
{
    public class Client
    {
        private Socket _sender;

        public string Origin =
            @"D:\Anime\[TheFantastics] Violet Evergarden  - VOSTFR (Bluray x264 10bits 1080p FLAC)\[Nemuri] Violet Evergarden ヴァイオレット・エヴァーガーデン (2018-2020) [FLAC]";
        public void Init()
        { 
            byte[] bytes = new byte[1024];  
  
            try  
            {  
                // Connect to a Remote server  
                // Get Host IP Address that is used to establish a connection  
                // In this case, we get one IP address of localhost that is IP : 127.0.0.1  
                // If a host has multiple addresses, you will get a list of addresses  
                IPHostEntry host = Dns.GetHostEntry("localhost");  
                IPAddress ipAddress = host.AddressList[0];  
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);  
  
                // Create a TCP/IP  socket.    
                _sender = new Socket(ipAddress.AddressFamily,  
                    SocketType.Stream, ProtocolType.Tcp);  
  
                // Connect the socket to the remote endpoint. Catch any errors.    
                try  
                {  
                    // Connect to Remote EndPoint  
                    _sender.Connect(remoteEP);  
  
                    /*Console.WriteLine("Socket connected to {0}",  
                        sender.RemoteEndPoint.ToString());  
  
                    // Encode the data string into a byte array.    
                    byte[] msg = Encoding.ASCII.GetBytes("<EOF>");  
  
                    // Send the data through the socket.    
                    //int bytesSent = sender.Send(msg);  
                    sender.SendFile(
                        @"C:\Users\Shadow\RiderProjects\IntegrityChecker\IntegrityChecker\bin\Release\netcoreapp3.1\Export - tets lundi 6 juillet 2020.json");
                    sender.Send(msg);
  
                    // Receive the response from the remote device.    
                    int bytesRec = sender.Receive(bytes);  
                    Console.WriteLine("Echoed test = {0}",  
                        Encoding.UTF8.GetString(bytes, 0, bytesRec));  
  
                    // Release the socket.    
                    sender.Shutdown(SocketShutdown.Both);  
                    sender.Close();  */
  
                }  
                catch (ArgumentNullException ane)  
                {  
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());  
                }  
                catch (SocketException se)  
                {  
                    Console.WriteLine("SocketException : {0}", se.ToString());  
                }  
                catch (Exception e)  
                {  
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());  
                }  
  
            }  
            catch (Exception e)  
            {  
                Console.WriteLine(e.ToString());  
            }
            ReceiveBackupCommand();
        }

        public void ReceiveBackupCommand()
        {
            byte[] bytes = new byte[1024];  
            int bytesRec = _sender.Receive(bytes);

            string message = Encoding.UTF8.GetString(bytes, 0, bytesRec);
            Tasks task = JsonSerializer.Deserialize<Tasks>(message);
            if (task.OriginName == Origin)
            {
                string status = JsonSerializer.Serialize(Status.Ok);
                _sender.Send(Encoding.UTF8.GetBytes(status));
            }
            else
            {
                string status = JsonSerializer.Serialize(Status.Error);
                _sender.Send(Encoding.UTF8.GetBytes(status));
                _sender.Close();
                throw new Exception("Backup init failed, exiting...");
            }

            switch (task.Current)
            {
                case Tasks.Task.Original:
                    throw new InvalidDataException("Client cannot be original folder!");
                    break;
                case Tasks.Task.Backup:
                    Console.WriteLine("Starting backup now!");
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
            var backup = Backup(Origin);
            bool finished = false;
            while (!finished || !backup.IsCompleted)
            {
                if (!backup.IsCompleted)
                {
                    _sender.Send(JsonSerializer.SerializeToUtf8Bytes(Status.Waiting));
                }
                else
                {
                    _sender.Send(JsonSerializer.SerializeToUtf8Bytes(Status.Ok));
                }
                Console.WriteLine("In loop");
                var bytes = new byte[10000];
                int bytesRec = _sender.Receive(bytes);  
                var data = Encoding.UTF8.GetString(bytes, 0, bytesRec); 
                Console.WriteLine(JsonSerializer.Deserialize<Status>(data));
                if (JsonSerializer.Deserialize<Status>(data) == Status.Ok)
                {
                    finished = true;
                }

                await Task.Delay(1000);
            }
            Console.WriteLine("Outside");
            string folder = await backup;
            Console.WriteLine("Finished");
            _sender.Send(JsonSerializer.SerializeToUtf8Bytes(Status.Ok));
            ProceedResults(folder);
        }
        public static async Task<string> Backup(string origin)
        {
            await Task.Delay(3000);
            string folder = new Folder(origin).ExportJson();
            Console.WriteLine("Sha1 generation finished");
            return folder;
        }

        public void ProceedResults(string folder)
        {
            _sender.Send(Encoding.UTF8.GetBytes(folder));
            GetResults();
        }

        public void GetResults()
        {
            Console.Write("Waiting for results....");
            // var bytesb = new byte[10000];
            // int bytesRec = _sender.Receive(bytesb);
            // var str = Encoding.UTF8.GetString(bytesb, 0, bytesRec);
            string data = string.Empty;
            while (true)  
            {  
                var bytes = new byte[10000];  
                int bytesRec = _sender.Receive(bytes);  
                data += Encoding.UTF8.GetString(bytes, 0, bytesRec);  
                if (data.IndexOf("<EOF>", StringComparison.Ordinal) > -1)  
                {  
                    break;  
                }  
            }

            string result = data.Substring(0, data.Length - 5);
            Result Result = JsonSerializer.Deserialize<Result>(result);
            Console.Write(Result.ErrorCount+Result.ErrorMessage);
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