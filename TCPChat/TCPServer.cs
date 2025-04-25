using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TCPChat
{
    internal class TCPServer
    {
        TcpListener server;
        bool isRunning;

        List<TcpClient> clients = new List<TcpClient>();
        public TCPServer(int port)
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            Console.WriteLine($"Server started on port {port}");
            isRunning = true;
            ServerLoop();
        }


        private void ServerLoop() 
        {
            while (isRunning)
            {
                TcpClient client = server.AcceptTcpClient();
                clients.Add(client);
                Console.WriteLine("Client connected");
                Thread clientThread = new Thread(ClientLoop);
                clientThread.Start(client);
            }
        }

        private void ClientLoop(object? o)
        {
            if (o == null) return;

            TcpClient client = (TcpClient)o;
            clients.Add(client);

            string username = "null";

            try
            {
                StreamReader sr = new StreamReader(client.GetStream(), Encoding.UTF8);
                StreamWriter sw = new StreamWriter(client.GetStream(), Encoding.UTF8);

                sr.ReadLine();

                sw.WriteLine("##Connected");
                sw.Flush();

                sw.WriteLine("##Welcome to the chat server, write your username:");
                sw.Flush();

                username = sr.ReadLine();
                sw.Flush();

                while (true)
                {
                    sw.Write(">> ");
                    sw.Flush();
                    string message = sr.ReadLine();
                    if (message == null) break;

                    foreach (var c in clients)
                    {
                        if (c != client)
                        {
                            StreamWriter otherSw = new StreamWriter(c.GetStream(), Encoding.UTF8);
                            otherSw.WriteLine(username + ":" + message);
                            otherSw.Flush();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                client.Close();
                clients.Remove(client);
                Console.WriteLine($"Client {username} disconnected");
            }
        }
    }
}
