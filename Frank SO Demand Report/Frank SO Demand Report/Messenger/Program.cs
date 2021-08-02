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
namespace Messenger
{
    class Program
    {
        static List<EndPoint> Endpoints = new List<EndPoint>();

        static void NotifyDataUpdate()
        {
            string message = "frank_dispo_tool_server_data_update:" + getLastDataUpdate().ToString();
            for (int i = 0; i < Endpoints.Count; i++)
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

        static void Main(string[] args)
        {
        }
    }
}
