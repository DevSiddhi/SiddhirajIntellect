using ConsoleApp1.Helpers;
using ConsoleApp1.Processes;
using ConsoleApp1.Proto;
using ConsoleApp1.QH;
using System;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MainQH.StartUpQH();  //Starting of QH
            var segments = ConfigHelper.GetEnabledSegments();
            foreach (var segment in segments)
            {
                switch (segment)
                {
                    case "GroupsMembers":

                        var process = new GroupsMembersProcess();
                        var result = process.Execute();
                        Console.WriteLine(result.message);
                        break;
                    case "NseEquity":
                        var nseEquityProcess = new NseSecuritySegmentProcess();
                        var nseEquityResult = nseEquityProcess.ExecuteNseSecurity();
                        Console.WriteLine(nseEquityResult.message);
                        break;
                    case "NseDerivative":
                        var nseDerivativeProcess = new NseDerivativeSegment();
                        var nseDerivativeResult = nseDerivativeProcess.ExecuteNseDerivative();
                        Console.WriteLine(nseDerivativeResult.message);
                        break;
                    case "BseEquity":
                        var bseEquitySegment = new BseEquitySegmentProcess();
                        var bseEquityResult = bseEquitySegment.ExecuteBseScrip();
                        Console.WriteLine(bseEquityResult.message);
                        break;
                    case "BseDerivative":
                        var bseDerivativeSegment = new BseDerivativeSegment();
                        var bseDerivativeResult = bseDerivativeSegment.ExecuteBSEDeriv();
                        Console.WriteLine(bseDerivativeResult.message);
                        break;
                }
            }
            
            Console.ReadLine();
        }
    }
}