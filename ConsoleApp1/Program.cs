using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using JIO_QH.BhavCopy;
using JIO_QH.Helpers;
using JIO_QH.Processes;
using JIO_QH.Proto;
using JIO_QH.QH;
using System;

namespace JIO_QH
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            var app = builder.Build();

            app.MapControllers();

            // Run API in background
            var apiTask = app.RunAsync("http://localhost:5000");

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
                        //var nseEquityBhavCopy = new NSECashBhavCopy();
                        //var nseEquityBhavCopyResult = nseEquityBhavCopy.NseEquityBhavCopyExecution();
                        //Console.WriteLine(nseEquityBhavCopyResult.message);
                        break;
                    case "NseDerivative":
                        var nseDerivativeProcess = new NseDerivativeSegment();
                        var nseDerivativeResult = nseDerivativeProcess.ExecuteNseDerivative();
                        Console.WriteLine(nseDerivativeResult.message);
                        //var nseDerivativeBhavCopy = new NSEDerivBhavCopy();
                        //var nseDerivativeBhavCopyResult = nseDerivativeBhavCopy.NseDerivBhavCopyExecution();
                        //Console.WriteLine(nseDerivativeBhavCopyResult.message);
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