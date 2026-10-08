using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Windows.Forms;

namespace OTS
{
    /// <summary>
    /// Implements the connection logic for the socket client.
    /// </summary>
    internal sealed class SocketClient : IDisposable
    {
        Form1 Mainfrm;
        BufferForOTS m_bufferManager;
        Action ExchConnDataAvailable;
        Action ConnectionStatusChangedDelegate;
        public event EventHandler ConnectionDisconnection;
        public int ECNo,BuffUsedBuff;
        public Buffer SockBufExchConn;
        public IPAddress address;
        public int port;

      
        /// <summary>
        /// Constants for socket operations.
        /// </summary>
        private const Int32 ReceiveOperation = 1, SendOperation = 0;

        /// <summary>
        /// The socket used to send/receive messages.
        /// </summary>
        public Socket clientSocket;

        /// <summary>
        /// Flag for connected socket.
        /// </summary>
        private Boolean connected = false;

        /// <summary>
        /// Listener endpoint.
        /// </summary>
        private IPEndPoint hostEndPoint;

        /// <summary>
        /// Signals a connection.
        /// </summary>
        private static AutoResetEvent autoConnectEvent = new AutoResetEvent(false); 

        /// <summary>
        /// Signals the send/receive operation.
        /// </summary>
        private static AutoResetEvent[] autoSendReceiveEvents = new AutoResetEvent[]
        {
            new AutoResetEvent(false),
            new AutoResetEvent(false)
        };
        public bool Connected
        {
            get
            {
                if (this == null)
                    return false;
                else if (this.clientSocket == null)
                    return false;
                else
                    return clientSocket.Connected;
            }
            
        }

        /// <summary>
        /// Create an uninitialized client instance.  
        /// To start the send/receive processing
        /// call the Connect method followed by SendReceive method.
        /// </summary>
        /// <param name="hostName">Name of the host where the listener is running.</param>
        /// <param name="port">Number of the TCP port from the listener.</param>
        /// 
        internal SocketClient()
        {
        }

        internal SocketClient(int Ecno, Form1 frm)
        {
            m_bufferManager = new BufferForOTS(1024, 1024);
            // Instantiates the endpoint and socket.               
            SockBufExchConn = new Buffer();
            Mainfrm = frm;
            ECNo = Ecno;
        }
        /// <summary>
        /// Connect to the host.
        /// </summary>
        /// <returns>True if connection has succeded, else false.</returns>
       
        internal void Connect()
        {
                SocketAsyncEventArgs connectArgs = new SocketAsyncEventArgs();

                this.hostEndPoint = new IPEndPoint(address, port);
                this.clientSocket = new Socket(this.hostEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                //connectArgs.UserToken = this.clientSocket;
                connectArgs.UserToken = new TASClientConnectionToDataServer();
                connectArgs.RemoteEndPoint = this.hostEndPoint;
                connectArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnConnect);
                bool willRaiseEvent = clientSocket.ConnectAsync(connectArgs);
                if (!willRaiseEvent)
                {
                    Connect();
                }

                //autoConnectEvent.WaitOne();

                SocketError errorCode = connectArgs.SocketError;
                if (errorCode != SocketError.Success)
                {
                    throw new SocketException((Int32)errorCode);
                }
                    
        }

        /// <summary>
        /// Disconnect from the host.
        /// </summary>
        
        public void Disconnect()
        {
            clientSocket.Disconnect(true);
        }

        private void OnConnect(object sender, SocketAsyncEventArgs e)
        {          
            // Signals the end of connection.
            autoConnectEvent.Set();
            // Set the flag for socket connected.
            this.connected = (e.SocketError == SocketError.Success);
            ConnectionStatusChangedDelegate = () => Mainfrm.ExchConnectorConnectionStatusChanged(ECNo);
            Mainfrm.Invoke(ConnectionStatusChangedDelegate);
            startReceiving();
        }

