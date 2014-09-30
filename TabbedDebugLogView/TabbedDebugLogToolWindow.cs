using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using System.Diagnostics;
using Microsoft.VisualStudio.Debugger.Interop;

namespace DarknessvsLightness.TabbedDebugLogView
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid("940a189d-a66d-4c5c-b63d-f22671b016ea")]
    public class TabbedDebugLogToolWindow : ToolWindowPane, IVsDebuggerEvents, IDebugEventCallback2
    {
        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public TabbedDebugLogToolWindow() :
            base(null)
        {
            // Set the window title reading it from the resources.
            this.Caption = Resources.ToolWindowTitle;
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
            // the object returned by the Content property.
            base.Content = new TabbedLogControl();

            m_previousDebuggerMode = DBGMODE.DBGMODE_Design;//Assume we are in design mode to start with.
            IVsDebugger debugService = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsShellDebugger)) as IVsDebugger;
            if (debugService != null)
            {
                debugService.AdviseDebuggerEvents(this, out cookie);
                debugService.AdviseDebugEventCallback(this);
            }
        }

        ~TabbedDebugLogToolWindow()
        {
            IVsDebugger debugService = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsShellDebugger)) as IVsDebugger;
            if (debugService != null)
            {
                debugService.UnadviseDebuggerEvents(cookie);
                debugService.UnadviseDebugEventCallback(this);
            }

            Dispose(false);
        }

        public int OnModeChange(DBGMODE dbgmodeNew)
        {
            switch (dbgmodeNew)
            {
                case DBGMODE.DBGMODE_Design:
                    {
                    }
                    break;
                case DBGMODE.DBGMODE_Run:
                    {
                        if (m_previousDebuggerMode == DBGMODE.DBGMODE_Design)
                        {
                            //Reset all the tab outputs here
                            ((TabbedLogControl)(base.Content)).ResetOutputTabs();
                        }
                        Debug.WriteLine("Moving to Run mode from, {0}", m_previousDebuggerMode);
                    }
                    break;
            }

            m_previousDebuggerMode = dbgmodeNew;
            return 0;
        }

        public int Event(IDebugEngine2 pEngine, IDebugProcess2 pProcess, IDebugProgram2 pProgram, IDebugThread2 pThread, IDebugEvent2 pEvent, ref Guid riidEvent, uint dwAttrib)
        {
            var outputString = pEvent as IDebugOutputStringEvent2;
            if (outputString != null)
            {
                string newDebugString;
                newDebugString = "";
                if (outputString.GetString(out newDebugString) == 0)
                { 
                    ((TabbedLogControl)(base.Content)).ReceivedString(newDebugString);
                }
            }
            return 0;
        }

        private UInt32 cookie;
        private DBGMODE m_previousDebuggerMode;
    }
}
