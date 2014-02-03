using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CliverSoft;

namespace FontInstaller
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //All of this craziness courtesy of http://www.codeproject.com/Articles/24066/Intercept-and-Manage-Windows-Originated-by-Third-p

            Application.EnableVisualStyles();
            string[] args = Environment.GetCommandLineArgs();

            try
            {
                if (args == null || !args.Any())
                    Application.Exit();

                if (args.Count() == 1 && args.First().EndsWith(".exe"))
                    Application.Exit();

                Form1 form = new Form1();
                //Start intercepting all dialog boxes owned by form
                var interceptor = new WindowInterceptor(IntPtr.Zero, ClickYesButton);
                form.Show();
                form.InstallFont(args.ElementAt(1));                         

                //Stop intercepting. Should be called to calm unmanaged code correctly
                interceptor.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Application.Exit();       
        }

        /// <summary>
        ///clicking "Yes" on confirm message box
        /// </summary>
        /// <param name="hwnd"></param>
        static void ClickYesButton(IntPtr hwnd)
        {
            //looking for button "Yes" within the intercepted window
            IntPtr h = (IntPtr)Win32.Functions.FindWindowEx(hwnd, IntPtr.Zero, "Button", "&Yes");
            if (h != IntPtr.Zero)
            {
                //clicking the found button
                Win32.Functions.PostMessage((IntPtr)h, (uint)Win32.Messages.WM_LBUTTONDOWN, 0, 0);
                Win32.Functions.PostMessage((IntPtr)h, (uint)Win32.Messages.WM_LBUTTONUP, 0, 0);
            }
        }
    }
}
