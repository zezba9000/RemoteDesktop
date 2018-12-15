using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WavPlayer
{       
    /// <summary>
    /// Application main class.
    /// </summary>
    public static class Program
    {
        #region static method Main

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {   
            try{
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
                Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new wfrm_Main());
            }
            catch(Exception x){
                MessageBox.Show(x.ToString());
            }
        }

        #endregion

        #region static method CurrentDomain_UnhandledException

        private static void CurrentDomain_UnhandledException(object sender,UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(null,"Unhandled error: " + ((Exception)e.ExceptionObject).ToString(),"Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
        }

        #endregion

        #region static method Application_ThreadException

        private static void Application_ThreadException(object sender,System.Threading.ThreadExceptionEventArgs e)
        {
            MessageBox.Show(null,"Unhandled error: " + e.Exception.ToString(),"Error:",MessageBoxButtons.OK,MessageBoxIcon.Error);
        }

        #endregion
    }
}