using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Sws.Threading
{
    public class MonitorLock : ILock
    {
        private readonly object _lockingObject;

        public MonitorLock(object lockingObject)
        {
            _lockingObject = lockingObject;
        }

        public void Enter()
        {
            Monitor.Enter(_lockingObject);
        }

        public void Exit()
        {
            Monitor.Exit(_lockingObject);
        }
    }
}
