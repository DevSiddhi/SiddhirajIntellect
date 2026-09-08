using System.Runtime.InteropServices;

namespace JIO_QH.Models
{
    public class StatusMessage
    {
        public string message { get; set; }

        public bool status { get; set; } = true;
    }

    public class Holidaydata 
    {
        public DateTime HolidayDate { get; set; }
        public string ? Day_Name { get; set; }
        public string ? Details { get; set; }
        public int IsHoliday { get; set; }
        public int SrNo { get; set; }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TGroupRecord
    {
        public char GrType;
        public byte GrCodeLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public char[] GrCode;
        public byte GrNameLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
        public char[] GrName;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TMemberRecord
    {
        public char Exch;
        public char ExchType;
        public int Code;
        public byte NameLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public char[] Name;
        public char GrType;
        public byte GrCodeLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public char[] GrCode;
    }
    public class CompanyMasterReply
    {
        public bool success { get; set; }
        public List<CoCodeMaster> data { get; set; }
    }
    public class CoCodeMaster
    {
        private string _sectorname = null;
        public double co_code { get; set; }
        public string bsecode { get; set; }
        public string nsesymbol { get; set; }
        public string companyname { get; set; }
        public string companyshortname { get; set; }
        public string categoryname { get; set; }
        public string isin { get; set; }
        public string bsegroup { get; set; }
        public string mcaptype { get; set; }
        public string sectorcode { get; set; }
        public string sectorname
        {
            get => _sectorname;
            set => _sectorname = value?.Replace(',', '-');
        }
        public string industrycode { get; set; }
        public string industryname { get; set; }
        public string bselistedflag { get; set; }
        public string nselistedflag { get; set; }
        public string DisplayType { get; set; }
    }

    struct bseFilesData
    {
        public decimal YearlyHigh;
        public decimal YearlyLow;
        public decimal UpperCkt;
        public decimal LowerCkt;
        public decimal PClose;
        public int YearlyHighDate;
        public int YearlyLowDate;
        public string SurveillanceMsg;
        public char SurveillanceFlag;
    }
    public struct bseIndexData
    {
        public decimal PrevOpen;
        public decimal PrevHigh;
        public decimal PrevLow;
        public decimal PrevClose;
        public decimal PrevDayClose;
    }
    public class ScripData_Dict
    {
        public Dictionary<string, List<ScripData>> scripNameData = new Dictionary<string, List<ScripData>>();

        public void addEntry(ScripData Data)
        {
            if (Data.Name != null && Data.Name != "")
            {
                if (!scripNameData.TryGetValue(Data.Name, out var scripData_EQs))
                {
                    scripData_EQs = new List<ScripData>();
                    scripNameData.Add(Data.Name, scripData_EQs);
                }
                scripData_EQs.Add(Data);
            }
        }

        public ScripData GetRecord(string Symbol, string Series)
        {
            if (scripNameData.TryGetValue(Symbol, out var scripList))
            {
                foreach (var item in scripList)
                {
                    var NseSeries = item.Series;
                    if (NseSeries == "" && Series != "") NseSeries = "EQ";
                    if (string.Compare(Series, NseSeries, true) != 0) continue;
                    return item;
                }
            }
            return null;
        }
        public ScripData GetRecord(string Symbol)
        {
            if (scripNameData.TryGetValue(Symbol, out var scripList))
            {
                return scripList[0];
            }
            return null;
        }
    }
    public class ScripData
    {
        public string Name;
        public string Series;
        public decimal PrevOpen;
        public decimal PrevHigh;
        public decimal PrevLow;
        public decimal PrevClose;
        public decimal PrevDayClose;
        public int PrevOI;
        public decimal YearlyHigh;
        public decimal YearlyLow;
        public int YearlyHighDate;
        public int YearlyLowDate;
        public string SurveillanceMsg = "";
        public char SurveillanceFlag = 'N';
    }

    class ScripDataForDictionary
    {
        public string Series { get; set; }
        public string Name { get; set; }
    }
}