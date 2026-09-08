using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace JIO_QH.Helpers
{
    public static class CommonFunctions
    {
        public static string Folder = AppDomain.CurrentDomain.BaseDirectory + "Reports\\";
        public static string LogFileName = Folder + "QH_Log_" + DateTime.Now.ToString("ddMMyy") + ".txt";
        public static readonly string LogFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        public static string MasterCSV = AppDomain.CurrentDomain.BaseDirectory + "\\Master_CSV_Files\\";  

        public static void WriteToLogFile(string strOut)
        {
            if (strOut == "")
            {
                if (!Directory.Exists(Folder))
                {
                    Directory.CreateDirectory(Folder);
                    File.AppendAllText(LogFileName, "\r\n");
                }
                else
                {
                    File.AppendAllText(LogFileName, "\r\n");
                }
            }
            else
            {
                if (!Directory.Exists(Folder))
                {
                    Directory.CreateDirectory(Folder);
                    File.AppendAllText(LogFileName, System.DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt") + "       " + strOut + "\r\n");
                }
                else
                {
                    File.AppendAllText(LogFileName, System.DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt") + "       " + strOut + "\r\n");
                }
            }

        }
        public static void ErrorLog(Exception e)
        {
            try
            {
                if (!Directory.Exists(LogFolder))
                {
                    Directory.CreateDirectory(LogFolder);
                }
                string fileName =
                    "QHErrorLog_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                string fullPath = Path.Combine(LogFolder, fileName);
                string errorMessage =
                    "\r\n==================================================\r\n" +
                    "Date Time : " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt") + "\r\n" +
                    "Message   : " + e.Message + "\r\n" +
                    "Source    : " + e.Source + "\r\n" +
                    "StackTrace: " + e.StackTrace + "\r\n" +
                    "==================================================\r\n";
                File.AppendAllText(fullPath, errorMessage);
            }
            catch
            {
            }
        }
        public static void ExportToCsv(string tableName, string csvFilePath)
        {
            using (SQLiteCommand sqLiteCommand = new SQLiteCommand($"SELECT * FROM {tableName}", SQLiteHelper.Master_dbConnection))
            {
                if (SQLiteHelper.Master_dbConnection.State == ConnectionState.Closed)
                {
                    ((DbConnection)SQLiteHelper.Master_dbConnection).Open();
                }
                    ((DbCommand)sqLiteCommand).ExecuteNonQuery();


                using (SQLiteDataReader reader = sqLiteCommand.ExecuteReader())
                {
                    if (!Directory.Exists(MasterCSV))
                    {
                        Directory.CreateDirectory(MasterCSV);
                    }
                    using (StreamWriter writer = new StreamWriter(MasterCSV + csvFilePath))
                    {
                        // Write header
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            writer.Write(reader.GetName(i));
                            if (i < reader.FieldCount - 1)
                                writer.Write(",");
                        }
                        writer.WriteLine();

                        // Write data
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                writer.Write(reader[i].ToString());
                                if (i < reader.FieldCount - 1)
                                    writer.Write(",");
                            }
                            writer.WriteLine();
                        }
                    }
                }
                    ((DbConnection)SQLiteHelper.Master_dbConnection).Close();

            }
        }


        public static T ReadUsingMarshalUnsafe<T>(byte[] data)
            where T : struct
        {
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);

            try
            {
                return Marshal.PtrToStructure<T>(
                    handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }

    }
}




