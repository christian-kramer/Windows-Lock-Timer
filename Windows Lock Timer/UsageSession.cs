﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Windows_Lock_Timer
{
    class UsageSession
    {
        public bool active;
        public bool warned;
        public string reason;
        public DateTime expiry;
    }
}
