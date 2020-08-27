using System;
using System.Collections.Generic;
using System.Text;

namespace Windows_Lock_Timer
{
    class ArgumentParser
    {
        public string warningMessage;
        public int lockTime;
        public int warningTime;
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
            }

            if (String.IsNullOrEmpty(lockTime.ToString()))
            {
                lockTime = 60;
            }

            if (String.IsNullOrEmpty(warningTime.ToString()))
            {
                warningTime = 2;
            }

            if (String.IsNullOrEmpty(warningMessage))
            {
                warningMessage = "This computer is set to forcefully lock soon.";
            }
        }
    }
}
