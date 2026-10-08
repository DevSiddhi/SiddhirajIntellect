using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace IntellectSharedClassLibrary
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct ISCL_HoldingEquityReply
    {
        public char Exch;
        public char ExchType;
        public int BseCode;
        public int NseCode;
        public byte ScripNameLength;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 50)]
        public string ScripName;
        public int TotalQty;
        public int SoldQty;
        public int PendingQty;
        public int BrokerBeniQty;
        public int ExchRcvQty;
        public int ExchRcvQty2;
        public int ClientDPQty;
        public int Collatral;
        public int FundedStockQty;
        public int FSCollateralQty;
        public int NonPOA;
        public double PrevClose;
        public double PurchasePrice;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] Reserves;
    }

}
