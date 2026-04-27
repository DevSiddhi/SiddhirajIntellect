using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OTS
{
    public class ChangeLog
    {
        public string chnagelog = @"

        sumit Pawaskar ---->    Activity Log brokerorderid and status added in structure--- case 202 ---->  02-march-2022

        sumit Pawaskar ---->    revert ots changes (sql string print on display )---->  05-march-2022

        Himanshu Soni ---->    Add Case for MTF Margin(217) and Holding Margin(218) ---->  11-march-2022
        
        sumit pawaskar--------> handle cancelled order in 78 case rtype = 'c' -----> 31-03-2022

        jeffin-------> Confirmation reply (chnages in addto pendinglist func  -----> 11-04-2022
           
        sumit pawaskar--------> //issue:  IN BSE EXCH 'B' was getting replace with 'S'(CASE 171)  for cover order   -----> 28-04-2022 

        Himanshu soni-------->  MTF Changes and build with pdb   -----> 12-09-2022 

        Himanshu Soni---------> Add FSCollateral and Funded Qty -------> 09-03-2023

        Siddhesh Mayekar------>IVR-OTP Changes - Added New Case 134,135,136 ---->16.03.23

        ";
    }
}
