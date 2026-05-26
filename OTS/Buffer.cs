using System;

namespace OTS
{
    public class Buffer
    {
        byte[] buffer,Junk;
        public int FBufSize, WrCount, RdCount, nsize=1*1024*1024;
            
        public int BufSize
        {
            get
            {
                return FBufSize;
            }
            set
            {
                FBufSize = value;
                buffer = new byte[FBufSize];
            }
        }
            
        public Buffer()
        {
            WrCount = 0;
            RdCount = 0;
            BufSize = nsize;
        }

        public void write(byte[] Data, int Length)
        {
            int Remaining;
            int Copied;
            Remaining = FBufSize - WrCount;
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
                Array.Copy(Data, 0, buffer, WrCount, Copied);
                WrCount = WrCount + Copied;
                return;
            }
        }
        
        public void Read( byte[] data, int length)
        {
           
                int Remaining;
                int Copied;
                Junk = data;
                Remaining = WrCount - RdCount;
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
                    Array.Copy(buffer, RdCount, Junk, 0, Copied);
                    RdCount = RdCount + Copied;

                    if (RdCount == WrCount)
                    {
                        RdCount = 0;
                        WrCount = 0;
                    }
                    return;
                }         
           
        }

        public void peekUpto(byte[] data, int length) //Indicate End of file
        {
            int Remaining;
            int Copied;
            Remaining = WrCount - RdCount;
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
                Array.Copy(buffer, RdCount, data, 0, Copied);
                return;
            }
        }

        public int GetBuffUsed()
        {
            return (WrCount - RdCount);
        }

        public void ClearBuff()
        {
            Array.Clear(buffer, 0, nsize);
        }
    }
}
