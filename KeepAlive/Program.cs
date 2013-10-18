using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace KeepAlive
{
    public class Program
    {
        private const string KeepAlive = "KeepAlive.exe";
        private const string KeepAliveClient = "KeepAliveClient.exe";
        private static readonly SortedSet<DateTime> RestartHistory = new SortedSet<DateTime>();

        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Length < 2)
                return;
            var action = args[0];

            if (action.Equals("launchclientandexit", StringComparison.OrdinalIgnoreCase))
            {
                LaunchAndExit(KeepAliveClient, string.Concat("keepaliveid ", args[1]));
                return;
            }

            if (action.Equals("launchselfandexit", StringComparison.OrdinalIgnoreCase))
            {
                LaunchAndExit(KeepAlive, string.Concat("clientid ", args[1]));
                return;
            }

            if (action.Equals("clientid", StringComparison.OrdinalIgnoreCase))
            {
                var processId = 0;
                Int32.TryParse(args[1], out processId);
                if (processId > 0) 
                    KeepingAlive(processId);
            }
        }

        private static void LaunchAndExit(string fileName, string parameters)
        {
            var process = Process.Start(fileName, parameters);
            if (process != null) Environment.Exit(process.Id);
        }

        private static void KeepingAlive(int processId)
        {
            while (true)
            {
                Process keepAliveProcess;
                if (processId == 0)
                {
                    var kamikazeProcess = Process.Start(KeepAlive, string.Concat("launchclientandexit ", Process.GetCurrentProcess().Id));
                    if (kamikazeProcess == null)
                        return;
                    kamikazeProcess.WaitForExit();
                    keepAliveProcess = Process.GetProcessById(kamikazeProcess.ExitCode);
                }
                else
                {
                    keepAliveProcess = Process.GetProcessById(processId);
                    processId = 0;
                }
                keepAliveProcess.WaitForExit();
                
                // If client failed more than 10 time within a 10 minutes stop restarting it
                var time = DateTime.Now;
                RestartHistory.Add(time);
                while (RestartHistory.Count > 0 && (RestartHistory.Min - time) > TimeSpan.FromMinutes(10))
                    RestartHistory.Remove(RestartHistory.Min);
                if (RestartHistory.Count >= 10)
                    return;
            }
        }
    }
}
