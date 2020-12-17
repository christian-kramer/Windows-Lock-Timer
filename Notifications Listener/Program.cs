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

namespace Notifications_Listener
{
    class Program
    {
        static void Toast(List<string> textLines, string notificationGroup)
        {

            XmlDocument template = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);

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
                            var textLines = new List<string>();
                            textLines.Add(arguments.lockedTitle);
                            textLines.Add(arguments.lockedMessage + packet.Count.ToString() + " minutes");

                            Toast(textLines, arguments.notificationGroup);
                        }
                    }
                }
            }

        }
    }
}
