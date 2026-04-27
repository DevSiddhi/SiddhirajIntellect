using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;

namespace OTS
{
    class MCastSocketClass
    {
        string IP = null;
        public int port;        
        public IPEndPoint remoteep,localep;      
       
        Form1 MainFrm;
        Action OTSMcastReceived;
        public UdpClient Receiver;
        public UdpClient Sender;
        public Buffer MulticastSocBuff;
        public UdpState udpState = new UdpState();
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(System.Net.IPAddress.Any, 0);


        public MCastSocketClass(string Socket_IP, string Socket_port, Form1 frm)
        {
            IP = Socket_IP;
            port = Convert.ToInt32(Socket_port);           
            MainFrm = frm;
            MulticastSocBuff= new Buffer();
        }

        public void Join()
        {
            try
            {
                IPAddress ip1 = IPAddress.Any;
                localep = new IPEndPoint(ip1, port);

                Receiver = new UdpClient();
                Receiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                Receiver.Client.Bind(localep);

                Sender = new UdpClient();
                Sender.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                Sender.Client.Bind(localep);
             
                IPAddress ip = IPAddress.Parse(IP);
                remoteep = new IPEndPoint(ip, port);


                Sender.JoinMulticastGroup(ip);
                Sender.EnableBroadcast = true;
                Sender.MulticastLoopback = true;

                Receiver.JoinMulticastGroup(ip);
                Receiver.EnableBroadcast = true;
                Receiver.MulticastLoopback = true;
              
                udpState.ipEndpt = RemoteIpEndPoint;
                udpState.udpClient = Receiver;
                BeginReceive();
                        
            }
            catch (Exception e)
            {
            }
        }

        public void BeginReceive()
        {
            Receiver.BeginReceive(new AsyncCallback(GetMsg), udpState);    
        }
        public void GetMsg(IAsyncResult ar)
        {
            
            try
            {              
                UdpClient udpClient = (UdpClient)((UdpState)(ar.AsyncState)).udpClient;
                IPEndPoint ipEndpt = (IPEndPoint)((UdpState)(ar.AsyncState)).ipEndpt;
                byte[] RecByte = Receiver.EndReceive(ar, ref ipEndpt);               
                OTSMcastReceived = () => MainFrm.ipwMCast1DataIn(RecByte);
                MainFrm.Invoke(OTSMcastReceived);
                Array.Clear(RecByte, 0, RecByte.Length);              
               
            }
            catch (Exception e)
            {
                MainFrm.listBox1.Items.Insert(0, "UDPSocket: " + e.ToString());
            }
        }
    }

    class UdpState
    {
        public UdpClient udpClient;
        public IPEndPoint ipEndpt;
    }
}
