using System;
using System.Collections.Generic;
using System.Text;

namespace Notifications_Listener
{
    class ArgumentParser
    {
        public string lockedTitle;
        public string lockedMessage;
        public string notificationGroup;
        public int port;
        public ArgumentParser(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-m")
                {
                    lockedMessage = args[i + 1];
                }
                else if (args[i] == "-t")
                {
                    lockedTitle = args[i + 1];
                }
                else if (args[i] == "-g")
                {
                    notificationGroup = args[i + 1];
                }
                else if (args[i] == "-p")
                {
                    port = int.Parse(args[i + 1]);
                }
            }
        }
    }
}
