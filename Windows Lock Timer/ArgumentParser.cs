using System;
using System.Collections.Generic;
using System.Text;

namespace Windows_Lock_Timer
{
    class ArgumentParser
    {
        public string warningMessage;
        public int lockTime;
        public int cooldownTime;
        public int warningTime;
        public int port;
        public ArgumentParser(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-t")
                {
                    lockTime = int.Parse(args[i + 1]);
                }
                else if (args[i] == "-w")
                {
                    warningTime = int.Parse(args[i + 1]);
                }
                else if (args[i] == "-m")
                {
                    warningMessage = args[i + 1];
                }
                else if (args[i] == "-c")
                {
                    cooldownTime = int.Parse(args[i + 1]);
                }
                else if (args[i] == "-p")
                {
                    port = int.Parse(args[i + 1]);
                }
            }


            if (lockTime.ToString() == "0")
            {
                lockTime = 60;
            }

            if (warningTime.ToString() == "0")
            {
                warningTime = 2;
            }

            if (string.IsNullOrEmpty(warningMessage))
            {
                warningMessage = "This computer is set to forcefully lock soon.";
            }
        }
    }
}
