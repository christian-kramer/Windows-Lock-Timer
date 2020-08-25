using Microsoft.Win32;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace Windows_Lock_Timer
{
    class Program
    {
        static void Main(string[] args)
        {

            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);

            void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
            {
                if (e.Reason == SessionSwitchReason.SessionLock)
                {
                    //Computer was locked, either by user or script
                    Console.WriteLine("locked");
                }
                else if (e.Reason == SessionSwitchReason.SessionLogoff)
                {
                    //User has logged off... this is of their own doing
                    Console.WriteLine("logged off");
                }
                else if (e.Reason == SessionSwitchReason.SessionUnlock)
                {
                    //User has been allowed back in
                    Console.WriteLine("unlocked");
                }
                else if (e.Reason == SessionSwitchReason.SessionLogon)
                {
                    //User has been allowed in, possibly for the first time of the day
                    Console.WriteLine("logged in");
                }
            }

            //This script is ran at the time the user logs in.

            /*
            var loop1Task = Task.Run(async () => {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            });

            loop1Task.Wait();
            */
        }
    }
}
