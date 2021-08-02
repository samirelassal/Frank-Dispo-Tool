using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frank_SO_Demand_Report;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    class Program
    {
        static int port_broadcast = 15000;
        static int port_communication = 8081;
        static TcpListener listener;
        static Thread broadcast_thread;
        static Thread tcp_thread;
        static List<TcpClient> Clients = new List<TcpClient>();
        static List<NetworkStream> Streams = new List<NetworkStream>();
        static string FileLocation = System.AppDomain.CurrentDomain.BaseDirectory;
        static string Calculated_Item_Path = FileLocation + @"\calculated_item.xml";
        static string Calculated_Sale_Path = FileLocation + @"\calculated_sale.xml";
        
        static byte[] Calculated_Item_File;
        static byte[] Calculated_Sale_File;

        static void Main(string[] args)
        {     
            Report report = new Report();
            report.Start("DSN=Navision Frank-Backup", FileLocation);
            
            Calculated_Item_File = File.ReadAllBytes(Calculated_Item_Path);
            Calculated_Sale_File = File.ReadAllBytes(Calculated_Sale_Path);
            Console.WriteLine("Welcome to the Frank SO Demand Server. This program connects to the Navision database, loads the data and distributes it in the network.");
            Console.WriteLine("This server uses two ports:");
            Console.WriteLine("{0} for broadcasting", port_broadcast);
            Console.WriteLine("{0} for the communication", port_communication);
            Console.WriteLine("\n" + new string('=', 79) + "\n");
            Console.WriteLine("Waiting for requests...\n");

            listener = new TcpListener(IPAddress.Any, port_communication);
            broadcast_thread = new Thread(new ThreadStart(ListenForBroadcast));
            broadcast_thread.Start();
            tcp_thread = new Thread(new ThreadStart(listenForCliens));
            tcp_thread.Start();
        }

        static void ListenForBroadcast()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipendpoint = new IPEndPoint(IPAddress.Any, port_broadcast);
            socket.Bind(ipendpoint);
            EndPoint endpoint = (EndPoint)ipendpoint;

            byte[] data = new byte[1024];
            while (true)
            {
                int recv = socket.ReceiveFrom(data, ref endpoint);
                string message = UTF8Encoding.UTF8.GetString(data).Trim('\0');
                Console.WriteLine("Received: {0}  from: {1}", message, endpoint.ToString());
                switch (message) 
                {
                    case "requesting server; frank ui": socket.SendTo(UTF8Encoding.UTF8.GetBytes(getIpAddress()), endpoint); break;
                }                
            }
        }

        static void listenForCliens() 
        {
            listener.Start();
            while (true) 
            {
                TcpClient client = listener.AcceptTcpClient();
                Clients.Add(client);
                Thread clientThread = new Thread(new ParameterizedThreadStart(handleClientConnection));
                clientThread.Start(client);
            }
        }

        static void handleClientConnection(object _client) 
        {            
            TcpClient client = (TcpClient)_client;
            Console.WriteLine("Client {0} has connected", client.Client.RemoteEndPoint.ToString());
            NetworkStream stream = client.GetStream();
            Streams.Add(stream);

            int bytesRead = 0;

            while (true) 
            {
                byte[] data = new byte[1024];
                try
                {
                    bytesRead = stream.Read(data, 0, data.Length);
                }
                catch (IOException ex) 
                {
                    Console.WriteLine("IOException with client {0}: {1}", client.Client.RemoteEndPoint.ToString(), ex.Message);
                    return;
                }
                
                if (bytesRead > 0) 
                {
                    string message = UTF8Encoding.UTF8.GetString(data).Trim('\0');
                    switch (message) 
                    {
                        case "requesting information":
                            message = "Calculated_Item_Size:" + Calculated_Item_File.Length.ToString() + "|Calculated_Sale_Size:" + Calculated_Sale_File.Length.ToString();
                            data = UTF8Encoding.UTF8.GetBytes(message);
                            stream.Write(data, 0, data.Length);
                            break;
                        case "requesting Calculated_Item":
                            stream.Write(Calculated_Item_File, 0, Calculated_Item_File.Length);
                            break;
                        case "requesting Calculated_Sale":
                            stream.Write(Calculated_Sale_File, 0, Calculated_Sale_File.Length);
                            break;
                    }
                }
            }
        }

        static void SendFile(NetworkStream stream, string Filename) 
        {
            byte[] file = File.ReadAllBytes(Filename);
        }

        static string getIpAddress() 
        {
            string address = "";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    address = ip.ToString();
                }
            }
            return address;
        }
    }
}
