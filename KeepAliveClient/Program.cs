using System;
using System.Diagnostics;
using System.Threading;

namespace KeepAliveClient
{
    public class Program
    {
        private const string KeepAlive = "KeepAlive.exe";
        private static Process _keepAliveProcess;
        private static Mutex _instanceMutex;
        private static bool _exiting;
        private static int _processId;

        [STAThread]
        public static void Main(string[] args)
        {
            if (!SingleInstance())
                return;

            if (args.Length > 1 && String.Equals(args[0], "keepaliveid"))
                Int32.TryParse(args[1], out _processId);

            var thread = new Thread(KeepingAlive);
            thread.Start();

            while (true)
            {
                if (Console.ReadLine() != "exit") 
                    continue;
                _exiting = true;
                ReleaseSingleInstance();
                _keepAliveProcess.Kill();
                Environment.Exit(0);
            }
        }
        
        private static void KeepingAlive()
        {
            while (true)
            {
                if (_exiting)
                    return;

                if (_processId == 0)
                {
                    var kamikazeProcess = Process.Start(KeepAlive, string.Concat("launchselfandexit ", Process.GetCurrentProcess().Id));
                    if (kamikazeProcess == null)
                        return;
                    
                    kamikazeProcess.WaitForExit();
                    _keepAliveProcess = Process.GetProcessById(kamikazeProcess.ExitCode);
                }
                else
                {
                    _keepAliveProcess = Process.GetProcessById(_processId);
                    _processId = 0;
                }
                
                _keepAliveProcess.WaitForExit();
            }
        }

        private static bool SingleInstance()
        {
            bool createdNew;
            _instanceMutex = new Mutex(true, @"Local\4A31488B-F86F-4970-AF38-B45761F9F060", out createdNew);
            if (createdNew) return true;
            Debug.WriteLine("Application already launched. Shutting down.");
            _instanceMutex = null;
            return false;
        }

        private static void ReleaseSingleInstance()
        {
            if (_instanceMutex == null)
                return;
            _instanceMutex.ReleaseMutex();
            _instanceMutex.Close();
            _instanceMutex = null;
        }
    }
}
