using ConsoleApp1.Helpers;
using ConsoleApp1.Models;
using ConsoleApp1.QH;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ConsoleApp1.Processes
{
    public class GroupsMembersProcess
    {
        public static TGroupRecord[] Group = new TGroupRecord[10];
        public static TMemberRecord[] Member = new TMemberRecord[10];


        public StatusMessage Execute(bool checkFile = false)
        {
            string outputMessage = string.Empty;
            try
            {
                CommonFunctions.WriteToLogFile("GroupMembersExecute Started");
                Console.WriteLine("GroupMembersExecute Started.....");
                SQLiteHelper.OpenSqlite();

                if (checkFile)
                {
                    if (!File.Exists("GroupsI2.dat")) outputMessage += "GroupsI2.dat,";
                    if (!File.Exists("MembersI2.dat")) outputMessage += "MembersI2.dat,";

                    return new StatusMessage { message = outputMessage, status = string.IsNullOrEmpty(outputMessage) };
                }



                if (!File.Exists(Path.Combine(MainQH.MasterFolder, "GroupsI2.dat")))
                {
                    outputMessage += "GroupsI2.dat file not found,";
                    CommonFunctions.WriteToLogFile("GroupsI2.dat File Not Found");
                }

                if (!File.Exists(Path.Combine(MainQH.MasterFolder, "MembersI2.dat")))
                {
                    outputMessage += "MembersI2.dat file not found";
                    CommonFunctions.WriteToLogFile("MembersI2.dat File Not Found");
                }

                if (!string.IsNullOrEmpty(outputMessage))
                {
                    return new StatusMessage
                    {
                        message = outputMessage,
                        status = false
                    };
                }

                CommonFunctions.WriteToLogFile("Loading Groups");
                StatusMessage returnVal = LoadGroup();
                if (!returnVal.status) return returnVal;
                outputMessage = returnVal.message + Environment.NewLine;
                CommonFunctions.WriteToLogFile("Loading Members");
                
                
                returnVal = LoadMember();
                if (!returnVal.status) return returnVal;
                outputMessage += returnVal.message + Environment.NewLine;

                string DelQry ="DELETE FROM Members WHERE GrType in ('Y','M');" + "DELETE FROM Groups WHERE GrType in ('Y','M')";

                if (SQLiteHelper.Master_dbConnection.State == ConnectionState.Closed)
                {
                    SQLiteHelper.Master_dbConnection.Open();
                }

                using (var cmd = new SQLiteCommand(DelQry, SQLiteHelper.Master_dbConnection))
                {
                    cmd.ExecuteNonQuery();
                }

                SQLiteHelper.CopySQLiteTabletoChild("Groups", "*");
                CommonFunctions.ExportToCsv("child.Groups", "Groups.csv");
                CommonFunctions.WriteToLogFile("Groups Table Copied");


                SQLiteHelper.CopySQLiteTabletoChild("Members", "*");
                CommonFunctions.ExportToCsv("child.Members", "Members.csv");
                CommonFunctions.WriteToLogFile("Members Table Copied");
                
                
                SQLiteHelper.Master_dbConnection.Close();
                CommonFunctions.WriteToLogFile( "GroupMembersExecute Completed Successfully");

                return new StatusMessage { message = outputMessage, status = true};
            }
            catch (Exception ex)
            {
                CommonFunctions.ErrorLog(ex);

                return new StatusMessage
                {
                    message = ex.Message,
                    status = false
                };
            }
        }
        
        public StatusMessage LoadGroup()
        {
            int i, RecCnt1;

            try
            {
                CommonFunctions.WriteToLogFile("LoadGroup Started");

                FileStream GrFile1;
                string FName1;
                FName1 = Path.Combine(MainQH.MasterFolder, "GroupsI2.dat");
                CommonFunctions.WriteToLogFile("Checking Group File : " + FName1);

                if (File.Exists(FName1))
                {
                    GrFile1 = new FileStream(FName1, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                    CommonFunctions.WriteToLogFile("Group File Opened Successfully");
                }
                else
                {
                    GrFile1 = new FileStream(FName1, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                    CommonFunctions.WriteToLogFile("Group File Created");
                }

                if (SQLiteHelper.Master_dbConnection.State == ConnectionState.Closed)
                {
                    ((DbConnection)SQLiteHelper.Master_dbConnection).Open();
                    CommonFunctions.WriteToLogFile("SQLite Connection Opened");
                }

                string CheckGroupsMaster = "drop table if exists Groups";
                using (var cmd = new SQLiteCommand(CheckGroupsMaster, SQLiteHelper.Master_dbConnection))
                {
                    cmd.ExecuteNonQuery();
                }
                CommonFunctions.WriteToLogFile("Old Groups Table Dropped");

                string GroupsMaster = "create table Groups (GrType char, GrCode varchar(6), GrName varchar(30))";

                using (var cmd = new SQLiteCommand(GroupsMaster, SQLiteHelper.Master_dbConnection))
                {
                    cmd.ExecuteNonQuery();
                }

                CommonFunctions.WriteToLogFile("Groups Table Created");
                RecCnt1 = (int)GrFile1.Length / Marshal.SizeOf<TGroupRecord>();
                CommonFunctions.WriteToLogFile("Total Records Found : " + RecCnt1);
                byte[] Buff = new byte[Marshal.SizeOf<TGroupRecord>()];
                Array.Resize(ref Group, RecCnt1);
                using (var transaction = SQLiteHelper.Master_dbConnection.BeginTransaction())
                {
                    using (var cmd = new SQLiteCommand(SQLiteHelper.Master_dbConnection))
                    {
                        SQLiteParameter ParamGrType = new SQLiteParameter("@GrType", DbType.String);
                        SQLiteParameter ParamGrCode = new SQLiteParameter("@GrCode", DbType.String);
                        SQLiteParameter ParamGrName = new SQLiteParameter("@GrName", DbType.String);

                        cmd.CommandText =
                            "INSERT INTO Groups (GrType, GrCode, GrName) VALUES (@GrType,@GrCode,@GrName)";

                        cmd.Parameters.Add(ParamGrType);
                        cmd.Parameters.Add(ParamGrCode);
                        cmd.Parameters.Add(ParamGrName);
                        for (i = 0; i < RecCnt1; i++)
                        {
                            GrFile1.Read(Buff, 0, Marshal.SizeOf<TGroupRecord>());
                            Group[i] = CommonFunctions.ReadUsingMarshalUnsafe<TGroupRecord>(Buff);
                            ParamGrType.Value = Group[i].GrType;
                            ParamGrCode.Value = new string(Group[i].GrCode).Substring(0, Group[i].GrCodeLength);
                            ParamGrName.Value = new string(Group[i].GrName).Substring(0, Group[i].GrNameLength);
                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                }
                SQLiteHelper.Master_dbConnection.Close();
                GrFile1.Close();
                return new StatusMessage
                {
                    message = "Groups : " + RecCnt1 + " Records Processed",
                    status = true
                };
            }
            catch (Exception ex)
            {
                CommonFunctions.ErrorLog(ex);
                return new StatusMessage
                {
                    message = ex.Message,
                    status = false
                };
            }
        }

        public StatusMessage LoadMember()
        {
            try
            {
                int i, RecCnt1;
                FileStream MrFile1;
                string FName1;

                FName1 = Path.Combine(MainQH.MasterFolder, "MembersI2.dat");
                if (File.Exists(FName1))
                    MrFile1 = new FileStream(FName1, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                else
                    MrFile1 = new FileStream(FName1, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

                if (SQLiteHelper.Master_dbConnection.State == ConnectionState.Closed)
                {
                    ((DbConnection)SQLiteHelper.Master_dbConnection).Open();
                }
                string CheckMembersMaster = "drop table if exists Members";
                using (var cmd = new SQLiteCommand(CheckMembersMaster, SQLiteHelper.Master_dbConnection))
                {
                    cmd.ExecuteNonQuery();
                }
                string MembersMaster = "create table Members (Exch char, ExchType char,Code int, Name varchar(50),GrType char,GrCode varchar(6))";
                using (var cmd = new SQLiteCommand(MembersMaster, SQLiteHelper.Master_dbConnection))
                {
                    cmd.ExecuteNonQuery();
                }
                string NseDerivID = "Update Members set rowid=Code";
                using (var cmd = new SQLiteCommand(NseDerivID, SQLiteHelper.Master_dbConnection))
                {
                    cmd.ExecuteNonQuery();
                }
                RecCnt1 = (int)MrFile1.Length / Marshal.SizeOf<TMemberRecord>();
                byte[] Buff = new byte[Marshal.SizeOf<TMemberRecord>()];

                Array.Resize(ref Member, RecCnt1);
                using (var transaction = SQLiteHelper.Master_dbConnection.BeginTransaction())
                {
                    using (var cmd = new SQLiteCommand(SQLiteHelper.Master_dbConnection))
                    {
                        SQLiteParameter ParamExch = new SQLiteParameter("@Exch", DbType.String);
                        SQLiteParameter ParamExchType = new SQLiteParameter("@ExchType", DbType.String);
                        SQLiteParameter ParamCode = new SQLiteParameter("@Code", DbType.Int32);
                        SQLiteParameter ParamName = new SQLiteParameter("@Name", DbType.String);
                        SQLiteParameter ParamGrType = new SQLiteParameter("@GrType", DbType.String);
                        SQLiteParameter ParamGrCode = new SQLiteParameter("@GrCode", DbType.String);

                        cmd.CommandText = "INSERT INTO Members (Exch, ExchType,Code, Name,GrType,GrCode) VALUES (@Exch,@ExchType,@Code,@Name,@GrType,@GrCode)";
                        cmd.Parameters.Add(ParamExch);
                        cmd.Parameters.Add(ParamExchType);
                        cmd.Parameters.Add(ParamCode);
                        cmd.Parameters.Add(ParamName);
                        cmd.Parameters.Add(ParamGrType);
                        cmd.Parameters.Add(ParamGrCode);
                        for (i = 0; i < RecCnt1; i++)
                        {
                            MrFile1.Read(Buff, 0, Marshal.SizeOf<TMemberRecord>());
                            Member[i] = CommonFunctions.ReadUsingMarshalUnsafe<TMemberRecord>(Buff);
                            ParamExch.Value = Member[i].Exch;
                            ParamExchType.Value = Member[i].ExchType;
                            ParamCode.Value = Member[i].Code;
                            if (Member[i].NameLength > Member[i].Name.Length) ParamName.Value = new string(Member[i].Name).Trim();
                            else ParamName.Value = new string(Member[i].Name).Substring(0, Member[i].NameLength);
                            ParamGrType.Value = Member[i].GrType;
                            if (Member[i].GrCodeLength > Member[i].GrCode.Length)
                                ParamGrCode.Value = new string(Member[i].GrCode).Trim();
                            else ParamGrCode.Value = new string(Member[i].GrCode).Substring(0, Member[i].GrCodeLength);
                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                }
                SQLiteHelper.Master_dbConnection.Close();
                SQLiteHelper.CopySQLiteTabletoChild("Members", "*");
                MrFile1.Close();
                return new StatusMessage { message = "Members : " + RecCnt1 + " Records Processed", status = true };
            }
            catch (Exception ex)
            {
                CommonFunctions.ErrorLog(ex);
                return new StatusMessage { message = ex.Message, status = false };
            }
        }

    }

}