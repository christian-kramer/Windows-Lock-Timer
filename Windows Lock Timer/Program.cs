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
            UsageSession usageSession = new UsageSession();
            usageSession.reason = "user";

            void startSession(string reason)
            {
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
                    /*
                    Console.Write("locked at: ");
                    Console.WriteLine(DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"));
                    */


                    eventLog("locked because of " + usageSession.reason, 1);
                    usageSession.active = false;
                    usageSession.reason = "user";
                }
                else if (e.Reason == SessionSwitchReason.SessionLogoff)
                {
                    eventLog("user has logged off", 1);
                    usageSession.active = false;
                    usageSession.reason = "user";
                    //User has logged off... this is of their own doing
                    //Console.WriteLine("logged off");

                    //since the user decided to lock themselves, we need to cancel the task and dispose
                    //tokenSource2.Dispose();
                }
                else if (e.Reason == SessionSwitchReason.SessionUnlock)
                {
                    //User has been allowed back in
                    startSession("unlocked");

                    //Print stuffs
                    /*
                    Console.Write("unlocked at: ");
                    Console.WriteLine(DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"));
                    */


                    //eventLog("going to lock at: " + usageSession.expiry.ToString("dddd, dd MMMM yyyy HH:mm:ss"), 1);
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
                    //Console.WriteLine(usageSession.active.ToString());
                    if (usageSession.active)
                    {
                        int expiryComparisonResult = DateTime.Compare(DateTime.Now, usageSession.expiry);
                        //Console.WriteLine(expiryComparisonResult);
                        if (expiryComparisonResult >= 0)
                        {
                            /*
                            Console.Write("expiring at: ");
                            Console.WriteLine(DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"));
                            */

                            //eventLog("session has expired", 1);

                            try
                            {
                                usageSession.reason = "script";
                                Process.Start("rundll32.exe", "user32.dll,LockWorkStation");
                            }
                            catch
                            {
                                eventLog("Failed to lock", 2);
                                usageSession.reason = "user";
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
