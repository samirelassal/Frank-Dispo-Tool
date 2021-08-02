using System;
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
using System.Xml;
using System.Security.Cryptography;
using System.Globalization;

namespace Server
{
    class Program
    {
        static int port_broadcast = 15000;
        static int port_communication = 8081;
        static int port_notification = 8082;
        static TcpListener listener;
        static Thread broadcast_thread;
        static Thread tcp_thread;
        static List<EndPoint> Endpoints = new List<EndPoint>();
        static string FileLocation = System.AppDomain.CurrentDomain.BaseDirectory;

        static string Parent_Child_Item_Path = FileLocation + @"\parent_child_item.xml";
        static string Final_Data_Path = FileLocation + @"\final_data.xml";
        static string ClientList_Path = FileLocation + @"\ClientList.lst";
        static string required_client_version = "0.8.0";

        static FileSystemWatcher fsw = new FileSystemWatcher(FileLocation, "*.xml");

        static void Main(string[] args)
        {
            GetHashOfFiles();
            fsw.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.Security;
            fsw.Changed += new FileSystemEventHandler(fsw_Changed);
            fsw.Created += new FileSystemEventHandler(fsw_Changed);
            fsw.Renamed += new RenamedEventHandler(fsw_Changed);
            fsw.EnableRaisingEvents = true;
            UpdateData();
            Thread timer_thread = new Thread(StartTimer);
            timer_thread.Start();

            ReadEndpointsFromFile();

            #region Load Files
            try
            {
                FtpClient ftp = new FtpClient(@"ftp://127.0.0.1/", "dispotool", "7Cf4pf2sF");
                ftp.upload("parent_child_item.xml", Parent_Child_Item_Path);
                ftp.upload("final_data.xml", Final_Data_Path);
            }
            catch (Exception ex) { Helper.ErrorMessage(ex); }
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
                Thread clientThread = new Thread(new ParameterizedThreadStart(handleClientConnection));
                clientThread.Start(client);
            }
        }

