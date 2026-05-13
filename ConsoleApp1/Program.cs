using ConsoleApp1.Helpers;
using ConsoleApp1.Processes;
using System;

namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var segments = ConfigHelper.GetSegments();

            foreach (var segment in segments)
            {
                switch (segment)
                {
                    //case "GroupsMembers":

                    //    var process = new GroupsMembersProcess();
                    //    var result = process.Execute();
                    //    Console.WriteLine(result.message);
                    //    break;
                    case "NSESecurity":
                        var nseProcess = new NseSecuritySegmentProcess();
                        var nseResult = nseProcess.ExecuteNseSecurity();
                        Console.WriteLine(nseResult.message);
                        break;
                }
            }

            Console.ReadLine();
        }
    }
}