using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BinaryViewer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // The third-party HexBox control has internal bugs that can throw during key
            // processing (e.g. rapid resize + typing sequences). Catch anything unexpected
            // here so a single bad keystroke can't kill the app and lose unsaved edits.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => ReportUnhandledException(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => ReportUnhandledException(e.ExceptionObject as Exception);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // Windows launches the app with a dropped file's path as args[0] when a user
            // drags a file onto the app's icon (desktop shortcut, taskbar, or Explorer).
            Application.Run(new MainForm(args.Length > 0 ? args[0] : null));
        }

        private static void ReportUnhandledException(Exception ex)
        {
            MessageBox.Show(
                $"An unexpected error occurred and has been recovered from. Your in-memory edits are preserved, but consider saving now.\n\nDetails: {ex?.Message}",
                "Unexpected Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
}
