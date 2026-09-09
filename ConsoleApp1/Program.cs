using JIO_QH.BhavCopy;
using JIO_QH.Helpers;
using JIO_QH.Models;
using JIO_QH.Processes;
using JIO_QH.Proto;
using JIO_QH.QH;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

                        GroupsAndMembersMaster groupsAndMembers = new GroupsAndMembersMaster();
                        var result = groupsAndMembers.ExecuteGroupsAndMembers();
                        Console.WriteLine(result.message);
                        break;
                    case "NseEquity":
                        NseEquityMaster nseEquity = new NseEquityMaster();
                        var nseEquityResult = nseEquity.ExecuteNseEquity();
                        Console.WriteLine(nseEquityResult.message);
                        break;
                    case "NseDerivative":
                        NseDerivativeMaster nseDerivative = new NseDerivativeMaster();
                        var nseDerivativeResult = nseDerivative.ExecuteNseDerivative();
                        Console.WriteLine(nseDerivativeResult.message);                
                        break;
                    case "BseEquity":
                        BseEquityMaster bseEquity = new BseEquityMaster();
                        var bseEquityResult = bseEquity.ExecuteBseEquity();
                        Console.WriteLine(bseEquityResult.message);
                        break;
                    case "BseDerivative":
                        BseDerivativeMaster bseDerivative = new BseDerivativeMaster();
                        var bseDerivativeResult = bseDerivative.ExecuteBseDerivative();
                        Console.WriteLine(bseDerivativeResult.message);
                        break;
                }
            }
            
            Console.ReadLine();
        }
    }
}