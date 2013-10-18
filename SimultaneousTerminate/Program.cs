using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SimultaneousTerminate
{
    public class Program
    {
        private const string KeepAlive = "KeepAlive";
        private const string KeepAliveClient = "KeepAliveClient";

        [STAThread]
        public static void Main(string[] args)
        {
            var pList = new List<Process>();
            pList.AddRange(Process.GetProcessesByName(KeepAlive));
            pList.AddRange(Process.GetProcessesByName(KeepAliveClient));
            foreach (var process in pList)
                process.Kill();
        }
    }
}
