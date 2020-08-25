using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;


namespace Windows_Lock_Timer
{
    class Program
    {
        static void Main(string[] args)
        {
            UsageSession usageSession = new UsageSession();

            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);

            void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
            {
                if (e.Reason == SessionSwitchReason.SessionLock)
                {
                    //Computer was locked, either by user or script
                    Console.WriteLine("locked at:");
                    usageSession.active = false;
                }
                else if (e.Reason == SessionSwitchReason.SessionLogoff)
                {
                    //User has logged off... this is of their own doing
                    Console.WriteLine("logged off");

                    //since the user decided to lock themselves, we need to cancel the task and dispose
                    //tokenSource2.Dispose();
                }
                else if (e.Reason == SessionSwitchReason.SessionUnlock)
                {
                    //User has been allowed back in
                    usageSession.active = true;
                    usageSession.expiry = DateTime.Now.AddSeconds(10); //MAKE THIS CONFIGURABLE

                    //Print stuffs
                    Console.Write("unlocked at: ");
                    Console.WriteLine(DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"));

                    Console.Write("going to lock at: ");
                    Console.WriteLine(usageSession.expiry.ToString("dddd, dd MMMM yyyy HH:mm:ss"));
                }
                else if (e.Reason == SessionSwitchReason.SessionLogon)
                {
                    //User has been allowed in, possibly for the first time of the day
                    Console.WriteLine("logged in");
                    //First, obviously, log this action.
                    //Second, we want to figure out "when is 60 minutes from now?"
                    //Third, we want to start checking periodically if the time has expired.
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
                        int comparisonResult = DateTime.Compare(DateTime.Now, usageSession.expiry);
                        Console.WriteLine(comparisonResult);
                        if (comparisonResult >= 0)
                        {
                            Console.Write("expiring at: ");
                            Console.WriteLine(DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss"));
                            try
                            {
                                Process.Start("rundll32.exe", "user32.dll,LockWorkStation");
                            }
                            catch
                            {
                                Console.WriteLine("Failed to lock");
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
