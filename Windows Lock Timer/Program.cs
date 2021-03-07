using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Generic;

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
                Debug.WriteLine("event logging: " + message);
            }
        }

        static bool sendTCP(TimerPacket timerPacket, string ipString, int port)
        {
            TcpClient server;

            try
            {
                server = new TcpClient(ipString, port);
            }
            catch (SocketException)
            {
                //Console.WriteLine("Unable to connect to server");
                return false;
            }

            string input = JsonConvert.SerializeObject(timerPacket);
            NetworkStream ns = server.GetStream();
            ns.Write(Encoding.ASCII.GetBytes(input), 0, input.Length);
            ns.Flush();
            return true;
        }

        static void Main(string[] args)
        {
            /* Begin bootstrapping
            string appRoaming = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Windows Lock Timer");
            string settingsFile = Path.Combine(appRoaming, "Settings.ini");
            Directory.CreateDirectory(appRoaming);


            var Settings = new Settings();
            var MyIni = new IniFile(settingsFile);

            if (File.Exists(settingsFile))
            {
                Debug.WriteLine(Settings.Times.LockTime);
            }
            else
            {
                foreach (PropertyInfo firstLevel in typeof(Settings).GetProperties())
                {
                    if (firstLevel.Name == firstLevel.PropertyType.Name)
                    {
                        //this be an object, loop it
                        Debug.WriteLine(firstLevel.Name);
                        foreach (PropertyInfo secondLevel in Type.GetType(typeof(Settings).Namespace + "." + firstLevel.Name).GetProperties())
                        {
                            Debug.WriteLine(secondLevel.Name);
                            Debug.WriteLine(Type.GetType(typeof(Settings).Namespace + "." + secondLevel.Name).GetProperty(secondLevel.Name).GetValue(Settings));
                            MyIni.Write(secondLevel.Name, "disavalue", firstLevel.Name);
                        }
                    }
                    else
                    {
                        //this is top-level boiz, dump it in the top config
                        MyIni.Write(firstLevel.Name, "disavalue");
                    }
                }
            }
            



            End bootstrapping */

            ArgumentParser arguments = new ArgumentParser(args);
            UsageSession usageSession = new UsageSession();

            DateTime lastLockTime = DateTime.MinValue;

#if DEBUG
            arguments.lockTime = 3;
            arguments.warningTime = 1;
            arguments.cooldownTime = 2;
            arguments.port = 31205;
            arguments.warningMessage = "Debug warning message";
            Console.WriteLine("Running in Debug Mode");
#endif

            void startSession(string reason)
            {
                //establish default reason
                usageSession.reason = "user";
                usageSession.active = true;
                usageSession.warned = false;
                usageSession.expiry = DateTime.Now.AddMinutes(arguments.lockTime); //MAKE THIS CONFIGURABLE
                eventLog(reason, 1);

                /* Begin Messaging */

                /*
                string hostFilePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "hosts");
                if (File.Exists(hostFilePath))
                {
                    Debug.WriteLine("Heyo file exists");
                    string hostFileContents = File.ReadAllText(hostFilePath);
                    foreach (string host in hostFileContents.Split('\n'))
                    {
                        string trimmedHost = host.Trim();

                        new Thread(() =>
                        {

                            TimerPacket timerPacket = new TimerPacket();
                            timerPacket.Message = reason;
                            timerPacket.Cooldown = arguments.cooldownTime;

                            if (sendTCP(timerPacket, trimmedHost, arguments.port))
                            {
                                Debug.WriteLine(trimmedHost + " totally worked");
                            }
                            else
                            {
                                Debug.WriteLine("TCP did not send to " + trimmedHost);
                            }

                        }).Start();
                    }
                }
                */

                /* End messaging */
            }

            void sendTimerPacket(int id, int count)
            {
                string hostFilePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "hosts");
                if (File.Exists(hostFilePath))
                {
                    Debug.WriteLine("Heyo file exists");
                    string hostFileContents = File.ReadAllText(hostFilePath);
                    foreach (string host in hostFileContents.Split('\n'))
                    {
                        string trimmedHost = host.Trim();

                        new Thread(() =>
                        {

                            TimerPacket timerPacket = new TimerPacket();
                            timerPacket.ID = id; // ID 0 represents "Forcefully Locked", might change this later
                            timerPacket.Count = count;

                            if (sendTCP(timerPacket, trimmedHost, arguments.port))
                            {
                                Debug.WriteLine(trimmedHost + " totally worked");
                            }
                            else
                            {
                                Debug.WriteLine("TCP did not send to " + trimmedHost);
                            }

                        }).Start();
                    }
                }
            }

            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);

            void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
            {
                if (e.Reason == SessionSwitchReason.SessionLock)
                {
                    //Computer was locked, either by user or script
                    eventLog("locked because of " + usageSession.reason, 1);
                    usageSession.active = false;

                    bool beforeExpiry = DateTime.Compare(DateTime.Now, usageSession.expiry) < 0; //"now" is earlier than expiry, true/false
                    bool withinBullshitRange = DateTime.Compare(DateTime.Now.AddMinutes(1), usageSession.expiry) > 0; //"now" is within x minutes of expiry, true/false
                    bool reasonIsUser = usageSession.reason == "user";
                    
                    if (beforeExpiry && withinBullshitRange && reasonIsUser && usageSession.warned)
                    {
                        //this means that the computer was locked manually less than 5 minutes before the expiration. BULLSHIT DETECTED
                        Debug.WriteLine("Bullshit detected!!"); //It is at this moment that a lock command should be given.
                        sendTimerPacket(0, arguments.cooldownTime/*+ arguments.warningTime*/);
                        //SystemSounds.Exclamation.Play();
                    }
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

                    if (DateTime.Compare(lastLockTime, DateTime.MinValue) > 0)
                    {
                        DateTime thisTime = DateTime.Now.AddMinutes(0 - arguments.cooldownTime);
                        int tooSoonComparisonResult = DateTime.Compare(lastLockTime, thisTime);
                        if (tooSoonComparisonResult >= 0)
                        {
                            Debug.WriteLine("Less than 10 seconds has elapsed");
                            Thread.Sleep(250); //This is important so that the messagebox shows *after* the windows unlock animation
                            int timeAmount = (int)((lastLockTime - thisTime).TotalSeconds);
                            string timeUnit = (timeAmount > 60) ? " minutes" : " seconds";
                            timeAmount = (timeAmount > 60) ? (int)(timeAmount / 60) : timeAmount;
                            string timePhrase = timeAmount.ToString() + timeUnit;

                            DialogResult dialogResult = MessageBox.Show("There is still " + timePhrase + " on the Cooldown Timer. Unlock Anyway?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2, MessageBoxOptions.DefaultDesktopOnly);

                            //Here should be where we focus to messagebox

                            if (dialogResult == DialogResult.Yes)
                            {
                                //do something
                                Debug.WriteLine("Messagebox was 'yes'");
                            }
                            else if (dialogResult == DialogResult.No)
                            {
                                try
                                {
                                    usageSession.reason = "cooldown not met";
                                    Process.Start("rundll32.exe", "user32.dll,LockWorkStation");
                                    eventLog("Denied entry, telling PC to lock", 1);
                                }
                                catch
                                {
                                    eventLog("Failed to lock", 2);
                                }
                            }
                        }
                    }

                }
                else if (e.Reason == SessionSwitchReason.SessionLogon)
                {
                    //User has been allowed in, possibly for the first time of the day
                    startSession("logged in");
                }
            }


            startSession("logged in (start)");

            /*
            Form f = new Form();
            f.StartPosition = FormStartPosition.CenterScreen;
            f.Size = new Size(500, 200);
            f.FormBorderStyle = FormBorderStyle.FixedSingle;
            f.MaximizeBox = false;
            f.MinimizeBox = false;
            f.Icon = SystemIcons.Exclamation;
            f.Text = "Timer Cooldown Not Exceeded";

            Label t = new Label();
            t.TextAlign = ContentAlignment.MiddleCenter;
            t.Text = "00:00";
            t.Font = new Font("Arial", 24, FontStyle.Bold);
            t.Size = new Size(400, 30);
            t.Left = (f.ClientSize.Width - t.Width) / 2;
            t.Top = ((f.ClientSize.Height - t.Height) / 2) - 50;
            f.Controls.Add(t);


            ProgressBar p = new ProgressBar();
            p.Location = new Point(10, 10);
            p.Size = new Size(400, 30);
            p.Left = (f.ClientSize.Width - p.Width) / 2;
            p.Top = (f.ClientSize.Height - p.Height) / 2;
            p.Style = ProgressBarStyle.Continuous;
            p.Maximum = 100;
            p.Step = 1;
            p.Value = 30;
            f.Controls.Add(p);
            
            Label m = new Label();
            m.TextAlign = ContentAlignment.MiddleCenter;
            m.Text = "There is still time left on the cooldown timer.\nUnlock Anyway?";
            //t.Font = new Font("Arial", 24, FontStyle.Bold);
            m.Size = new Size(400, 30);
            m.Left = (f.ClientSize.Width - t.Width) / 2;
            m.Top = ((f.ClientSize.Height - t.Height) / 2) + 50;
            f.Controls.Add(m);

            Button folderButton = new Button();
            folderButton.Width = 50;
            folderButton.Height = 14;
            folderButton.ForeColor = Color.Black;
            folderButton.Text = "Yes";
            f.Controls.Add(folderButton);

            f.ShowDialog();
            */

            //This script is ran at the time the computer turns on.

            var loop1Task = Task.Run(async () => {

                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    //Console.WriteLine(usageSession.active.ToString());
                    if (usageSession.active)
                    {
                        //Console.WriteLine("looping");
                        int warningComparisonResult = DateTime.Compare(DateTime.Now, usageSession.expiry.AddMinutes(0 - arguments.warningTime));
                        int expiryComparisonResult = DateTime.Compare(DateTime.Now, usageSession.expiry);

                        if ((usageSession.warned == false) && (warningComparisonResult >= 0))
                        {
                            //Console.WriteLine("doing the shutdown");
                            Process.Start("shutdown", "-s -t 60 -c \"" + arguments.warningMessage + "\"");
                            usageSession.warned = true;
                            Thread.Sleep(5000);
                            Process.Start("shutdown", "-a");
                        }

                        if (expiryComparisonResult >= 0)
                        {
                            //Console.WriteLine("locking");
                            lastLockTime = DateTime.Now;

                            try
                            {
                                usageSession.reason = "script";
                                Process.Start("rundll32.exe", "user32.dll,LockWorkStation");
                                eventLog("Time's up, telling PC to lock", 1);
                            }
                            catch
                            {
                                eventLog("Failed to lock", 2);
                            }

                            usageSession.active = false;
                            sendTimerPacket(0, arguments.cooldownTime);
                            /*
                            string hostFilePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "hosts");
                            if (File.Exists(hostFilePath))
                            {
                                Debug.WriteLine("Heyo file exists");
                                string hostFileContents = File.ReadAllText(hostFilePath);
                                foreach (string host in hostFileContents.Split('\n'))
                                {
                                    string trimmedHost = host.Trim();

                                    new Thread(() =>
                                    {

                                        TimerPacket timerPacket = new TimerPacket();
                                        timerPacket.ID = 0; // ID 0 represents "Forcefully Locked", might change this later
                                        timerPacket.Count = arguments.cooldownTime;

                                        if (sendTCP(timerPacket, trimmedHost, arguments.port))
                                        {
                                            Debug.WriteLine(trimmedHost + " totally worked");
                                        }
                                        else
                                        {
                                            Debug.WriteLine("TCP did not send to " + trimmedHost);
                                        }

                                    }).Start();
                                    /*
                                    IPAddress hostAddress;

                                    if (IPAddress.TryParse(trimmedHost, out hostAddress))
                                    {
                                        Debug.WriteLine(hostAddress.ToString() + " is an IP address");
                                    }
                                    else
                                    {
                                        IPHostEntry yeet;
                                        bool success = true;
                                        try
                                        {
                                            yeet = Dns.GetHostEntry(trimmedHost);
                                            Debug.WriteLine(yeet.AddressList[0]);
                                        }
                                        catch
                                        {
                                            //Debug.WriteLine("Failed to resolve " + trimmedHost);
                                            success = false;
                                        }

                                        if (success)
                                        {
                                            Debug.WriteLine("successfully resolved host " + trimmedHost);
                                        }
                                    }
                                    */ /*
                                }
                            }
                            */

                            /* Begin TCP communication 

                            TcpClient server;
                            TimerPacket timerPacket = new TimerPacket();


                            timerPacket.Message = "this is my broadcast text";
                            timerPacket.Count = 20;


                            try
                            {
                                server = new TcpClient("localhost", 1337);
                            }
                            catch (SocketException)
                            {
                                Console.WriteLine("Unable to connect to server");
                                return;
                            }

                            string input = JsonConvert.SerializeObject(timerPacket);
                            NetworkStream ns = server.GetStream();
                            ns.Write(Encoding.ASCII.GetBytes(input), 0, input.Length);
                            ns.Flush();

                             End TCP communication */
                        }
                    }
                }
            });

            loop1Task.Wait();
        }
    }
}
