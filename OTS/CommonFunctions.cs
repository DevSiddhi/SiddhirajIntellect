using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using ICSharpCode.SharpZipLib.BZip2;

namespace OTS
{
    class CommonFunctions
    {
        #region Binary Search
        public static int BinSearchClient(ref  ITInteractiveStructures.TClientDbNumReportReplyRecord[] x, string searchValue)
        {
            //int Result=1;
            //for (int i = 0; i < x.Length; i++)
            //{
            //    if (new string(x[i].ClientCode) != searchValue) continue;
            //    Result = x[i].DBNum;
            //    break;
            //}
            //return Result;
            int First, Last, Pivot, Results;
            bool Found;

            First = 0;
            Last = x.Length - 1;
            Found = false;
            Results = 1;   //Initializes the Result to 1 changed on 26.06.2012 as if the user enters wrong ID it should by default return 1


            while (First <= Last && !Found)
            {
                Pivot = (First + Last) / 2;
                if (new string(x[Pivot].ClientCode).CompareTo(searchValue) == 0)
                {
                    Found = true;
                    Results = x[Pivot].DBNum;
                }
                else if (new string(x[Pivot].ClientCode).CompareTo(searchValue) > 0)
                {
                    Last = Pivot - 1;
                }
                else
                {
                    First = Pivot + 1;
                }
            }
            return Results;
        }

        #endregion      

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

        public static byte[] RawSerialize(object anything)
        {
            int rawsize = Marshal.SizeOf(anything);
            IntPtr buffer = Marshal.AllocHGlobal(rawsize);
            Marshal.StructureToPtr(anything, buffer, false);
            byte[] rawdatas = new byte[rawsize];
            Marshal.Copy(buffer, rawdatas, 0, rawsize);
            Marshal.FreeHGlobal(buffer);
            return rawdatas;
        }

        public static byte[] RawSerialize(object anything, out int rawsize)
        {
            rawsize = Marshal.SizeOf(anything);
            IntPtr buffer = Marshal.AllocHGlobal(rawsize);
            Marshal.StructureToPtr(anything, buffer, false);
            byte[] rawdatas = new byte[rawsize];
            Marshal.Copy(buffer, rawdatas, 0, rawsize);
            Marshal.FreeHGlobal(buffer);
            return rawdatas;
        }

        string[] SplitCSV(string s, char Delim)
        {
            List<string> list = new List<string>();
            StringBuilder work = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '"')
                {
                    int p2;
                    while (true)
                    {
                        p2 = s.IndexOf('"', i + 1);
                        work.Append(s.Substring(i + 1, p2 - i - 1));
                        i = p2;

                        // If this is a double quote, keep going!
                        if (((p2 + 1) < s.Length) && (s[p2 + 1] == '"'))
                        {
                            work.Append('"');
                            i++;

                            // otherwise, this is a single quote, we're done
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else if (c == Delim)
                {
                    list.Add(work.ToString());
                    work.Length = 0;
                }
                else
                {
                    work.Append(c);
                }
            }
            list.Add(work.ToString());
            return list.ToArray();
        }

        public static int GetSelectedRadioButtonText(GroupBox grb)
        {
            return grb.Controls.OfType<RadioButton>().SingleOrDefault(rad => rad.Checked == true).TabIndex;
        }
        
    }

    public static class TCustomWSocketLogger
    {
        public static List<Tuple<int, FileStream>> OTS_REP_DatFileStream = new List<Tuple<int, FileStream>>();
        public static bool isLogEnabled = false;

