using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace OTS
{
    public class TASClientConnectionToDataServer
    {
        int WriteCount, ReadCount;
        public byte[] SockBuf, Junk; 
        public Socket OTSClientSock; //Client Socket
        public List<byte> sendBuffer = new List<byte>();
 
        //public SocketAsyncEventArgs e;   
   
        public byte[] SendBuffer
        {
            get { return sendBuffer.ToArray(); }
           
        }   

        public DateTime SockCreateTime,SockDestroyTime;        
     
      
       

        internal void PutDataInSendBuffer(byte[] p)
        {
            sendBuffer.AddRange(p);
        }

        public void Write(byte[] Data, int Length)
        {
            int Remaining;
            int Copied;
            Remaining = SockBuf.Length - WriteCount;
            if (Remaining <= 0)
            {
                return;
            }
            else
            {
                if (Length < Remaining)
                    Copied = Length;
                else
                {
                    Copied = Remaining;
                }

                Array.Copy(Data, 0, SockBuf, WriteCount, Copied);
                WriteCount = WriteCount + Copied;
                return;
            }
        }

        public void Read(byte[] data, int length)
        {
            int Remaining;
            int Copied;
            Junk = data;
            Remaining = WriteCount - ReadCount;
            if (Remaining <= 0)
            {
                return;
            }
            else
            {
                if (length < Remaining)
                {
                    Copied = length;
                }
                else
                {
                    Copied = Remaining;
                }
                Array.Copy(SockBuf, ReadCount, Junk, 0, Copied);
                ReadCount = ReadCount + Copied;

                if (ReadCount == WriteCount)
                {
                    ReadCount = 0;
                    WriteCount = 0;
                }
                return;
            }
        }

        internal void ClearSendBuffer()
        {
            sendBuffer.RemoveRange(0, SendBuffer.Length);
        }

        public void peekUpto(byte[] data, int length)
        {
            int Remaining;
            int Copied;
            Remaining = WriteCount - ReadCount;
            if (Remaining <= 0)
            {
                return;
            }
            else
            {
                if (length < Remaining)
                {
                    Copied = length;
                }
                else
                {
                    Copied = Remaining;
                }
                Array.Copy(SockBuf, ReadCount, data, 0, Copied);
                return;
            }
        }

        public int GetBuffUsed()
        {
            return (WriteCount - ReadCount);
        }
    }
}
