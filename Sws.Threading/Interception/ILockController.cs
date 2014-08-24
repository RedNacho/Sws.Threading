using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Threading.Interception
{
    public interface ILockController
    {
        void Enter(ILock theLock, ref bool lockTaken);
        void Exit(ILock theLock);
    }
}
