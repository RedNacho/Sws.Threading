﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Threading
{
    public interface ILock
    {
        void Enter();
        void Exit();
    }
}
