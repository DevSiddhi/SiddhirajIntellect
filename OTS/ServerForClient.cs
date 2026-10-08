using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace OTS
{
 
    //public delegate void ClientDataAvailableEventDelegate(SocketAsyncEventArgs client);  

    class ServerForClient
    {
        public Hashtable m_hashtable = new Hashtable();
        public bool AcceptConnections;        
        private int m_numConnections;   
        private int m_receiveBufferSize;
        BufferForOTS m_bufferManager;  
        const int opsToPreAlloc = 2;  //2 for receive and send
        public Socket listenSocket,Client;
        SocketAsyncEventArgs ConnectArgs;
        SocketAsyncEventArgsPool m_readWritePool;
        int m_totalBytesRead;          
        private int m_numConnectedSockets;
        public int LoginCnt, ConnectedCnt = 0, DisConnectedCnt = 0;
        Semaphore m_maxNumberAcceptedClients;

        public event EventHandler<SocketAsyncEventArgs> OnClientDataAvailable;       
        public event EventHandler ConnectionStatusChanged;
        Action ConnectionDisConnection;
        Form1 Mainfrm;

        public ServerForClient(int numConnections, int receiveBufferSize, Form1 frm)
        {
            m_totalBytesRead = 0;
            m_numConnectedSockets = 0;
            m_numConnections = numConnections;
            m_receiveBufferSize = receiveBufferSize;

            m_bufferManager = new BufferForOTS(receiveBufferSize * numConnections * opsToPreAlloc,
                receiveBufferSize);

            m_readWritePool = new SocketAsyncEventArgsPool(numConnections);
            m_maxNumberAcceptedClients = new Semaphore(numConnections, numConnections);
            Mainfrm = frm;
        }        

        public void Init()
        {
            m_bufferManager.InitBuffer();
           
            SocketAsyncEventArgs readWriteEventArg=null;

            for (int i = 0; i < m_numConnections; i++)
            {                
                readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                readWriteEventArg.UserToken = new TASClientConnectionToDataServer();
                m_bufferManager.SetBuffer(readWriteEventArg); //sets the socket buffer             
                m_readWritePool.Push(readWriteEventArg);               
            }
        }    

        public void Start(int port)
        {
            IPHostEntry localMachineInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPEndPoint myEndpoint = new IPEndPoint(IPAddress.Any, port);
            listenSocket = new Socket(myEndpoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(myEndpoint);            
            listenSocket.Listen(200);           
            StartAccept(null);         
        }

        public void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {            
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {               
                acceptEventArg.AcceptSocket = null;
            }

            m_maxNumberAcceptedClients.WaitOne();
            bool willRaiseEvent = listenSocket.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArg);
            }
        }
        
        void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {            
                TASClientConnectionToDataServer Client;
                string str;
                Interlocked.Increment(ref m_numConnectedSockets);
                Interlocked.Increment(ref LoginCnt);

                SocketAsyncEventArgs readEventArgs = m_readWritePool.Pop();
                Client = (TASClientConnectionToDataServer)readEventArgs.UserToken;
                //Client.e = e;
                Client.OTSClientSock = e.AcceptSocket;
                Client.OTSClientSock.Blocking = false;
                ConnectedCnt++;

                Client.HashEntryId = LoginCnt;
                Client.SockBuf = new byte[1024*1024];

                Client.SockCreateTime = DateTime.Now;
                Client.CanSendData = false;   //changed for quote handler and CTCL TCP requests.
                Client.LoginOK = false ;
                //Client.ITS_IP = ((IPEndPoint)e.AcceptSocket.RemoteEndPoint).Address.ToString();
                Client.ITS_IP = ((IPEndPoint)Client.OTSClientSock.RemoteEndPoint).Address.ToString();
                Mainfrm.listBox1.Items.Add( Client.ITS_IP+ " Connected at :"+ DateTime.Now.ToString("hh:mm:ss"));
                m_hashtable.Add(LoginCnt, Client); //every connected client entry is added in the hashtable                     
             
                bool willRaiseEvent = e.AcceptSocket.ReceiveAsync(readEventArgs);
                if (!willRaiseEvent)
                {
                    ProcessReceive(readEventArgs);
                }
         

            StartAccept(e); // starts accepting new connections
        }

        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            TASClientConnectionToDataServer token = (TASClientConnectionToDataServer)e.UserToken;         
           
            try
            {
                Mainfrm.Invoke(Mainfrm.ClientDataAvailable,e,token);
            }
            catch (Exception ex)
            {
                //OTS.ErrorLog(ex);                
            }

            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                Interlocked.Add(ref m_totalBytesRead, e.BytesTransferred); 
               
                ProcessSend(e);
               
            }
            else
            {
                CloseClientSocket(e);
            }
        }
       
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    TASClientConnectionToDataServer token = (TASClientConnectionToDataServer)e.UserToken;

                    bool willRaiseEvent = token.OTSClientSock.ReceiveAsync(e);
                    if (!willRaiseEvent)
                    {
                        ProcessReceive(e);
                    }
                }
                else
                {
                    CloseClientSocket(e);
                }
            }
            catch (Exception er)
            {
            }
        }

        public void CloseClientSocket(SocketAsyncEventArgs e)
        {         
            TASClientConnectionToDataServer Cli;

            Cli = e.UserToken as TASClientConnectionToDataServer;
            try
            {
                Array.Clear(Cli.SockBuf, 0, Cli.SockBuf.Length);
                Cli.OTSClientSock.Shutdown(SocketShutdown.Both);
                Cli.SockDestroyTime = DateTime.Now;
                DisConnectedCnt++;
                Mainfrm.listBox1.Items.Add(Cli.ITS_IP + " Disconnected at :" + DateTime.Now.ToString("hh:mm:ss"));
            }
                
            catch (Exception) { }
            Cli.OTSClientSock.Close();
            m_hashtable.Remove(Cli.HashEntryId);  // removes entry from hashtable on disconnection

            Interlocked.Decrement(ref m_numConnectedSockets);
            m_maxNumberAcceptedClients.Release();
            m_readWritePool.Push(e);
        }
    }
}
