using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LockScreen
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 进程
        /// </summary>
        private Mutex mutex;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            bool mutexResult;

            // 第二个参数为 你的工程命名空间名。
            // out 给 ret 为 false 时，表示已有相同实例运行。
            string MutexName = (string)Application.Current.Resources["MutexName"];
            mutex = new Mutex(true, MutexName, out mutexResult);

            if (!mutexResult)
            {
                try
                {
                    Process currentProcess = Process.GetCurrentProcess();
                    // 获取本进程名称
                    string curProcessName = currentProcess.ProcessName;
                    int curProcessId = currentProcess.Id;

                    Process[] processes = System.Diagnostics.Process.GetProcesses();
                    foreach (Process item in processes)
                    {
                        if (curProcessId != item.Id &&
                            item.ProcessName.Equals(curProcessName, StringComparison.OrdinalIgnoreCase))
                        {
                            item.Kill();
                        }
                    }
                }
                catch
                {
                }
            }
        }
    }
}
