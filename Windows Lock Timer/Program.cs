using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;


namespace Windows_Lock_Timer
{
    class Program
    {
        static void eventLog(string message, short type)
        {
            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = "Application";
                eventLog.WriteEntry(message, EventLogEntryType.Information, 420, type);
            }
        }
        static void Main(string[] args)
        {
            // Clean up all these comments
            ArgumentParser arguments = new ArgumentParser(args);
            UsageSession usageSession = new UsageSession();

            Console.WriteLine(arguments.lockTime);
            Console.WriteLine(arguments.warningTime);
            Console.WriteLine(arguments.warningMessage);

            void startSession(string reason)
            {
                //establish default reason
                usageSession.reason = "user";
                usageSession.active = true;
                usageSession.expiry = DateTime.Now.AddSeconds(600); //MAKE THIS CONFIGURABLE
                eventLog(reason, 1);
            }

            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);

            void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
            {
                if (e.Reason == SessionSwitchReason.SessionLock)
                {
                    //Computer was locked, either by user or script
                    eventLog("locked because of " + usageSession.reason, 1);
                    usageSession.active = false;
                }
                else if (e.Reason == SessionSwitchReason.SessionLogoff)
                {
                    eventLog("user has logged off", 1);
                    usageSession.active = false;
                }
                else if (e.Reason == SessionSwitchReason.SessionUnlock)
                {
                    //User has been allowed back in
                    startSession("unlocked");
                }
                else if (e.Reason == SessionSwitchReason.SessionLogon)
                {
                    //User has been allowed in, possibly for the first time of the day
                    startSession("logged in");
                }
            }

            //This script is ran at the time the computer turns on.

            var loop1Task = Task.Run(async () => {
                while (true)
                {

                    await Task.Delay(TimeSpan.FromSeconds(1));
                    if (usageSession.active)
                    {
                        int expiryComparisonResult = DateTime.Compare(DateTime.Now, usageSession.expiry);
                        if (expiryComparisonResult >= 0)
                        {
                            try
                            {
                                usageSession.reason = "script";
                                Process.Start("rundll32.exe", "user32.dll,LockWorkStation");
                            }
                            catch
                            {
                                eventLog("Failed to lock", 2);
                            }

                            usageSession.active = false;
                        }
                    }
                }
            });

            loop1Task.Wait();
        }
    }
}
