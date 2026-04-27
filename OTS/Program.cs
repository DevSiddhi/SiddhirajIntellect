using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Threading;

namespace OTS
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomainOnUnhandledException);
            Application.Run(new Form1());
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Form1.ErrorLog(e.Exception);
            // SendReport(e.Exception);
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            // SendReport((Exception)unhandledExceptionEventArgs.ExceptionObject);
            Environment.Exit(0);
        }
    }
}
