using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sws.Threading
{
    public interface ISafeFailingLock : ILock
    {
        /// <summary>
        /// Enters the lock, setting the lockTaken value to true when the lock is acquired.
        /// If an exception is thrown but the lock is acquired beforehand, lockTaken should be set to true.
        /// If an exception is thrown before the lock is acquired, lockTaken should be set to false.
        /// If no exception occurs, the lock should always be acquired and lockTaken should be set to true.
        /// </summary>
        /// <param name="lockTaken"></param>
        void Enter(ref bool lockTaken);
    }
}