        public void startReceiving()
        {
            SocketAsyncEventArgs ReceiveArgs = new SocketAsyncEventArgs();
            TASClientConnectionToDataServer Client;
            if (this.clientSocket.Connected)
            {
                m_bufferManager.InitBuffer();
                ReceiveArgs.RemoteEndPoint = hostEndPoint;
                ReceiveArgs.UserToken = new TASClientConnectionToDataServer();           
                m_bufferManager.SetBuffer(ReceiveArgs);
                ReceiveArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                Client = (TASClientConnectionToDataServer)ReceiveArgs.UserToken;
                Client.SockBuf = new byte[1024 * 10];               
                try
                {
                    bool willRaiseEvent = clientSocket.ReceiveAsync(ReceiveArgs);
                    if (!willRaiseEvent)
                    {
                        ProcessReceive(ReceiveArgs);
                    }
                }
                catch (Exception er)
                {
                }
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
         {
             TASClientConnectionToDataServer token = (TASClientConnectionToDataServer)e.UserToken;
             if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
             {
                 try
                 {
                     ExchConnDataAvailable = () => Mainfrm.ProcessDataAvailable(ECNo, e);
                     Mainfrm.Invoke(ExchConnDataAvailable);

                 }
                 catch (Exception ex)
                 {
                     //OTS.ErrorLog(ex);                
                 }

                 ProcessSend(e);

             }
             else
             {
                 Dispose();
             }
        }
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                TASClientConnectionToDataServer token = (TASClientConnectionToDataServer)e.UserToken;

                bool willRaiseEvent = clientSocket.ReceiveAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessReceive(e);
                }
            }
            else
            {
                Dispose();
            }
        }

        private void OnReceive(object sender, SocketAsyncEventArgs e)
        {
            // Signals the end of receive.
            autoSendReceiveEvents[SendOperation].Set();
        }

        private void OnSend(object sender, SocketAsyncEventArgs e)
        {
            // Signals the end of send.
            autoSendReceiveEvents[ReceiveOperation].Set();

            if (e.SocketError == SocketError.Success)
            {
                if (e.LastOperation == SocketAsyncOperation.Send)
                {
                    // Prepare receiving.
                    Socket s = e.UserToken as Socket;

                    byte[] receiveBuffer = new byte[255];
                    e.SetBuffer(receiveBuffer, 0, receiveBuffer.Length);
                    e.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceive);
                    s.ReceiveAsync(e);
                }
            }
            else
            {
                this.ProcessError(e);
            }
        }

        /// <summary>
        /// Close socket in case of failure and throws a SockeException according to the SocketError.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the failed operation.</param>
        /// 

        private void ProcessError(SocketAsyncEventArgs e)
        {
            Socket s = e.UserToken as Socket;
            if (s.Connected)
            {
                // close the socket associated with the client
                try
                {
                    s.Shutdown(SocketShutdown.Both);
                }
                catch (Exception)
                {
                    // throws if client process has already closed
                }
                finally
                {
                    if (s.Connected)
                    {
                        s.Close();
                    }
                }
            }

            // Throw the SocketException
            throw new SocketException((Int32)e.SocketError);
        }

        /// <summary>
        /// Exchange a message with the host.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <returns>Message sent by the host.</returns>
        /// 

        internal String SendReceive(String message)
        {
            if (this.connected)
            {
                // Create a buffer to send.
                Byte[] sendBuffer = Encoding.ASCII.GetBytes(message);

                // Prepare arguments for send/receive operation.
                SocketAsyncEventArgs completeArgs = new SocketAsyncEventArgs();
                completeArgs.SetBuffer(sendBuffer, 0, sendBuffer.Length);
                completeArgs.UserToken = this.clientSocket;
                completeArgs.RemoteEndPoint = this.hostEndPoint;
                completeArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSend);

                // Start sending asyncronally.
                clientSocket.SendAsync(completeArgs);

                // Wait for the send/receive completed.
                AutoResetEvent.WaitAll(autoSendReceiveEvents);

                // Return data from SocketAsyncEventArgs buffer.
                return Encoding.ASCII.GetString(completeArgs.Buffer, completeArgs.Offset, completeArgs.BytesTransferred);
            }
            else
            {
                throw new SocketException((Int32)SocketError.NotConnected);
            }
        }

        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessReceive(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Disposes the instance of SocketClient.
        /// </summary>

        public void Dispose()
        {      
            autoConnectEvent.Reset();
            autoSendReceiveEvents[SendOperation].Reset();
            autoSendReceiveEvents[ReceiveOperation].Reset();

            Close();
                Mainfrm.Invoke(ConnectionStatusChangedDelegate);
           
          
        }
        public void Close()
        {
            if (this.clientSocket.Connected)
            {
                this.clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Disconnect(true);
                this.SockBufExchConn.RdCount = 0;
                this.SockBufExchConn.WrCount = 0;
            }
        }

        #endregion
    }
}
