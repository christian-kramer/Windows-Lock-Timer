using System;
using System.Linq;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading;

namespace Notifications_Listener
{
    class Program
    {
        static void Toast(List<string> textLines, string notificationGroup, bool alarm = false)
        {

            XmlDocument template = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);

            if (alarm)
            {
                /* Handle setting toast duration to "long" */
                IXmlNode toastNode = template.SelectSingleNode("toast");
                XmlAttribute duration = template.CreateAttribute("duration");
                duration.Value = "long";
                toastNode.Attributes.SetNamedItem(duration);


                XmlElement audioElement = template.CreateElement("audio");
                /* Source Attribute */
                XmlAttribute audioSource = template.CreateAttribute("src");
                audioSource.Value = "ms-winsoundevent:Notification.Looping.Alarm3";
                audioElement.Attributes.SetNamedItem(audioSource);
                /* Loop Attribute */
                XmlAttribute audioLoop = template.CreateAttribute("loop");
                audioLoop.Value = "true";
                audioElement.Attributes.SetNamedItem(audioLoop);

                toastNode.AppendChild(audioElement);
            }

            //This absolutely needs to be built programatically
            //XmlDocument template = new XmlDocument();
            //template.LoadXml("<toast duration=\"long\"><visual><binding template=\"ToastText02\"><text id=\"1\"></text><text id=\"2\"></text></binding></visual><audio src=\"ms-winsoundevent:Notification.Looping.Alarm3\" loop=\"true\"/></toast>");


            /* Begin add text lines */
            byte lineIndex = 0;
            var textNodes = template.GetElementsByTagName("text").ToList();
            foreach (var textNode in textNodes)
            {
                textNode.AppendChild(template.CreateTextNode(textLines[lineIndex++]));
            }
            /* End add text lines */


            /* Not working, ignore
            var content = new ToastContentBuilder()
                .AddToastActivationInfo("picOfHappyCanyon", ToastActivationType.Foreground)
                .AddText("Andrew sent you a picture")
                .AddText("Check this out, Happy Canyon in Utah!")
                .GetToastContent();
            */


            var toast = new ToastNotification(template);
            toast.Tag = "Notifications Listener";
            toast.Group = "Windows Lock Timer";
            toast.ExpirationTime = DateTimeOffset.Now.AddMinutes(5);

            var notifier = ToastNotificationManager.CreateToastNotifier(notificationGroup);
            notifier.Show(toast);
        }
        static void Main(string[] args)
        {
            /* Unnecessary
            DesktopNotificationManagerCompat.RegisterActivator<MyNotificationActivator>();
            DesktopNotificationManagerCompat.RegisterAumidAndComServer<MyNotificationActivator>("6B29FC40-CA47-1067-B31D-00DD010662DA");
            */

            ArgumentParser arguments = new ArgumentParser(args);

#if DEBUG
            arguments.port = 31205;
#endif

            TcpListener server = new TcpListener(IPAddress.Any, arguments.port);
            TcpClient client = default;

            server.Start();

            while (true)
            {
                client = server.AcceptTcpClient();

                byte[] receivedBuffer = new byte[256];
                NetworkStream stream = client.GetStream();

                bool read = true;
                try
                {
                    stream.Read(receivedBuffer, 0, receivedBuffer.Length);
                }
                catch (System.IO.IOException e)
                {
                    //
                    read = false;
                    client.GetStream().Close();
                    client.Close();
                }

                if (read)
                {
                    string msg = Encoding.ASCII.GetString(receivedBuffer, 0, receivedBuffer.Length);

                    Windows_Lock_Timer.TimerPacket packet = new Windows_Lock_Timer.TimerPacket();

                    bool validPacket = true;
                    try
                    {
                        packet = JsonConvert.DeserializeObject<Windows_Lock_Timer.TimerPacket>(msg);
                    }
                    catch (JsonReaderException e)
                    {
                        //
                        validPacket = false;
                        Console.Write(msg + "\r\n");
                        Console.WriteLine("Invalid JSON");
                    }

                    if (validPacket)
                    {
                        Console.WriteLine("Valid JSON");

                        if (packet.ID == 0)
                        {
#if DEBUG
                            arguments.lockedTitle = "PC Locked";
                            arguments.lockedMessage = "Please set timer for ";
                            arguments.notificationGroup = "Debug Notifications";
#endif
                            /* Initial Toast warning of Lock Event */
                            var lockTextLines = new List<string>();
                            lockTextLines.Add(arguments.lockedTitle);
                            lockTextLines.Add(arguments.lockedMessage + packet.Count.ToString() + " minutes");

                            Toast(lockTextLines, arguments.notificationGroup);


                            /* Timed Delay */
                            DateTime alarmTime = DateTime.Now.AddMinutes(packet.Count);
                            while (DateTime.Now < alarmTime) { Thread.Sleep(1); }

                            /* Second Toast warning of Timer Expiry Event */
                            var alarmTextLines = new List<string>();
                            alarmTextLines.Add("Time's Up!");
                            alarmTextLines.Add("PC may be unlocked");

                            Toast(alarmTextLines, arguments.notificationGroup, true); //The trailing "true" makes this an alarm toast

                            /* Everything below this is non-functional for this context
                            ToastContent toastContent = new ToastContent()
                            {
                                Scenario = ToastScenario.Alarm,
                                Visual = new ToastVisual()
                                {
                                    BindingGeneric = new ToastBindingGeneric()
                                    {
                                        Children =
                                        {
                                            new AdaptiveText()
                                            {
                                                Text = "yeeeet"
                                            },

                                            new AdaptiveText()
                                            {
                                                Text = "boi"
                                            }

                                        }
                                    }
                                },
                                Actions = new ToastActionsCustom()
                                {
                                    Buttons =
                                    {
                                        new ToastButtonSnooze(),
                                        new ToastButtonDismiss()
                                    }
                                }
                            };
                            DateTime alarmTime = DateTime.Now.AddSeconds(15);
                            var scheduledNotif = new ScheduledToastNotification(
                                toastContent.GetXml(), // Content of the toast
                                alarmTime // Time we want the toast to appear at
                            );

                            DesktopNotificationManagerCompat.CreateToastNotifier().AddToSchedule(scheduledNotif);
                            */
                        }
                    }
                }
            }

        }
    }
}