        public static FileStream getFileStream(int ITS_Num)
        {
            for (int i = 0; i < OTS_REP_DatFileStream.Count; i++)
                if (OTS_REP_DatFileStream[i].Item1 == ITS_Num)
                    return OTS_REP_DatFileStream[i].Item2;
            FileStream fs = new FileStream($"OTS_RawData_{DateTime.Now:ddMMyy-HHmmss}_{ITS_Num}.dat", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            OTS_REP_DatFileStream.Add(new Tuple<int, FileStream>(ITS_Num, fs));
            return fs;
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct TGlobalHeader
        {
            public DateTime DateTime;
            public int Size;
            public byte SentFrom;
        }

        public static void logData(byte[] Data, int Len, int ITS_Num, byte type)
        {
            if (isLogEnabled)
            {
                var fs = getFileStream(ITS_Num);
                TGlobalHeader hdr = new TGlobalHeader();
                hdr.DateTime = DateTime.Now;
                hdr.Size = Len;
                hdr.SentFrom = type;
                fs.Write(CommonFunctions.RawSerialize(hdr, out int size), 0, size);
                fs.Write(Data, 0, Len);
            }
        }
        public static void flushAllandClose()
        {

            foreach (var item in OTS_REP_DatFileStream)
            {
                item.Item2.Flush();
                item.Item2.Close();
            }
        }

    }

    class Constants
    {
        public const int OTS_ORDERTYPE_PLACE_REQ = 17;
        public const int OTS_ORDERTYPE_PLACE_REQ_COVER = 171;    // place cover   // sumitp 28_jan_22
        public const int OTS_ORDERTYPE_PLACE_REPLY = 27;
        public const int OTS_ORDERTYPE_PLACE_REPLY_COVER = 30;     // reply cover same like 27 sumit p    // sumitp 28_jan_22
        public const int OTS_ORDERTYPE_MODIFY_REQ = 18;
        public const int OTS_ORDERTYPE_MODIFY_REPLY = 28;
        public const int OTS_ORDERTYPE_CANCEL_REQ = 19;
        public const int OTS_ORDERTYPE_CANCEL_REPLY = 29;
        public const int OTS_ORDERTYPE_CONVERSION = 26;
        public const int OTS_ORDERTYPE_DERIVCONVERSION = 25;
        public const int OTS_ORDERTYPE_TRADES_REPORTS_REC = 71;
        public const int OTS_ORDERTYPE_ORDER_REPORTS_REC = 72;
        public const int OTS_ORDERTYPE_ACTIVITY_LOG_REC = 104;//74;
        public const int OTS_ORDERTYPE_NEW_ACTIVITY_LOG_REC_NEW = 204; // 15.10.16 Hetal
        public const int OTS_ORDERTYPE_NEW_ACTIVITY_LOG_REC_NEW_new = 202; // Sumit 21_jan_22 For Single Activity Log

        public const int OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW = 61; //Added on 18-Apr-2018 - Rohan
        public const int OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW = 62;      //Added on 18-Apr-2018 - Rohan
        public const int OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_COVER_BRACKET = 78; // Order Report for Cover orders   // sumitp 28_jan_22
        public const int OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW_COVER = 58;    // Trade Report for Cover orders // sumitp 28_jan_22
        public const int OTS_ORDERTYPE_ACTIVITY_LOG_REC_NEW = 64;   //Added on 18-Apr-2018 - Rohan

        public const int OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_WITH_REJOR = 52;

        public const int OTS_SECURITY_INFO = 42;
        public const int OTS_SCRIP_MARGIN = 49;
        public const int OTS_HOLDING_VALUATION_REPORT_NEW = 170;
        public const int OTS_HOLDING_REPORT = 94;//Mcx
        public const int OTS_HOLDING_REPORT_PreClose = 106;
        public const int OTS_HOLDING_REPORT_PreClose_FS = 112;
        public const int OTS_ALLCLIENTS_HOLDING_REPORT = 95;
        public const int OTS_ALLCLIENTS_HOLDING_REPORT_New = 105; 
        public const int OTS_VERIFY_USER = 99; //99 for User Verification
        //15.10.16 Hetal
        public const int OTS_ORDERTYPE_DERIV_PLACE_REPLY = 111;
        public const int OTS_ORDERTYPE_DERIV_MODIFY_REPLY = 121;
        public const int OTS_ORDERTYPE_DERIV_CANCEL_REPLY = 131;
        public const int OTS_ORDERTYPE_CURR_PLACE_REPLY = 212;
        public const int OTS_ORDERTYPE_CURR_MODIFY_REPLY = 222;
        public const int OTS_ORDERTYPE_CURR_CANCEL_REPLY = 232;

        //EXECRISE REQUEST 02.08.17
        public const int OTS_EXERCISE_PLACE_REQ = 181;
        public const int OTS_EXERCISE_MODIFY_REQ = 182;
        public const int OTS_EXERCISE_CANCEL_REQ = 183;

        public const int OTS_EXERCISE_PLACE_REPLY = 184;
        public const int OTS_EXERCISE_MODIFY_REPLY = 185;
        public const int OTS_EXERCISE_CANCEL_REPLY = 186;
        public const int OTS_ORDERTYPE_EXERCISE_REPORTS_REC = 63;

        //Bsemf Krutika
        public const int OTS_ORDERTYPE_MF_PLACE_REQ = 43;
        public const int OTS_ORDERTYPE_MF_MODIFY_REQ = 44;
        public const int OTS_ORDERTYPE_MF_CANCEL_REQ = 45;
        public const int OTS_ORDERTYPE_MF_PLACE_EXCH_REPLY = 46;
        public const int OTS_ORDERTYPE_MF_MODIFY_EXCH_REPLY = 47;
        public const int OTS_ORDERTYPE_MF_CANCEL_EXCH_REPLY = 48;
        public const int OTS_ORDERTYPE_MF_ORDER_REPORTS_REC = 93;
        public const int OTS_Acitivitylog_MF_ORDER_REPORTS_REC = 100;
        public const int OTS_ORDERTYPE_MF_SIPREGISTRATION_REQ = 34;
        public const int OTS_ORDERTYPE_MF_XSIPREGISTRATION_REQ = 39;
        public const int OTS_ORDERTYPE_MF_SIPREGISTRATION_EXCH_REPLY = 36; 
        public const int OTS_ORDERTYPE_MF_SIPCANCEL_REQ = 35;
        public const int OTS_ORDERTYPE_MF_SIPCANCEL_EXCH_REPLY = 37;
        public const int OTS_ORDERTYPE_MF_SIP_ORDER_REPORTS_REC = 38;
        public const int OTS_ORDERTYPE_MF_MANDATE_ORDER_REPORTS_REC = 189;
        public const int OTS_MANDATE = 187;
        public const int OTS_MANDATE_EXCH_REPLY = 188;

        public const int OTS_ORDERTYPE_ORDER_REPORTS_REC_NEW_141 = 141;  //Added on 5-Apr-2018 - Rohan
        public const int OTS_ORDERTYPE_TRADES_REPORTS_REC_NEW_142 = 142; //Added on 5-Apr-2018 - Rohan
        #region OFS
        public const int OTS_OFS_WATCHLIST = 201;
        public const int OTS_OFS_REPORTS = 203;
        public const int OTS_OFS_PLACE_BROKER_REQ = 122;
        public const int OTS_OFS_MODIFY_BROKER_REQ = 123;
        public const int OTS_OFS_CANCEL_BROKER_REQ = 124;
        #endregion
    }

    public static class IntellectExtensions
    {
        public static byte[] BZipCompress(this byte[] Data, int Length)
        {
            MemoryStream Ms1 = new MemoryStream(Data);
            MemoryStream Ms2 = new MemoryStream();
            BZip2.Compress(Ms1, Ms2, (int)Ms1.Length);
            byte[] CompData = Ms2.ToArray();
            return CompData;
        }

        public static T ReadStruct<T>(this FileStream fs)
        {
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
            fs.Read(buffer, 0, Marshal.SizeOf(typeof(T)));
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            T temp = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return temp;
        }

        public static T ToStructure<T>(this byte[] data, int offset = 0) where T : struct
        {
            unsafe
            {
                fixed (byte* p = &data[offset])
                {
                    return (T)Marshal.PtrToStructure(new IntPtr(p), typeof(T));
                }
            }
        }

    }
}
