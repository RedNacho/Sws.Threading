using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Sws.Threading
{
    public class SafeFailingMonitorLock : MonitorLock, ISafeFailingLock
    {

        private readonly object _lockingObject;

        public SafeFailingMonitorLock(object lockingObject)
            : base(lockingObject)
        {
            if (lockingObject == null)
            {
                throw new ArgumentNullException("lockingObject");
            }

            _lockingObject = lockingObject;
        }

        public void Enter(ref bool lockTaken)
        {
            Monitor.Enter(_lockingObject, ref lockTaken);
        }

    }
}
