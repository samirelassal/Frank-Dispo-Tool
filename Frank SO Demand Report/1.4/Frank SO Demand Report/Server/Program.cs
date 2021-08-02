﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frank_SO_Demand_Report;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FTP;
using System.Timers;
using System.Diagnostics;
using Helper_Library;

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
        static string Parent_Child_Item_Path = FileLocation + @"\parent_child_item.xml";

        static FileSystemWatcher fsw = new FileSystemWatcher(FileLocation, "*.xml");

        //static byte[] Calculated_Item_File;
        //static byte[] Calculated_Sale_File;
        //static byte[] Parent_Child_Item_File;

        static void Main(string[] args)
        {
            fsw.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.Security;
            fsw.Changed += new FileSystemEventHandler(fsw_Changed);
            fsw.Created += new FileSystemEventHandler(fsw_Changed);
            fsw.Renamed += new RenamedEventHandler(fsw_Changed);
            fsw.EnableRaisingEvents = true;
            UpdateData();
            Thread timer_thread = new Thread(StartTimer);
            timer_thread.Start();

            #region Load Files
            FtpClient ftp = new FtpClient(@"ftp://127.0.0.1/", "dispotool", "7Cf4pf2sF");
            ftp.upload("calculated_item.xml", Calculated_Item_Path);
            ftp.upload("calculated_sale.xml", Calculated_Sale_Path);
            ftp.upload("parent_child_item.xml", Parent_Child_Item_Path);
            #endregion

            #region UI
            Helper.LogWrite("Started");
            Console.WriteLine("Welcome to the Frank SO Demand Server. This program connects to the Navision database, loads the data and distributes it in the network.");
            Console.WriteLine("This server uses two ports:");
            Console.WriteLine("{0} for broadcasting", port_broadcast);
            Console.WriteLine("{0} for the communication", port_communication);
            Console.WriteLine("\n" + new string('=', 79) + "\n");
            Console.WriteLine("Waiting for requests...\n");
            #endregion

            #region Broadcast listener
            listener = new TcpListener(IPAddress.Any, port_communication);
            broadcast_thread = new Thread(new ThreadStart(ListenForBroadcast));
            broadcast_thread.Start();
            #endregion

            #region client-connection listener
            tcp_thread = new Thread(new ThreadStart(listenForCliens));
            tcp_thread.Start();
            #endregion
        }

        static void StartTimer() 
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 3600 * 1000;
            timer.AutoReset = true;
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            while (true) 
            {
                if (DateTime.Now.Hour >= 5) 
                {
                    timer.Enabled = true;
                }
                if (DateTime.Now.Hour >= 18) 
                {
                    timer.Enabled = false;
                }
            }
        }

        private static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e) 
        {            
            UpdateData();
        }

        static void UpdateData()
        {
            Helper.LogWrite(ConsoleColor.Green, "Data update started");
            System.Diagnostics.Process.Start(new FileInfo(FileLocation).Directory.Parent.Parent.Parent.FullName + @"\Frank SO Demand Report\bin\Release\Frank SO Demand Report.exe", "-filelocation \"" + FileLocation + "\"");
        }
        private static void UpdateData_Thread()
        {
            Report report = new Report();
            report.Start("DSN=Navision Frank-Live", FileLocation);
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
                Helper.LogWrite(ConsoleColor.Magenta, "Broadcast recieved from: {0}", endpoint.ToString());
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
            Helper.LogWrite(ConsoleColor.Magenta, "Client {0} has connected", client.Client.RemoteEndPoint.ToString());
            NetworkStream stream = client.GetStream();
            Streams.Add(stream);

            bool isConnected = true;

            while (isConnected)
            {
                int bytesRead = 0;
                byte[] data = new byte[1024];
                try
                {
                    bytesRead = stream.Read(data, 0, data.Length);
                }
                catch (IOException ex)
                {
                    Helper.ErrorMessage("IOException with client {0}: {1}", client.Client.RemoteEndPoint.ToString(), ex.Message);
                    return;
                }

                if (bytesRead > 0)
                {
                    string message = UTF8Encoding.UTF8.GetString(data).Trim('\0');
                    switch (message)
                    {
                        case "update_server_data": UpdateData(stream); break;
                        case "close_connection": Helper.LogWrite(ConsoleColor.Magenta, "Client {0} has disconnected\r\n", client.Client.RemoteEndPoint.ToString()); isConnected = false; break;
                        case "requesting_server_information": SendServerInformation(stream); break;
                    }
                }
            }
        }

        #region Request Handlers
        private static void SendServerInformation(NetworkStream stream)
        {
            FileInfo Calculated_Item_Info = new FileInfo(Calculated_Item_Path);
            FileInfo Calculated_Sale_Info = new FileInfo(Calculated_Sale_Path);
            FileInfo Parent_Child_Item_Info = new FileInfo(Parent_Child_Item_Path);

            DateTime Calculated_Item_Date = Calculated_Item_Info.LastWriteTime;
            DateTime Calculated_Sale_Date = Calculated_Sale_Info.LastWriteTime;
            DateTime Parent_Child_Item_Date = Parent_Child_Item_Info.LastWriteTime;

            string date;

            #region The oldest date will be taken
            if (Calculated_Item_Date < Calculated_Sale_Date)
            {
                if (Calculated_Item_Date < Parent_Child_Item_Date)
                    date = Calculated_Item_Date.ToString();
                else
                    date = Parent_Child_Item_Date.ToString();
            }
            else
            {
                if (Calculated_Sale_Date < Parent_Child_Item_Date)
                    date = Calculated_Sale_Date.ToString();
                else
                    date = Parent_Child_Item_Date.ToString();
            }
            #endregion

            date += "|";
            byte[] data = UTF8Encoding.UTF8.GetBytes(date);

            streamWrite(stream, data);

        }
        private static void UpdateData(NetworkStream stream)
        {
            Thread thread = new Thread(new ParameterizedThreadStart(UpdateData_Thread));
            thread.Start(stream);
        }
        private static void UpdateData_Thread(object _stream)
        {
            NetworkStream stream = (NetworkStream)_stream;
            Report report = new Report();
            report.Start("DSN=Navision Frank-Live", FileLocation);
            byte[] data = UTF8Encoding.UTF8.GetBytes("updating_data_done");
            streamWrite(stream, data);
        }
        #endregion

        private static void streamWrite(NetworkStream stream, byte[] data) 
        {
            try
            {
                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex) { Helper.ErrorMessage(ex); }
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

        static void fsw_Changed(object sender, FileSystemEventArgs e) 
        {
            Helper.LogWrite(ConsoleColor.Green, "New XML files uploaded to FTP-Server");
            FtpClient ftp = new FtpClient(@"ftp://127.0.0.1/", "dispotool", "7Cf4pf2sF");
            ftp.upload("calculated_item.xml", Calculated_Item_Path);
            ftp.upload("calculated_sale.xml", Calculated_Sale_Path);
            ftp.upload("parent_child_item.xml", Parent_Child_Item_Path);
        }
    }
}
