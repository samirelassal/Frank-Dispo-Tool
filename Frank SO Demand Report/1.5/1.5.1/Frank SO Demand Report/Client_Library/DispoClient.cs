using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace Client_Library
{
    public struct ServerInfo 
    {
        public DateTime LastUpdated { get; private set; }
        internal ServerInfo(DateTime lastupdated) : this()
        {
            LastUpdated = lastupdated;
        }
    }

    public delegate void ServerUpdatedEventHandler();

    public class DispoClient
    {
        public static event ServerUpdatedEventHandler ServerUpdated;

        static readonly int timeout = 15000;

        static int port_broadcast = 15000;
        static int port_communication = 8081;
        public static string Server_IP { get; private set; }
        static TcpClient tcp;
        static IPEndPoint ipendpoint;

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
        //static FileStream Calculated_Item_fStream;
        //static FileStream Calculated_Sale_fStream;
        //static FileStream Parent_Child_Item_fStream;
        //static FileStream SalesHeader_fStream;
        //static int Calculated_Item_Size;
        //static int Calculated_Sale_Size;
        //static int Parent_Child_Item_Size;
        //static int SalesHeader_Size;

        private static bool SendBroadcast()
        {
            try
            {
                UdpClient udp = new UdpClient();
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
                    _serverinfo = new ServerInfo(DateTime.Parse(info_string_segments[0]));
                }
                catch (Exception ex) { ErrorMessage(ex); }
                finally { if (tcp.Connected) CloseConnection(tcp); }
            }
        }   
        public static void GetData()
        {
            RequestServerInformation();
            if (serverInfo != null)
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
                ErrorMessage("In GetData(): Es konnte keine Verbindung zum Server hergestellt werden!");
        }

        private static void WaitForServerToFinishUpdatingData(object _stream)
        {
            NetworkStream stream = (NetworkStream)_stream;
            byte[] data = new byte[1024];
            stream.Read(data, 0, data.Length);
            string answer = UTF8Encoding.UTF8.GetString(data);
            if (answer == "updating_data_done")
            {
                if (ServerUpdated != null)
                    ServerUpdated();
            }
        }

        static void Main(string[] args)
        {
            GetData();
        }
    }
}
