using ConsoleApp1.Helpers;
using ConsoleApp1.Models;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static ConsoleApp1.Helpers.CommonFunctions;
using static ConsoleApp1.Helpers.SQLiteHelper;

namespace ConsoleApp1.QH
{
    public static class MainQH
    {
        public static string protoFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "binaryMastersFiles");
        public static string MasterFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Masters");
        public static string mfFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mfFiles");
        public static DateTime LastTradingDate { get; set; }

        public static BrokerNameRecords BrokerCodeQH;
        static int exchcount = 9, maxthread = 15;
        public static ExchdataForQH[] ExchData = new ExchdataForQH[exchcount];
        public static readonly DateTime ExchStartDate = new DateTime(1980, 1, 1);
        public static string[] OFISType = new string[] { "", "FUTIDX", "OPTIDX", "FUTSTK", "OPTSTK", "FUTIVX", "FUTCUR", "OPTCUR", "UNDCUR", "FUTCOM", "OPTFUT", "FUTIRC", "FUTIRT",
            "COMDTY", "OPTCOM" }, OFISTypeBSEFO = new string[] { "", "IF", "IO", "SF", "SO" },
            CPType = new string[5] { "XX", "CA", "PA", "CE", "PE" }, BSEBhavCopyCPType = new string[5] { "", "CA", "PA", "CALL", "PUT" }, SeriesArray = new string[] { "BE", "EQ", "DR", "DE", "BZ", "SM", "IV", "ID",
                "E1", "ST", "BL", "BT", "D1", "D2", "D3", "DR", "E2", "E3", "E4", "E5", "GC", "IL", "K1", "M1", "MF", "N1", "N2", "N3", "N4", "N5", "N6", "N7", "N8",
                "N9", "NA", "NB", "NC", "ND", "NE", "NF", "NG", "NH", "NI", "NJ", "NK", "NL", "NM", "NN", "NO", "NP", "NQ", "NR", "NS", "NT", "NU", "O1", "O2", "O3",
                "O4", "O5", "O9", "P1", "P2", "P3", "P4", "Q1", "Q2", "RL", "NO", "NP", "NQ", "NR", "NS", "NT", "NU" };


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

        public static void StartUpQH()
        {
            try
            {
                CommonFunctions.WriteToLogFile("Starting up QH.....");
                Console.WriteLine("Starting up QH.....");
                string basePath = AppDomain.CurrentDomain.BaseDirectory;

                if(!Directory.Exists(mfFolder))
                {
                    Directory.CreateDirectory(mfFolder);
                }

                string masterFile = Path.Combine(mfFolder, "Masters.mf");
                string masterServerFile = Path.Combine(mfFolder, "MastersServer.mf");

                if (File.Exists(masterFile))
                    File.Delete(masterFile);

                if (File.Exists(masterServerFile))
                    File.Delete(masterServerFile);

                Console.WriteLine("Old master files deleted.");

                //Creation of ProtoFolder
                if (!Directory.Exists(protoFolder))
                {
                    Directory.CreateDirectory(protoFolder);
                }

                LastTradingDate = getLastTradingDate();  //User will enter last trading date here. 
                if (LastTradingDate == DateTime.MinValue)
                {
                    throw new InvalidOperationException("Unable to determine last trading date from Holiday.csv");
                }

                Console.WriteLine($"Last Trading date is : {LastTradingDate:dd-MM-yyyy}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                CommonFunctions.ErrorLog(ex);
            }
        }

        public static DateTime getLastTradingDate()
        {
            while (true)
            {
                Console.Write("Enter Previous Trading Date (dd-MM-yyyy): ");
                string input = Console.ReadLine();

                if (DateTime.TryParseExact(input, "dd-MM-yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime date))
                {
                    return date;
                }

                Console.WriteLine("Invalid date. Please enter date in dd-MM-yyyy format.");
            }

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
