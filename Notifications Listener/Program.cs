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
            arguments.lockedTitle = "Test Title";
            arguments.lockedMessage = "Test Lock Message";
            arguments.notificationGroup = "Debug Notifications";
            arguments.port = 31205;
#endif
            var textLines = new List<string>();
            textLines.Add(arguments.lockedTitle);
            textLines.Add(arguments.lockedMessage);


            TcpListener server = new TcpListener(IPAddress.Any, arguments.port);
            TcpClient client = default;

            server.Start();

            while (true)
            {
                client = server.AcceptTcpClient();

                byte[] receivedBuffer = new byte[256];
                NetworkStream stream = client.GetStream();

                stream.Read(receivedBuffer, 0, receivedBuffer.Length);

                string msg = Encoding.ASCII.GetString(receivedBuffer, 0, receivedBuffer.Length);

                TimerPacket packet = new TimerPacket();

                bool validPacket = true;
                try
                {
                    packet = JsonConvert.DeserializeObject<TimerPacket>(msg);
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
                    Toast(textLines, arguments.notificationGroup);
                }
            }

        }
    }
}
