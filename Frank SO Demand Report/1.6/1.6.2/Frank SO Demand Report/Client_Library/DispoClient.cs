using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using Helper_Library;
using System.ComponentModel;
using System.Windows;

namespace Client_Library
{
    public struct ServerInfo 
    {
        public DateTime LastUpdated { get; private set; }
        public string RequiredVersion { get; private set; }
        internal ServerInfo(DateTime lastupdated, string requiredversion) : this()
        {
            LastUpdated = lastupdated;
            RequiredVersion = requiredversion;
        }
    }

    public delegate void ServerNotificationEventHandler(string message);

    public static class DispoClient
    {
        public static event ServerNotificationEventHandler ServerNotificationReceived;

        static BackgroundWorker worker = new BackgroundWorker();

        static readonly int timeout = 15000;

        static int port_broadcast = 15000;
        static int port_communication = 8081;
        static int port_notification = 8082;
        public static string Server_IP { get; private set; }
        static UdpClient udp = new UdpClient();
        static TcpClient tcp;
        static IPEndPoint ipendpoint;

        public static string ClientVersion = "0.6.5";

        static ServerInfo? _serverinfo = null;
        public static ServerInfo? serverInfo 
        {
            get { return _serverinfo; }
            private set { _serverinfo = value; }
        }

        static byte[] _Calculated_Item_File;
        static byte[] _Calculated_Sale_File;
        static byte[] _Parent_Child_Item_File;

        public static byte[] Calculated_Item_File 
        {
            get { return _Calculated_Item_File; }
            private set { _Calculated_Item_File = value; }
        }
        public static byte[] Calculated_Sale_File
        {
            get { return _Calculated_Sale_File; }
            private set { _Calculated_Sale_File = value; }
        }
        public static byte[] Parent_Child_Item_File
        {
            get { return _Parent_Child_Item_File; }
            private set { _Parent_Child_Item_File = value; }
        }

        static readonly string FileLocation = System.AppDomain.CurrentDomain.BaseDirectory;
        public static readonly string Calculated_Item_Path = FileLocation + @"\calculated_item.xml";
        public static readonly string Calculated_Sale_Path = FileLocation + @"\calculated_sale.xml";
        public static readonly string Parent_Child_Item_Path = FileLocation + @"\parent_child_item.xml";
        public static readonly string SalesHeader_Path = FileLocation + @"\salesheader.xml";

        static DispoClient() 
        {
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = false;
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);
        }

        private static void worker_DoWork(object sender, DoWorkEventArgs e) 
        {
            try
            {
                byte[] data = new byte[1024];
                TcpListener listener = new TcpListener(IPAddress.Any, port_notification);
                listener.Start();
                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();
                    stream.Read(data, 0, data.Length);
                    worker.ReportProgress(0, UTF8Encoding.UTF8.GetString(data).Trim('\0'));
                }
            }
            catch (Exception ex) { Helper.ErrorMessage(ex); }
            finally { if (tcp.Connected) CloseConnection(tcp); }
        }
        private static void worker_ProgressChanged(object sender, ProgressChangedEventArgs e) 
        {
            Helper.LogWrite((string)e.UserState);
            if (ServerNotificationReceived != null)
                ServerNotificationReceived((string)e.UserState);            
        }

        private static bool SendBroadcast()
        {
            try
            {
                udp.Client.ReceiveTimeout = timeout;
                IPEndPoint ipendpoint = new IPEndPoint(IPAddress.Broadcast, port_broadcast);
                string message = "requesting server; frank ui";
                byte[] data = UTF8Encoding.UTF8.GetBytes(message);
                udp.Send(data, data.Length, ipendpoint);
                data = udp.Receive(ref ipendpoint);
                Server_IP = UTF8Encoding.UTF8.GetString(data);
                return true;
            }
            catch (Exception ex) { ErrorMessage(ex); return false; }
        }
        private static void CloseConnection(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] data = UTF8Encoding.UTF8.GetBytes("close_connection");
            stream.Write(data, 0, data.Length);
            client.Close();
        }

        private static void ErrorMessage(Exception ex)
        {
            var standard = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(new string('=', 37) + "Error" + new string('=', 37));
            Console.WriteLine("Error Type:     {0}", ex.GetType().ToString());
            Console.WriteLine("Error Message:  {0}", ex.Message);
            Console.WriteLine("\nStack Trace:  {0}", ex.StackTrace);

            Console.WriteLine(new string('=', 79));
            Console.ForegroundColor = standard;
            Console.ReadLine();
        }
        private static void ErrorMessage(string message)
        {
            var standard = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(new string('=', 37) + "Error" + new string('=', 37));
            Console.WriteLine("Message:  {0}", message);
            Console.WriteLine(new string('=', 79));
            Console.ForegroundColor = standard;
            Console.ReadLine();
        }

        /// <summary>
        /// induce server to update the navision data
        /// </summary>
        public static void RequestServerInformation()
        {
            bool broadcast_result = true;
            if (Server_IP == null)
                broadcast_result = SendBroadcast();

            if (broadcast_result)
            {
                tcp = new TcpClient();
                tcp.ReceiveTimeout = timeout;
                ipendpoint = new IPEndPoint(IPAddress.Parse(Server_IP), port_communication);
                byte[] data = new byte[1024];
                try
                {
                    tcp.Connect(ipendpoint);
                    NetworkStream stream = tcp.GetStream();
                    data = UTF8Encoding.UTF8.GetBytes("requesting_server_information");
                    stream.Write(data, 0, data.Length);
                    data = new byte[1024];
                    stream.Read(data, 0, data.Length);
                    string info_string = UTF8Encoding.UTF8.GetString(data);
                    string[] info_string_segments = info_string.Split('|');
                    _serverinfo = new ServerInfo(DateTime.Parse(info_string_segments[0]), info_string_segments[1]);
                }
                catch (Exception ex) { ErrorMessage(ex); }
                finally
                {
                    if (tcp.Connected) CloseConnection(tcp);
                    if (!worker.IsBusy) worker.RunWorkerAsync();
                }
            }
        }

        public static string GetData()
        {
            string result = "";
            RequestServerInformation();
            if (serverInfo != null)
            {
                if (Helper.CompareVersion(serverInfo.Value.RequiredVersion, ClientVersion))
                {
                    try
                    {
                        FTP.FtpClient ftp = new FTP.FtpClient(@"ftp://" + Server_IP + "/", "dispotool", "7Cf4pf2sF");
                        ftp.download("calculated_item.xml", Calculated_Item_Path);
                        ftp.download("calculated_sale.xml", Calculated_Sale_Path);
                        ftp.download("parent_child_item.xml", Parent_Child_Item_Path);
                        ftp.download("salesheader.xml", SalesHeader_Path);
                    }
                    catch (Exception ex) { ErrorMessage(ex); }
                    finally { if (tcp.Connected) CloseConnection(tcp); }
                }
                else
                {
                   Helper.LogWrite(result = String.Format("Inkompatibel: {0} benötigt", serverInfo.Value.RequiredVersion));
                }
            }
            else
                ErrorMessage("In GetData(): Es konnte keine Verbindung zum Server hergestellt werden!");
            return result;
        }

        static void Main(string[] args)
        {
            GetData();
            while (true)
                Thread.Sleep(100);
        }
    }
}