        static void handleClientConnection(object _client)
        {
            TcpClient client = (TcpClient)_client;
            Helper.LogWrite(ConsoleColor.Magenta, "Client {0} has connected", client.Client.RemoteEndPoint.ToString());
            NetworkStream stream = client.GetStream();
            Endpoints.Add(client.Client.RemoteEndPoint);

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
                        case "close_connection": Helper.LogWrite(ConsoleColor.Magenta, "Client {0} has disconnected\r\n", client.Client.RemoteEndPoint.ToString()); isConnected = false; break;
                        case "requesting_server_information": SendServerInformation(stream); break;
                    }
                }
            }
        }

        #region Request Handlers
        private static void SendServerInformation(NetworkStream stream)
        {
            string message = getLastDataUpdate().ToString();

            message += "|";
            message += required_client_version + "|";
            message += GetHashOfFiles() + "|";

            byte[] data = UTF8Encoding.UTF8.GetBytes(message);

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

        static bool Parent_Child_Item_Changed = false;
        static bool Final_Data_Changed = false;
        static void fsw_Changed(object sender, FileSystemEventArgs e)
        {            
            FtpClient ftp = new FtpClient(@"ftp://127.0.0.1/", "dispotool", "7Cf4pf2sF");
            switch (e.Name)
            {
                case "parent_child_item.xml": Parent_Child_Item_Changed = true; break;
                case "final_data.xml": Final_Data_Changed = true; break;
            }
            if (Final_Data_Changed && Parent_Child_Item_Changed)
            {
                System.Threading.Thread.Sleep(1000);
                ftp.upload("parent_child_item.xml", Parent_Child_Item_Path);
                ftp.upload("final_data.xml", Final_Data_Path);

                Helper.LogWrite(ConsoleColor.Green, "New XML files uploaded to FTP-Server");
                Final_Data_Changed = Parent_Child_Item_Changed = false;
                NotifyDataUpdate();
            }
        }
        

        static DateTime getLastDataUpdate() 
        {
            FileInfo Parent_Child_Item_Info = new FileInfo(Parent_Child_Item_Path);
            FileInfo Final_Data_Info = new FileInfo(Final_Data_Path);

            List<DateTime> times = new List<DateTime> { Final_Data_Info.LastWriteTime, Parent_Child_Item_Info.LastWriteTime };
            times.Sort();
            DateTime lastupdate = times.Last<DateTime>();
            return lastupdate;
        }

        static void NotifyDataUpdate()
        {
            string message = "frank_dispo_tool_server_data_update:" + getLastDataUpdate().ToString();
            for(int i = 0; i < Endpoints.Count; i++)
            {
                EndPoint endpoint = Endpoints[i];
                try
                {
                    TcpClient tcp = new TcpClient();
                    IPEndPoint ipep = (IPEndPoint)endpoint;
                    if (Endpoints.Count(item => item.ToString().Split(':')[0] == endpoint.ToString().Split(':')[0]) > 1)
                    {
                        Endpoints.Remove(endpoint);
                        i--;
                    }
                    else
                    {
                        ipep.Port = port_notification;
                        tcp.Connect(ipep);
                        NetworkStream stream = tcp.GetStream();
                        Helper.LogWrite("Notification sent: " + ipep.ToString());
                        byte[] data = UTF8Encoding.UTF8.GetBytes(message);
                        stream.Write(data, 0, data.Length);
                        tcp.Close();
                    }
                }
                catch (Exception ex) { Helper.ErrorMessage(ex); Endpoints.Remove(endpoint); i--; }
            }
            WriteEndpointsToFile();
        }

        /// <summary>
        /// This method generates an xml file that contains the hashcode of each file so that the client can check the validity of the files
        /// </summary>
        /// <returns></returns>
        public static string GetHashOfFiles() 
        {
            string result = "";
            try
            {
                MD5 md5 = MD5.Create();

                FileStream parentchilditem_stream = File.OpenRead(Parent_Child_Item_Path);
                FileStream final_data_stream = File.OpenRead(Final_Data_Path);

                byte[] parentchilditem_hash = md5.ComputeHash(parentchilditem_stream);
                byte[] final_data_hash = md5.ComputeHash(final_data_stream);

                StringBuilder parentchilditem_hashstring = new StringBuilder();
                StringBuilder final_data_hashstring = new StringBuilder();

                for (int i = 0; i < final_data_hash.Length; i++)
                {
                    parentchilditem_hashstring.Append(parentchilditem_hash[i].ToString("x2"));
                    final_data_hashstring.Append(final_data_hash[i].ToString("x2"));
                }
                result = String.Format("Final_Data:{0};Parent_Child_Item:{1}", final_data_hashstring.ToString(), parentchilditem_hashstring.ToString());
            }
            catch (Exception ex) 
            {
                Helper.ErrorMessage(ex);
            }
            return result;
        }

        public static void WriteEndpointsToFile() 
        {
            try
            {
                string result = "";
                foreach (EndPoint ep in Endpoints)
                {
                    result += ep.ToString() + "|";
                }
                File.WriteAllText(ClientList_Path, result);
            }
            catch (Exception ex) { Helper.ErrorMessage(ex); }
        }

        public static void ReadEndpointsFromFile()
        {
            try
            {
                string content = File.ReadAllText(ClientList_Path);
                string[] segments = content.Split('|');
                foreach (string str in segments)
                {
                    if (!String.IsNullOrEmpty(str))
                        Endpoints.Add(CreateIPEndPoint(str));
                }
            }
            catch (Exception ex) { Helper.ErrorMessage(ex); }
        }

        public static IPEndPoint CreateIPEndPoint(string endPoint)
        {
            string[] ep = endPoint.Split(':');
            if (ep.Length != 2) throw new FormatException("Invalid endpoint format");
            IPAddress ip;
            if (!IPAddress.TryParse(ep[0], out ip))
            {
                throw new FormatException("Invalid ip-adress");
            }
            int port;
            if (!int.TryParse(ep[1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out port))
            {
                throw new FormatException("Invalid port");
            }
            return new IPEndPoint(ip, port);
        }
    }
}
