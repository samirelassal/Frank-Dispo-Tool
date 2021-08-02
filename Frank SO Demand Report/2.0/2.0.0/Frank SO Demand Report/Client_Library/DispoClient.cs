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
using System.Security.Cryptography;

namespace Client_Library
{
    public struct Hashes
    {
        public string Calculated_Item_Hash { get; private set; }
        public string Calculated_Sale_Hash { get; private set; }
        public string Parent_Child_Item_Hash { get; private set; }
        public string SalesHeader_Hash { get; private set; }

        internal Hashes(string ServerResult) : this()
        {
            string[] segments = ServerResult.Split(';');
            Calculated_Item_Hash = segments[0].Split(':')[1];
            Calculated_Sale_Hash = segments[1].Split(':')[1];
            Parent_Child_Item_Hash = segments[2].Split(':')[1];
            SalesHeader_Hash = segments[3].Split(':')[1];
        }
    }

    public struct ServerInfo 
    {
        public DateTime LastUpdated { get; private set; }
        public string RequiredVersion { get; private set; }
        public Hashes Hashes { get; private set; }
        internal ServerInfo(DateTime lastupdated, string requiredversion, Hashes hashes) : this()
        {
            LastUpdated = lastupdated;
            RequiredVersion = requiredversion;
            Hashes = hashes;
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

        public static string ClientVersion = "0.8";

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
            catch (Exception ex) { Helper.ErrorMessage(ex); return false; }
        }
        private static void CloseConnection(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] data = UTF8Encoding.UTF8.GetBytes("close_connection");
            stream.Write(data, 0, data.Length);
            client.Close();
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
                    _serverinfo = new ServerInfo(DateTime.Parse(info_string_segments[0]), info_string_segments[1], new Hashes(info_string_segments[2]));
                }
                catch (Exception ex) { Helper.ErrorMessage(ex); }
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
                        ftp.download("calculated_item.xml", Calculated_Item_Path + "_1");
                        ftp.download("calculated_sale.xml", Calculated_Sale_Path + "_1");
                        ftp.download("parent_child_item.xml", Parent_Child_Item_Path + "_1");
                        ftp.download("salesheader.xml", SalesHeader_Path + "_1");
                    }
                    catch (Exception ex) { Helper.ErrorMessage(ex); }
                    finally { if (tcp.Connected) CloseConnection(tcp); }

                    try
                    {
                        Hashes newhashes = GetHashOfFiles();
                        //check validity of files
                        if (newhashes.Calculated_Item_Hash == serverInfo.Value.Hashes.Calculated_Item_Hash)
                            File.Copy(Calculated_Item_Path + "_1", Calculated_Item_Path, true);
                        else
                            Helper.ErrorMessage(result += Calculated_Item_Path + " ist fehlerhaft!\n");

                        if (newhashes.Calculated_Sale_Hash == serverInfo.Value.Hashes.Calculated_Sale_Hash)
                            File.Copy(Calculated_Sale_Path + "_1", Calculated_Sale_Path, true);
                        else
                            Helper.ErrorMessage(result += Calculated_Sale_Path + " ist fehlerhaft!");

                        if (newhashes.Parent_Child_Item_Hash == serverInfo.Value.Hashes.Parent_Child_Item_Hash)
                            File.Copy(Parent_Child_Item_Path + "_1", Parent_Child_Item_Path, true);
                        else
                            Helper.ErrorMessage(result += Parent_Child_Item_Path + " ist fehlerhaft!");

                        if (newhashes.SalesHeader_Hash == serverInfo.Value.Hashes.SalesHeader_Hash)
                            File.Copy(SalesHeader_Path + "_1", SalesHeader_Path, true);
                        else
                            Helper.ErrorMessage(result += SalesHeader_Path + " ist fehlerhaft!");
                    }
                    catch (Exception ex) { Helper.ErrorMessage(ex); }
                }
                else
                {
                   Helper.LogWrite(result = String.Format("Inkompatibel: {0} benötigt", serverInfo.Value.RequiredVersion));
                }
            }
            else
                Helper.ErrorMessage("In GetData(): Es konnte keine Verbindung zum Server hergestellt werden!");
            return result;
        }

        /// <summary>
        /// This method generates an xml file that contains the hashcode of each file so that the client can check the validity of the files
        /// </summary>
        /// <returns></returns>
        public static Hashes GetHashOfFiles()
        {
            MD5 md5 = MD5.Create();

            FileStream calculateditem_stream = File.OpenRead(Calculated_Item_Path + "_1");
            FileStream calculatedsale_stream = File.OpenRead(Calculated_Sale_Path + "_1");
            FileStream parentchilditem_stream = File.OpenRead(Parent_Child_Item_Path + "_1");
            FileStream salesheader_stream = File.OpenRead(SalesHeader_Path + "_1");

            byte[] calculateditem_hash = md5.ComputeHash(calculateditem_stream);
            byte[] calculatedsale_hash = md5.ComputeHash(calculatedsale_stream);
            byte[] parentchilditem_hash = md5.ComputeHash(parentchilditem_stream);
            byte[] salesheader_hash = md5.ComputeHash(salesheader_stream);

            StringBuilder calculateditem_hashstring = new StringBuilder();
            StringBuilder calculatedsale_hashstring = new StringBuilder();
            StringBuilder parentchilditem_hashstring = new StringBuilder();
            StringBuilder salesheader_hashstring = new StringBuilder();

            for (int i = 0; i < calculateditem_hash.Length; i++)
            {
                calculateditem_hashstring.Append(calculateditem_hash[i].ToString("x2"));
                calculatedsale_hashstring.Append(calculatedsale_hash[i].ToString("x2"));
                parentchilditem_hashstring.Append(parentchilditem_hash[i].ToString("x2"));
                salesheader_hashstring.Append(salesheader_hash[i].ToString("x2"));
            }
            string result = "";
            result = String.Format("Calculated_Item:{0};Calculated_Sale:{1};Parent_Child_Item:{2};SalesHeader:{3}", calculateditem_hashstring.ToString(), calculatedsale_hashstring.ToString(), parentchilditem_hashstring.ToString(), salesheader_hashstring.ToString());
            return new Hashes(result);
        }

        static void Main(string[] args)
        {
            GetData();
            while (true)
                Thread.Sleep(100);
        }
    }
}
