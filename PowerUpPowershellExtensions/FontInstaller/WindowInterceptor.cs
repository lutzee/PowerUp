//********************************************************************************************
//Author: Sergey Stoyan, CliverSoft Co.
//        stoyan@cliversoft.com
//        sergey.stoyan@gmail.com
//        http://www.cliversoft.com
//        07 September 2006
//Copyright: (C) 2006, Sergey Stoyan
//********************************************************************************************

using System;
using System.Runtime.InteropServices;

namespace FontInstaller
{
    /// <summary>
    /// Intercept creation of window and get its HWND
    /// </summary>
    public class WindowInterceptor
    {
        IntPtr hook_id = IntPtr.Zero;

        Functions.HookProc cbf;

        /// <summary>
        /// Delegate to process intercepted window 
        /// </summary>
        /// <param name="hwnd"></param>
        public delegate void ProcessWindow(IntPtr hwnd);
        ProcessWindow process_window;

        IntPtr owner_window = IntPtr.Zero;

        /// <summary>
        /// Start dialog box interception for the specified owner window
        /// </summary>
        /// <param name="owner_window">owner window, if it is IntPtr.Zero then any windows will be intercepted</param>
        /// <param name="process_window">custom delegate to process intercepted window. It should be a fast code in order to have no message stack overflow.</param>
        public WindowInterceptor(IntPtr owner_window, ProcessWindow process_window)
        {
            if (process_window == null)
                throw new Exception("process_window cannot be null!");
            this.process_window = process_window;

            this.owner_window = owner_window;

            cbf = new Functions.HookProc(dlg_box_hook_proc);
            //notice that win32 callback function must be a global variable within class to avoid disposing it!
            hook_id = Functions.SetWindowsHookEx(HookType.WH_CALLWNDPROCRET, cbf, IntPtr.Zero, Functions.GetCurrentThreadId());
        }

        /// <summary>
        /// Stop intercepting. Should be called to calm unmanaged code correctly
        /// </summary>
        public void Stop()
        {
            if (hook_id != IntPtr.Zero)
                Functions.UnhookWindowsHookEx(hook_id);
            hook_id = IntPtr.Zero;
        }

        ~WindowInterceptor()
        {
            if (hook_id != IntPtr.Zero)
                Functions.UnhookWindowsHookEx(hook_id);
        }

        private IntPtr dlg_box_hook_proc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
                return Functions.CallNextHookEx(hook_id, nCode, wParam, lParam);

            CWPRETSTRUCT msg = (CWPRETSTRUCT)Marshal.PtrToStructure(lParam, typeof(CWPRETSTRUCT));

            //filter out create window events only
            if (msg.message == (uint)Messages.WM_SHOWWINDOW)
            {
                int h = Functions.GetWindow(msg.hwnd, Functions.GW_OWNER);
                //check if owner is that is specified
                if (owner_window == IntPtr.Zero || owner_window == new IntPtr(h))
                {
                    if (process_window != null)
                        process_window(msg.hwnd);
                }
            }

            return Functions.CallNextHookEx(hook_id, nCode, wParam, lParam);
        }
    }
}
