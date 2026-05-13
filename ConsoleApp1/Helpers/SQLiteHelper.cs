using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace ConsoleApp1.Helpers
{
    public static class SQLiteHelper
    {
        public static SQLiteConnection Master_dbConnection;
        public static DateTime MastersDate;
        public static readonly DateTime ExchStartDate = new DateTime(1980, 1, 1);

        public static void OpenSqlite()
        {
            try
            {
                if (Master_dbConnection == null)
                {
                    if (!File.Exists("Masters.mf"))
                        SQLiteConnection.CreateFile("Masters.mf");

                    if (!File.Exists("MastersServer.mf"))
                        SQLiteConnection.CreateFile("MastersServer.mf");
                    Master_dbConnection = new SQLiteConnection("Data Source=MastersServer.mf;Version=3;");
                }

                if (Master_dbConnection.State != ConnectionState.Open)
                {
                    Master_dbConnection.Open();
                    SQLiteCommand scom = new SQLiteCommand(Master_dbConnection);
                    scom.CommandText = "ATTACH 'Masters.mf' AS Child";
                    scom.ExecuteNonQuery();

                    using (var cmd = new SQLiteCommand("drop table if exists MasterDate", Master_dbConnection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new SQLiteCommand("create table MasterDate (CreationDate int)",Master_dbConnection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    MastersDate = DateTime.Now;
                    var CurDate = Convert.ToInt32((MastersDate - ExchStartDate).TotalSeconds);

                    using (var cmd = new SQLiteCommand($"insert into MasterDate(CreationDate) values ({CurDate})",Master_dbConnection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    CopySQLiteTabletoChild("MasterDate", "*");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static void CopySQLiteTabletoChild(string tableName,string Fields)
        {
            OpenSqlite();
            try
            {
                var Qry = $"drop table if exists Child.{tableName}";
                using (var cmd = new SQLiteCommand(Qry,Master_dbConnection))
                {
                    cmd.ExecuteNonQuery();
                }

                Qry = $"CREATE TABLE Child.{tableName} AS SELECT {Fields} FROM {tableName}";
                using (var cmd = new SQLiteCommand(Qry, Master_dbConnection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}