using ConsoleApp1.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static ConsoleApp1.Helpers.CommonFunctions;
using static ConsoleApp1.Helpers.SQLiteHelper;

namespace ConsoleApp1.QH
{
    public static class MainQH
    {
        public static BrokerNameRecords BrokerCodeQH;
        static int exchcount = 9, maxthread = 15;
        public static ExchdataForQH[] ExchData = new ExchdataForQH[exchcount];


        public static T ReadUsingMarshalUnsafe<T>(byte[] data) where T : struct
        {
            unsafe
            {
                fixed (byte* p = &data[0])
                {
                    return (T)Marshal.PtrToStructure(new IntPtr(p), typeof(T));
                }
            }
        }
        public static byte[] RawSerialize(object anything) //Convert Structure To Byte
        {
            int rawsize = Marshal.SizeOf(anything);
            IntPtr buffer = Marshal.AllocHGlobal(rawsize);
            Marshal.StructureToPtr(anything, buffer, false);
            byte[] rawdatas = new byte[rawsize];
            Marshal.Copy(buffer, rawdatas, 0, rawsize);
            Marshal.FreeHGlobal(buffer);
            return rawdatas;
        }
        public enum BrokerNameRecords //10.07.2015
        {
            BROKER_VENTURA = 0, BROKER_IN_NIVESH = 1, BROKER_KIFS = 2, BROKER_LKP = 3, BROKER_MONARCH_NETWORTH = 4,
            BROKER_PINNACLE = 5, BROKER_SUNLIGHT = 6, BROKER_SWASTIKA = 7, BROKER_PURE = 8, BROKER_NIRMAN = 9,
            BROKER_SHAH_INVESTORS = 10, BROKER_FAIRWEALTH = 11, BROKER_INDIRA = 12, BROKER_ASTHA = 13, BROKER_EMKAY = 14,
            BROKER_LKPTRADESMART = 15, BROKER_AMBALAL = 16, BROKER_INDOTHAI = 17, BROKER_LATINMANHARLAL = 18,
            BROKER_PUMARTH = 19, BROKER_F6 = 20, BROKER_LAXMISHREE = 21, BROKER_MANGALKESHAV = 22, BROKER_ARANOX = 23,
            BROKER_PlusWealth = 24, BROKER_MSEFSL = 25, BROKER_TradeJini = 26, BROKER_Ajmera = 27, BROKER_Intellect = 28 /*BROKER_Marfatia = 28*/
        };
    }
}
