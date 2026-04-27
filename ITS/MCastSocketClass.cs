using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;

namespace ITS
{
    class MCastSocketClass
    {
        string IP = null;
        public int port;
        public IPEndPoint remoteep, localep;

        Action<byte []> ECMcastReceived;
        public static UdpClient Receiver;
        public UdpClient Sender;
        public Control Invoker;
        public UdpState udpState = new UdpState();
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(System.Net.IPAddress.Any, 0);


        public MCastSocketClass(string Socket_IP, string Socket_port, Action<byte []> action, Control Invoker)
        {
            IP = Socket_IP;
            port = Convert.ToInt32(Socket_port);
            ECMcastReceived = action;
            this.Invoker = Invoker;
            Join();
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

                IPAddress ip = IPAddress.Parse(IP);
                remoteep = new IPEndPoint(ip, port);

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
                Invoker.Invoke(new Action(() => ECMcastReceived(RecByte)));
                Array.Clear(RecByte, 0, RecByte.Length);
                BeginReceive();
            }
            catch (Exception e)
            {
            }
        }
    }

    class UdpState
    {
        public UdpClient udpClient;
        public IPEndPoint ipEndpt;
    }
}
